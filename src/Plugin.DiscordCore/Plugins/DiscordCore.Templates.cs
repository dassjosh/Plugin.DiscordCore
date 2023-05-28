using System;
using System.Collections.Generic;
using DiscordCorePlugin.Localization;
using DiscordCorePlugin.Templates;
using Oxide.Core.Libraries.Covalence;
using Oxide.Ext.Discord.Entities;
using Oxide.Ext.Discord.Entities.Channels;
using Oxide.Ext.Discord.Entities.Interactions;
using Oxide.Ext.Discord.Entities.Interactions.MessageComponents;
using Oxide.Ext.Discord.Entities.Interactions.Response;
using Oxide.Ext.Discord.Entities.Messages;
using Oxide.Ext.Discord.Entities.Users;
using Oxide.Ext.Discord.Helpers;
using Oxide.Ext.Discord.Interfaces.Promises;
using Oxide.Ext.Discord.Libraries.Placeholders;
using Oxide.Ext.Discord.Libraries.Templates;
using Oxide.Ext.Discord.Libraries.Templates.Components;
using Oxide.Ext.Discord.Libraries.Templates.Embeds;
using Oxide.Ext.Discord.Libraries.Templates.Messages;
using Oxide.Ext.Discord.Promises;

namespace DiscordCorePlugin.Plugins
{
    //Define:FileOrder=50
    public class InClassName
    {
        public InClassName(string description, string color)
        {
            Description = description;
            Color = color;
        }

        public string Description { get; private set; }
        public string Color { get; private set; }
    }

    public partial class DiscordCore
    {
        private const string AcceptEmoji = "✅";
        private const string DeclineEmoji = "❌";
        
        public void RegisterTemplates()
        {
            RegisterAnnouncements();
            RegisterWelcomeMessages();
            RegisterCommandMessages();
            RegisterAdminCommandMessages();
            RegisterLinkMessages();
            RegisterBanMessages();
            RegisterJoinMessages();
            RegisterErrorMessages();
        }

        public void RegisterAnnouncements()
        {
            DiscordMessageTemplate linkCommand = CreateTemplateEmbed(new InClassName($"Player {{player.name}}({{player.id}}) has {DiscordFormatting.Bold("linked")} with discord {{user.mention}}", DiscordSuccess), new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Announcements.Link.Command, linkCommand, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate linkAdmin = CreateTemplateEmbed(new InClassName($"Player {{player.name}}({{player.id}}) was {DiscordFormatting.Bold("linked")} with discord {{user.mention}} by an admin", DiscordSuccess), new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Announcements.Link.Admin, linkAdmin, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate linkApi = CreateTemplateEmbed(new InClassName($"Player {{player.name}}({{player.id}}) has {DiscordFormatting.Bold("linked")} with discord {{user.mention}}", DiscordSuccess), new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Announcements.Link.Api, linkApi, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));

            DiscordMessageTemplate linkGuildRejoin = CreateTemplateEmbed(new InClassName($"Player {{player.name}}({{player.id}}) has {DiscordFormatting.Bold("linked")} with discord {{user.mention}} because they rejoined the {DiscordFormatting.Bold("{guild.name}")} Discord server", DiscordSuccess), new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Announcements.Link.GuildRejoin, linkGuildRejoin, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate linkInactiveRejoin = CreateTemplateEmbed(new InClassName($"Player {{player.name}}({{player.id}}) has {DiscordFormatting.Bold("linked")} with discord {{user.mention}} because they rejoined the {DiscordFormatting.Bold("{server.name}")} game server", DiscordSuccess), new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Announcements.Link.InactiveRejoin, linkInactiveRejoin, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate unlinkCommand = CreateTemplateEmbed(new InClassName($"Player {{player.name}}({{player.id}}) has {DiscordFormatting.Bold("unlinked")} from discord {{user.mention}}", DiscordDanger), new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Announcements.Unlink.Command, unlinkCommand, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate unlinkAdmin = CreateTemplateEmbed(new InClassName($"Player {{player.name}}({{player.id}}) was {DiscordFormatting.Bold("unlinked")} from discord {{user.mention}} by an admin", DiscordDanger), new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Announcements.Unlink.Admin, unlinkAdmin, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate unlinkApi = CreateTemplateEmbed(new InClassName($"Player {{player.name}}({{player.id}}) has {DiscordFormatting.Bold("unlinked")} from discord {{user.mention}}", DiscordDanger), new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Announcements.Unlink.Api, unlinkApi, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));

            DiscordMessageTemplate unlinkLeftGuild = CreateTemplateEmbed(new InClassName($"Player {{player.name}}({{player.id}}) has {DiscordFormatting.Bold("unlinked")} from discord {{user.fullname}}({{user.id}}) because they left the {DiscordFormatting.Bold("{guild.name}")} Discord server", DiscordDanger), new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Announcements.Unlink.LeftGuild, unlinkLeftGuild, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate unlinkInactive = CreateTemplateEmbed(new InClassName($"Player {{player.name}}({{player.id}}) has {DiscordFormatting.Bold("unlinked")} from discord {{user.fullname}}({{user.id}}) because they were inactive for {{discordcore.inactive.duration}} days", DiscordDanger), new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Announcements.Unlink.Inactive, unlinkInactive, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
        }

        public void RegisterWelcomeMessages()
        {
            DiscordMessageTemplate pmWelcomeMessage = CreateTemplateEmbed(new InClassName($"Welcome to the {DiscordFormatting.Bold("{guild.name}")} Discord server. " +
                                                                                          $"If you would link to link your player and Discord accounts please click on the {DiscordFormatting.Bold("Link Accounts")} button below to start the process." +
                                                                                          $"{DiscordFormatting.Underline("\nNote: You must be in game to complete the link.")}", DiscordSuccess), new TemplateVersion(1, 0, 0));
            pmWelcomeMessage.Components = new List<BaseComponentTemplate>
            {
                new ButtonTemplate("Link Accounts", ButtonStyle.Success, WelcomeMessageLinkAccountsButtonId, AcceptEmoji)
            };
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.WelcomeMessage.PmWelcomeMessage, pmWelcomeMessage, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate guildWelcomeMessage = CreateTemplateEmbed(new InClassName($"Welcome to the {DiscordFormatting.Bold("{guild.name}")} Discord server. " +
                                                                                             "This server supports linking your Discord and in game accounts. " +
                                                                                             $"If you would link to link your player and Discord accounts please click on the {DiscordFormatting.Bold("Link Accounts")} button below to start the process." + 
                                                                                             $"{DiscordFormatting.Underline("\nNote: You must be in game to complete the link.")}", DiscordSuccess), new TemplateVersion(1, 0, 0));
            guildWelcomeMessage.Components = new List<BaseComponentTemplate>
            {
                new ButtonTemplate("Link Accounts", ButtonStyle.Success, GuildWelcomeMessageLinkAccountsButtonId, AcceptEmoji)
            };
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.WelcomeMessage.GuildWelcomeMessage, guildWelcomeMessage, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate welcomeMessageAlreadyLinked = CreateTemplateEmbed(new InClassName("You are unable to link your {user.mention} Discord user because you're already linked to {player.name}", DiscordDanger), new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.WelcomeMessage.Error.AlreadyLinked, welcomeMessageAlreadyLinked, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
        }

        public void RegisterCommandMessages()
        {
            DiscordMessageTemplate codeSuccess = CreateTemplateEmbed(new InClassName($"Please join the {DiscordFormatting.Bold("{server.name}")} game server and type {DiscordFormatting.Bold($"/{{plugin.lang:{ServerLang.Commands.DcCommand}}} {ServerLinkArgument} {{discordcore.link.code}}")} in server chat.", DiscordSuccess), new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Commands.Code.Success, codeSuccess, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate userSuccess = CreateTemplateEmbed(new InClassName($"We have sent a message to {DiscordFormatting.Bold("{player.name}")} on the {DiscordFormatting.Bold("{server.name}")} server. Please follow the directions to complete your link.", DiscordSuccess), new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Commands.User.Success, userSuccess, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate userInvalidPlayer = CreateTemplateEmbed(new InClassName($"You have not selected a valid player from the dropdown. Please try the command again.", DiscordDanger), new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Commands.User.Error.PlayerIsInvalid, userInvalidPlayer, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate userNotConnected = CreateTemplateEmbed(new InClassName($"Player {DiscordFormatting.Bold("{player.name}")} is not connected to the {DiscordFormatting.Bold("{server.name}")} server. Please join the server and try the command again.", DiscordDanger), new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Commands.User.Error.PlayerNotConnected, userNotConnected, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate leaveNotLinked = CreateTemplateEmbed(new InClassName($"You are not able to unlink because you are not currently linked.", DiscordDanger), new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Commands.Leave.Error.UserNotLinked, leaveNotLinked, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
        }

        public void RegisterAdminCommandMessages()
        {
            DiscordMessageTemplate playerNotFound = CreateTemplateEmbed(new InClassName($"Failed to link. Player with '{DiscordFormatting.Bold("{player.id}")}' ID was not found.", DiscordDanger), new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Commands.Admin.Link.Error.PlayerNotFound, playerNotFound, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate playerAlreadyLinked = CreateTemplateEmbed(new InClassName($"Failed to link. Player '{DiscordFormatting.Bold("{player.name}({player.id})")}' is already linked to {{user.mention}}. If you would like to link this player please unlink first.", DiscordDanger), new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Commands.Admin.Link.Error.PlayerAlreadyLinked, playerAlreadyLinked, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate userAlreadyLinked = CreateTemplateEmbed(new InClassName($"Failed to link. User {{user.mention}} is already linked to {{player.name}}({{player.id}}). If you would like to link this user please unlink them first.", DiscordDanger), new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Commands.Admin.Link.Error.UserAlreadyLinked, userAlreadyLinked, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate adminLinkSuccess = CreateTemplateEmbed(new InClassName($"You have successfully linked Player '{DiscordFormatting.Bold("{player.name}({player.id})")}' to {{user.mention}}", DiscordSuccess), new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Commands.Admin.Link.Success, adminLinkSuccess, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate unlinkMustSpecify = CreateTemplateEmbed(new InClassName($"Failed to unlink. You must specify either player or user or both.", DiscordDanger), new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Commands.Admin.Unlink.Error.MustSpecifyOne, unlinkMustSpecify, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));

            DiscordMessageTemplate unlinkPlayerNotLinked = CreateTemplateEmbed(new InClassName($"Failed to unlink.'{DiscordFormatting.Bold("{player.name}({player.id})")}' is not linked.", DiscordDanger), new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Commands.Admin.Unlink.Error.PlayerIsNotLinked, unlinkPlayerNotLinked, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));

            DiscordMessageTemplate unlinkUserNotLinked = CreateTemplateEmbed(new InClassName($"Failed to unlink. {{user.mention}} is not linked.", DiscordDanger), new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Commands.Admin.Unlink.Error.UserIsNotLinked, unlinkUserNotLinked, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate unlinkNotSame = CreateTemplateEmbed(new InClassName($"Failed to unlink. The specified player and user are not linked to each other.\n" +
                                                                                       $"Player '{DiscordFormatting.Bold("{player.name}({player.id})")}' is linked to {{discordcore.other.user.mention}}.\n" +
                                                                                       $"User {{user.mention}} is linked to '{DiscordFormatting.Bold("{discordcore.other.player.name}({discordcore.other.player.id})")}'", DiscordDanger), new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Commands.Admin.Unlink.Error.LinkNotSame, unlinkNotSame, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));

            DiscordMessageTemplate adminUnlinkSuccess = CreateTemplateEmbed(new InClassName($"You have successfully unlink Player '{DiscordFormatting.Bold("{player.name}({player.id})")}' from {{user.mention}}", DiscordSuccess), new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Commands.Admin.Unlink.Success, adminUnlinkSuccess, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));

            DiscordMessageTemplate playerSearchNotFound = CreateTemplateEmbed(new InClassName($"Failed to find Player with '{DiscordFormatting.Bold("{player.id}")}' ID", DiscordDanger), new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Commands.Admin.Search.Error.PlayerNotFound, playerSearchNotFound, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate searchSuccess = new DiscordMessageTemplate
            {
                Embeds = new List<DiscordEmbedTemplate>
                {
                    new DiscordEmbedTemplate
                    {
                        Description = "[{{plugin.title}}] Successfully found a match.",
                        Color = $"#{DiscordDanger}",
                        Fields =
                        {
                            new DiscordEmbedFieldTemplate("Player", "{player.name}({player.id})"),
                            new DiscordEmbedFieldTemplate("User", "{user.fullname}"),
                            new DiscordEmbedFieldTemplate("Is Linked", "{player.islinked}"),
                        }
                    }
                },
                Components =
                {
                    new ButtonTemplate("Steam Profile", ButtonStyle.Link, "https://steamcommunity.com/profiles/{player.id}"),
                    new ButtonTemplate("BattleMetrics Profile", ButtonStyle.Link, "https://www.battlemetrics.com/rcon/players?filter[search]={player.id}"),
                },
            };
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Commands.Admin.Search.Success, searchSuccess, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
        }

        public void RegisterLinkMessages()
        {
            DiscordMessageTemplate linkCommand = CreateTemplateEmbed(new InClassName($"You have successfully linked {DiscordFormatting.Bold("{player.name}")} with your Discord user {{user.mention}}", DiscordSuccess), new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Link.Completed.Command, linkCommand, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate linkAdmin = CreateTemplateEmbed(new InClassName($"You have been successfully linked with {DiscordFormatting.Bold("{player.name}")} and Discord user {{user.mention}} by an admin", DiscordSuccess), new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Link.Completed.Admin, linkAdmin, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate linkApi = CreateTemplateEmbed(new InClassName($"You have successfully linked {DiscordFormatting.Bold("{player.name}")} with your Discord user {{user.mention}}", DiscordSuccess), new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Link.Completed.Api, linkApi, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate linkRejoin = CreateTemplateEmbed(new InClassName($"Your {DiscordFormatting.Bold("{player.name}")} game account has been relinked with your Discord user {{user.mention}} because you rejoined the {DiscordFormatting.Bold("{guild.name}")} Discord server", DiscordSuccess), new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Link.Completed.GuildRejoin, linkRejoin, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate linkInactive = CreateTemplateEmbed(new InClassName($"Your {DiscordFormatting.Bold("{player.name}")} game account has been relinked with your Discord user {{user.mention}} because you rejoined the {DiscordFormatting.Bold("{server.name}")} game server", DiscordSuccess), new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Link.Completed.InactiveRejoin, linkInactive, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate unlinkCommand = CreateTemplateEmbed(new InClassName($"You have successfully unlinked {DiscordFormatting.Bold("{player.name}")} from your Discord user {{user.mention}}", DiscordSuccess), new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Unlink.Completed.Command, unlinkCommand, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate unlinkAdmin = CreateTemplateEmbed(new InClassName($"You have successfully been unlinked {DiscordFormatting.Bold("{player.name}")} from your Discord user {{user.mention}} by an admin", DiscordSuccess), new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Unlink.Completed.Admin, unlinkAdmin, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));

            DiscordMessageTemplate unlinkApi = CreateTemplateEmbed(new InClassName($"You have successfully unlinked {DiscordFormatting.Bold("{player.name}")} from your Discord user {{user.mention}}", DiscordSuccess), new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Unlink.Completed.Api, unlinkApi, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate unlinkInactive = CreateTemplateEmbed(new InClassName($"You have been successfully unlinked from {DiscordFormatting.Bold("{player.name}")} and Discord user {{user.mention}} because you have been inactive for {{discordcore.inactive.duration}} days", DiscordSuccess), new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Unlink.Completed.Inactive, unlinkInactive, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate declineUser = CreateTemplateEmbed(new InClassName("We have successfully declined the link request from {player.name}. We're sorry for the inconvenience.", DiscordDanger), new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Link.Declined.JoinWithUser, declineUser, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate declinePlayer = CreateTemplateEmbed(new InClassName("{player.name} has declined your link request. Repeated declined attempts may result in a link ban.", DiscordDanger), new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Link.Declined.JoinWithPlayer, declinePlayer, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate dmLinkAccounts = CreateTemplateEmbed(new InClassName($"To complete the link process please join the {DiscordFormatting.Bold("{server.name}")} game server and type {DiscordFormatting.Bold($"/{{plugin.lang:{ServerLang.Commands.DcCommand}}} {ServerLinkArgument} {{discordcore.link.code}}")} in server chat.", DiscordSuccess), new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Link.WelcomeMessage.DmLinkAccounts, dmLinkAccounts, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate guildLinkAccounts = CreateTemplateEmbed(new InClassName($"To complete the link process please join the {DiscordFormatting.Bold("{server.name}")} game server and type {DiscordFormatting.Bold($"/{{plugin.lang:{ServerLang.Commands.DcCommand}}} {ServerLinkArgument} {{discordcore.link.code}}")} in server chat.", DiscordSuccess), new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Link.WelcomeMessage.GuildLinkAccounts, guildLinkAccounts, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
        }

        public void RegisterBanMessages()
        {
            DiscordMessageTemplate banned = CreateTemplateEmbed(new InClassName($"You have been banned from making any more player link requests for {{discordcore.join.banned.duration:0.##}} hours due to multiple declined requests.", DiscordDanger), new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Banned.PlayerBanned, banned, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
        }

        public void RegisterJoinMessages()
        {
            DiscordMessageTemplate byUsername = CreateTemplateEmbed(new InClassName($"The player {DiscordFormatting.Bold("{player.name}")} is trying to link their game account to this discord user.\n" +
                                                                                    $"If you could like to accept please click on the {DiscordFormatting.Bold("Accept")} button.\n" +
                                                                                    $"If you did not initiate this link please click on the {DiscordFormatting.Bold("Decline")} button", DiscordSuccess), new TemplateVersion(1, 0, 0));
            byUsername.Components = new List<BaseComponentTemplate>
            {
                new ButtonTemplate("Accept", ButtonStyle.Success, AcceptLinkButtonId, AcceptEmoji),
                new ButtonTemplate("Decline", ButtonStyle.Danger, DeclineLinkButtonId, DeclineEmoji)
            };
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Join.ByUsername, byUsername, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
        }

        public void RegisterErrorMessages()
        {
            DiscordMessageTemplate userAlreadyLinked = CreateTemplateEmbed(new InClassName($"You are unable to link because you are already linked to player {DiscordFormatting.Bold("{player.name}")}", DiscordDanger), new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Errors.UserAlreadyLinked, userAlreadyLinked, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate playerAlreadyLinked = CreateTemplateEmbed(new InClassName($"You are unable to link to player {DiscordFormatting.Bold("{player.name}")} because they are already linked", DiscordDanger), new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Errors.PlayerAlreadyLinked, playerAlreadyLinked, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));

            DiscordMessageTemplate codeActivationNotFound = CreateTemplateEmbed(new InClassName($"We failed to find a pending link activation for the code {DiscordFormatting.Bold("{discordcore.error.invalid.code}")}. Please confirm you have the correct code and try again.", DiscordDanger), new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Errors.CodActivationNotFound, codeActivationNotFound, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
        
            DiscordMessageTemplate lookupActivationNotFound = CreateTemplateEmbed(new InClassName($"We failed to find a pending link activation for the code {DiscordFormatting.Bold("{user.fullname}")}. Please confirm you have started that activation from the game server for this user.", DiscordDanger), new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Errors.LookupActivationNotFound, lookupActivationNotFound, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
        }

        public DiscordMessageTemplate CreateTemplateEmbed(InClassName inClassName, TemplateVersion version)
        {
            return new DiscordMessageTemplate
            {
                Embeds = new List<DiscordEmbedTemplate>
                {
                    new DiscordEmbedTemplate
                    {
                        Description = $"[{{plugin.title}}] {inClassName.Description}",
                        Color = $"#{inClassName.Color}"
                    }
                }
            };
        }
        
        public void SendTemplateMessage(string templateName, DiscordInteraction interaction, PlaceholderData placeholders = null)
        {
            Puts($"Keys: {placeholders?.GetKeys()}");
            InteractionCallbackData response = new InteractionCallbackData();
            if (interaction.GuildId.HasValue)
            {
                response.Flags = MessageFlags.Ephemeral;
            }
            
            interaction.CreateTemplateResponse(Client, this, InteractionResponseType.ChannelMessageWithSource, templateName, response, placeholders);
        }

        public void SendTemplateMessage(string templateName, DiscordUser user, IPlayer player = null, PlaceholderData placeholders = null)
        {
            AddDefaultPlaceholders(ref placeholders, user, player);
            user.SendTemplateDirectMessage(Client, this, templateName, _lang.GetPlayerLanguage(player), null, placeholders);
        }
        
        public void SendGlobalTemplateMessage(string templateName, DiscordUser user, IPlayer player = null, PlaceholderData placeholders = null)
        {
            AddDefaultPlaceholders(ref placeholders, user, player);
            user.SendGlobalTemplateDirectMessage(Client, this, templateName, null, placeholders);
        }
        
        public IPromise<DiscordMessage> SendGlobalTemplateMessage(string templateName, Snowflake channelId, DiscordUser user = null, IPlayer player = null, PlaceholderData placeholders = null)
        {
            DiscordChannel channel = Guild.Channels[channelId];
            if (channel != null)
            {
                AddDefaultPlaceholders(ref placeholders, user, player);
                return channel.CreateGlobalTemplateMessage(Client, this, templateName, null, placeholders);
            }

            return Promise<DiscordMessage>.Rejected(new Exception("Channel Not Found"));
        }
        
        public void UpdateGuildTemplateMessage(string templateName, DiscordMessage message, PlaceholderData placeholders = null)
        {
            AddDefaultPlaceholders(ref placeholders, null, null);
            message.EditGlobalTemplateMessage(Client, this, templateName, placeholders);
        }

        private void AddDefaultPlaceholders(ref PlaceholderData placeholders, DiscordUser user, IPlayer player)
        {
            placeholders = placeholders ?? GetDefault();
            placeholders.AddUser(user).AddPlayer(player).AddGuild(Guild);
        }
    }
}