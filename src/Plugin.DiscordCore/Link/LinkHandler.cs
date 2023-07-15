using System;
using System.Linq;
using DiscordCorePlugin.Configuration;
using DiscordCorePlugin.Data;
using DiscordCorePlugin.Enums;
using DiscordCorePlugin.Localization;
using DiscordCorePlugin.Plugins;
using DiscordCorePlugin.Templates;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Ext.Discord.Entities;
using Oxide.Ext.Discord.Entities.Guilds;
using Oxide.Ext.Discord.Entities.Interactions;
using Oxide.Ext.Discord.Entities.Users;
using Oxide.Ext.Discord.Extensions;
using Oxide.Ext.Discord.Libraries.Linking;
using Oxide.Ext.Discord.Libraries.Placeholders;
using Oxide.Plugins;

namespace DiscordCorePlugin.Link
{
    public class LinkHandler
    {
        private readonly PluginData _pluginData;
        private readonly LinkPermissionSettings _permissionSettings;
        private readonly LinkSettings _settings;
        private readonly DiscordLink _link = Interface.Oxide.GetLibrary<DiscordLink>();
        private readonly IPlayerManager _players = Interface.Oxide.GetLibrary<Covalence>().Players;
        private readonly DiscordCore _plugin = DiscordCore.Instance;
        private readonly Hash<LinkReason, LinkMessage> _linkMessages = new Hash<LinkReason, LinkMessage>();
        private readonly Hash<UnlinkedReason, LinkMessage> _unlinkMessages = new Hash<UnlinkedReason, LinkMessage>();

        public LinkHandler(PluginData pluginData, PluginConfig config)
        {
            _pluginData = pluginData;
            _settings = config.LinkSettings;
            _permissionSettings = config.PermissionSettings;
            LinkSettings link = config.LinkSettings;
            
            _linkMessages[LinkReason.Command] = new LinkMessage(ServerLang.Link.Completed.Command, ServerLang.Announcements.Link.Command, TemplateKeys.Link.Completed.Command, TemplateKeys.Announcements.Link.Command, _plugin, link);
            _linkMessages[LinkReason.Admin] = new LinkMessage(ServerLang.Link.Completed.Admin, ServerLang.Announcements.Link.Admin, TemplateKeys.Link.Completed.Admin, TemplateKeys.Announcements.Link.Admin, _plugin, link);
            _linkMessages[LinkReason.Api] = new LinkMessage(ServerLang.Link.Completed.Api, ServerLang.Announcements.Link.Api, TemplateKeys.Link.Completed.Api, TemplateKeys.Announcements.Link.Api, _plugin, link);
            _linkMessages[LinkReason.GuildRejoin] = new LinkMessage(ServerLang.Link.Completed.GuildRejoin, ServerLang.Announcements.Link.GuildRejoin, TemplateKeys.Link.Completed.GuildRejoin, TemplateKeys.Announcements.Link.GuildRejoin, _plugin, link);
            _linkMessages[LinkReason.InactiveRejoin] = new LinkMessage(ServerLang.Link.Completed.InactiveRejoin, ServerLang.Announcements.Link.InactiveRejoin, TemplateKeys.Link.Completed.InactiveRejoin, TemplateKeys.Announcements.Link.InactiveRejoin, _plugin, link);

            _unlinkMessages[UnlinkedReason.Command] = new LinkMessage(ServerLang.Unlink.Completed.Command, ServerLang.Announcements.Unlink.Command, TemplateKeys.Unlink.Completed.Command, TemplateKeys.Announcements.Unlink.Command, _plugin, link);
            _unlinkMessages[UnlinkedReason.Admin] = new LinkMessage(ServerLang.Unlink.Completed.Admin, ServerLang.Announcements.Unlink.Admin, TemplateKeys.Unlink.Completed.Admin, TemplateKeys.Announcements.Unlink.Admin, _plugin, link);
            _unlinkMessages[UnlinkedReason.Api] = new LinkMessage(ServerLang.Unlink.Completed.Api, ServerLang.Announcements.Unlink.Api, TemplateKeys.Unlink.Completed.Api, TemplateKeys.Announcements.Unlink.Api, _plugin, link);
            _unlinkMessages[UnlinkedReason.LeftGuild] = new LinkMessage(ServerLang.Unlink.Completed.LeftGuild, ServerLang.Announcements.Unlink.LeftGuild, null, TemplateKeys.Announcements.Unlink.LeftGuild, _plugin, link);
            _unlinkMessages[UnlinkedReason.Inactive] = new LinkMessage(null, TemplateKeys.Unlink.Completed.Inactive, ServerLang.Announcements.Unlink.Inactive, TemplateKeys.Announcements.Unlink.Inactive, _plugin, link);
        }

        public void HandleLink(IPlayer player, DiscordUser user, LinkReason reason, DiscordInteraction interaction)
        {
            _pluginData.InactivePlayerInfo.Remove(player.Id);
            _pluginData.LeftPlayerInfo.Remove(user.Id);
            _pluginData.PlayerDiscordInfo[player.Id] = new DiscordInfo(player, user);
            _link.OnLinked(_plugin, player, user);
            PlaceholderData data = _plugin.GetDefault(player, user);
            _linkMessages[reason]?.SendMessages(player, user, interaction, data);
            AddPermissions(player, user);
            _plugin.SaveData();
        }
        
        public void HandleUnlink(IPlayer player, DiscordUser user, UnlinkedReason reason, DiscordInteraction interaction)
        {
            if (player == null || user == null || !user.Id.IsValid())
            {
                return;
            }

            DiscordInfo info = _pluginData.PlayerDiscordInfo[player.Id];
            if (info == null)
            {
                return;
            }

            PlaceholderData data = _plugin.GetDefault(player, user);
            if (reason == UnlinkedReason.LeftGuild)
            {
                _pluginData.LeftPlayerInfo[info.DiscordId] = info;
            } 
            else if (reason == UnlinkedReason.Inactive)
            {
                _pluginData.InactivePlayerInfo[info.PlayerId] = info;
                data.AddTimeSpan(TimeSpan.FromDays(_settings.InactiveSettings.UnlinkInactiveDays));
            }

            _pluginData.PlayerDiscordInfo.Remove(player.Id);
            _link.OnUnlinked(_plugin, player, user);
            _unlinkMessages[reason]?.SendMessages(player, user, interaction, data);
            RemovePermissions(player, user, reason);
            _plugin.SaveData();
        }

        public void OnUserConnected(IPlayer player)
        {
            DiscordInfo info = _pluginData.PlayerDiscordInfo[player.Id];
            if (info != null)
            {
                info.LastOnline = DateTime.UtcNow;
                return;
            }

            if (_settings.InactiveSettings.AutoRelinkInactive)
            {
                info = _pluginData.InactivePlayerInfo[player.Id];
                if (info != null)
                {
                    info.LastOnline = DateTime.UtcNow;
                    DiscordUser user = _plugin.Guild.Members[info.DiscordId]?.User;
                    if (user == null)
                    {
                        _pluginData.LeftPlayerInfo[info.DiscordId] = info;
                        return;
                    }

                    HandleLink(player, user, LinkReason.InactiveRejoin, null);
                }
            }
        }
        
        public void OnGuildMemberLeft(DiscordUser user)
        {
            IPlayer player = user.Player;
            if (player != null)
            {
                HandleUnlink(player, user, UnlinkedReason.LeftGuild, null);
            }
        }

        public void OnGuildMemberJoin(DiscordUser user)
        {
            if (!_settings.AutoRelinkPlayer)
            {
                return;
            }
            
            DiscordInfo info = _pluginData.LeftPlayerInfo[user.Id];
            if (info == null)
            {
                return;
            }
            
            _pluginData.PlayerDiscordInfo[info.PlayerId] = info;
            _pluginData.LeftPlayerInfo.Remove(info.DiscordId);

            IPlayer player = _players.FindPlayerById(info.PlayerId);
            if (player == null)
            {
                return;
            }
            
            HandleLink(player, user, LinkReason.GuildRejoin, null);
        }

        public void ProcessLeaveAndRejoin()
        {
            foreach (DiscordInfo info in _pluginData.PlayerDiscordInfo.Values.ToList())
            {
                if (!_plugin.Guild.Members.ContainsKey(info.DiscordId))
                {
                    IPlayer player = _link.GetPlayer(info.DiscordId);
                    if (player != null)
                    {
                        DiscordUser user = player.GetDiscordUser();
                        HandleUnlink(player, user, UnlinkedReason.LeftGuild, null);
                    }
                }

                if (_settings.InactiveSettings.UnlinkInactive && info.LastOnline + TimeSpan.FromDays(_settings.InactiveSettings.UnlinkInactiveDays) < DateTime.UtcNow)
                {
                    IPlayer player = _link.GetPlayer(info.DiscordId);
                    if (player != null)
                    {
                        DiscordUser user = player.GetDiscordUser();
                        HandleUnlink(player, user, UnlinkedReason.LeftGuild, null);
                    }
                }
            }

            if (_settings.AutoRelinkPlayer)
            {
                foreach (DiscordInfo info in _pluginData.LeftPlayerInfo.Values.ToList())
                {
                    GuildMember member = _plugin.Guild.Members[info.DiscordId];
                    if (member != null)
                    {
                        OnGuildMemberJoin(member.User);
                    }
                }
            }
        }

        private void AddPermissions(IPlayer player, DiscordUser user)
        {
            for (int index = 0; index < _permissionSettings.LinkPermissions.Count; index++)
            {
                string permission = _permissionSettings.LinkPermissions[index];
                player.GrantPermission(permission);
            }
            
            for (int index = 0; index < _permissionSettings.LinkGroups.Count; index++)
            {
                string group = _permissionSettings.LinkGroups[index];
                player.AddToGroup(group);
            }

            for (int index = 0; index < _permissionSettings.LinkRoles.Count; index++)
            {
                Snowflake role = _permissionSettings.LinkRoles[index];
                DiscordCore.Instance.Guild.AddMemberRole(_plugin.Client, user.Id, role);
            }
        }

        private void RemovePermissions(IPlayer player, DiscordUser user, UnlinkedReason reason)
        {
            for (int index = 0; index < _permissionSettings.UnlinkPermissions.Count; index++)
            {
                string permission = _permissionSettings.UnlinkPermissions[index];
                player.RevokePermission(permission);
            }
            
            for (int index = 0; index < _permissionSettings.UnlinkGroups.Count; index++)
            {
                string group = _permissionSettings.UnlinkGroups[index];
                player.RemoveFromGroup(group);
            }

            if (reason != UnlinkedReason.LeftGuild)
            {
                for (int index = 0; index < _permissionSettings.UnlinkRoles.Count; index++)
                {
                    Snowflake role = _permissionSettings.UnlinkRoles[index];
                    DiscordCore.Instance.Guild.RemoveMemberRole(_plugin.Client, user.Id, role);
                }
            }
        }
    }
}