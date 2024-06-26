﻿using System.Linq;
using DiscordCorePlugin.Templates;
using Oxide.Core.Plugins;
using Oxide.Ext.Discord.Constants;
using Oxide.Ext.Discord.Entities;
using Oxide.Ext.Discord.Extensions;

namespace DiscordCorePlugin.Plugins
{
    //Define:FileOrder=30
    public partial class DiscordCore
    {
        // ReSharper disable once UnusedMember.Local
        [HookMethod(DiscordExtHooks.OnDiscordGatewayReady)]
        private void OnDiscordGatewayReady(GatewayReadyEvent ready)
        {
            _bot = ready.User;
            
            DiscordGuild guild = null;
            if (ready.Guilds.Count == 1 && !_pluginConfig.GuildId.IsValid())
            {
                guild = ready.Guilds.Values.FirstOrDefault();
            }

            if (guild == null)
            {
                guild = ready.Guilds[_pluginConfig.GuildId];
                if (guild == null)
                {
                    PrintError("Failed to find a matching guild for the Discord Server Id. " +
                               "Please make sure your guild Id is correct and the bot is in the discord server.");
                    return;
                }
            }

            Guild = guild;
            
            DiscordApplication app = Client.Bot.Application;
            if (!app.HasApplicationFlag(ApplicationFlags.GatewayGuildMembersLimited) && !app.HasApplicationFlag(ApplicationFlags.GatewayGuildMembers))
            {
                PrintError($"You need to enable \"Server Members Intent\" for {Client.Bot.BotUser.Username} @ https://discord.com/developers/applications\n" +
                           $"{Name} will not function correctly until that is fixed. Once updated please reload {Name}.");
                return;
            }
            
            Puts($"Connected to bot: {_bot.Username}");
        }

        // ReSharper disable once UnusedMember.Local
        [HookMethod(DiscordExtHooks.OnDiscordBotFullyLoaded)]
        private void OnDiscordBotFullyLoaded()
        {
            RegisterTemplates();
            RegisterUserApplicationCommands();
            RegisterAdminApplicationCommands();
            _linkHandler.ProcessLeaveAndRejoin();
            SetupGuildWelcomeMessage();
            foreach (Snowflake role in _pluginConfig.PermissionSettings.LinkRoles)
            {
                if (!Guild?.Roles.ContainsKey(role) ?? false)
                {
                    PrintWarning($"`{role}` is set as the link role but role does not exist");
                }
            }
            
            foreach (Snowflake role in _pluginConfig.PermissionSettings.UnlinkRoles)
            {
                if (!Guild?.Roles.ContainsKey(role) ?? false)
                {
                    PrintWarning($"`{role}` is set as the unlink role but role does not exist");
                }
            }
        }

        // ReSharper disable once UnusedMember.Local
        [HookMethod(DiscordExtHooks.OnDiscordGuildMemberAdded)]
        private void OnDiscordGuildMemberAdded(GuildMemberAddedEvent member, DiscordGuild guild)
        {
            if (Guild?.Id != guild.Id)
            {
                return;
            }
            
            _linkHandler.OnGuildMemberJoin(member.User);
            if (!_pluginConfig.WelcomeMessageSettings.EnableWelcomeMessage || !_pluginConfig.WelcomeMessageSettings.SendOnGuildJoin)
            {
                return;
            }

            if (member.User.IsLinked())
            {
                return;
            }
            
            SendGlobalTemplateMessage(TemplateKeys.WelcomeMessage.PmWelcomeMessage, member.User);
        }
        
        // ReSharper disable once UnusedMember.Local
        [HookMethod(DiscordExtHooks.OnDiscordGuildMemberRemoved)]
        private void OnDiscordGuildMemberRemoved(GuildMemberRemovedEvent member, DiscordGuild guild)
        {
            if (Guild?.Id != guild.Id)
            {
                return;
            }
            
            _linkHandler.OnGuildMemberLeft(member.User);
        }

        // ReSharper disable once UnusedMember.Local
        [HookMethod(DiscordExtHooks.OnDiscordGuildMemberRoleAdded)]
        private void OnDiscordGuildMemberRoleAdded(GuildMember member, Snowflake roleId, DiscordGuild guild)
        {
            if (Guild?.Id != guild.Id)
            {
                return;
            }

            if (!_pluginConfig.WelcomeMessageSettings.EnableWelcomeMessage || !_pluginConfig.WelcomeMessageSettings.SendOnRoleAdded.Contains(roleId))
            {
                return;
            }

            if (member.User.IsLinked())
            {
                return;
            }
            
            SendGlobalTemplateMessage(TemplateKeys.WelcomeMessage.PmWelcomeMessage, member.User);
        }
    }
}