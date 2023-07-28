using System;
using System.Collections.Generic;
using DiscordCorePlugin.Localization;
using DiscordCorePlugin.Placeholders;
using DiscordCorePlugin.Templates;
using Oxide.Core.Libraries.Covalence;
using Oxide.Ext.Discord.Entities;
using Oxide.Ext.Discord.Entities.Channels;
using Oxide.Ext.Discord.Entities.Interactions;
using Oxide.Ext.Discord.Entities.Interactions.MessageComponents;
using Oxide.Ext.Discord.Entities.Interactions.Response;
using Oxide.Ext.Discord.Entities.Messages;
using Oxide.Ext.Discord.Entities.Users;
using Oxide.Ext.Discord.Extensions;
using Oxide.Ext.Discord.Helpers;
using Oxide.Ext.Discord.Interfaces.Promises;
using Oxide.Ext.Discord.Libraries.Placeholders;
using Oxide.Ext.Discord.Libraries.Placeholders.Keys;
using Oxide.Ext.Discord.Libraries.Templates;
using Oxide.Ext.Discord.Libraries.Templates.Components;
using Oxide.Ext.Discord.Libraries.Templates.Embeds;
using Oxide.Ext.Discord.Libraries.Templates.Messages;
using Oxide.Ext.Discord.Promises;

namespace DiscordCorePlugin.Plugins
{
    //Define:FileOrder=50
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
            DiscordMessageTemplate linkCommand = CreateTemplateEmbed($"Player {DefaultKeys.Player.NamePlayerId} has {DiscordFormatting.Bold("linked")} with discord {DefaultKeys.User.Mention}", DiscordColor.Success);
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Announcements.Link.Command, linkCommand, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate linkAdmin = CreateTemplateEmbed($"Player {DefaultKeys.Player.NamePlayerId} was {DiscordFormatting.Bold("linked")} with discord {DefaultKeys.User.Mention} by an admin", DiscordColor.Success);
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Announcements.Link.Admin, linkAdmin, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate linkApi = CreateTemplateEmbed($"Player {DefaultKeys.Player.NamePlayerId} has {DiscordFormatting.Bold("linked")} with discord {DefaultKeys.User.Mention}", DiscordColor.Success);
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Announcements.Link.Api, linkApi, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));

            DiscordMessageTemplate linkGuildRejoin = CreateTemplateEmbed($"Player {DefaultKeys.Player.NamePlayerId} has {DiscordFormatting.Bold("linked")} with discord {DefaultKeys.User.Mention} because they rejoined the {DiscordFormatting.Bold(DefaultKeys.Guild.Name)} Discord server", DiscordColor.Success);
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Announcements.Link.GuildRejoin, linkGuildRejoin, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate linkInactiveRejoin = CreateTemplateEmbed($"Player {DefaultKeys.Player.NamePlayerId} has {DiscordFormatting.Bold("linked")} with discord {DefaultKeys.User.Mention} because they rejoined the {DiscordFormatting.Bold(DefaultKeys.Server.Name)} game server", DiscordColor.Success);
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Announcements.Link.InactiveRejoin, linkInactiveRejoin, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate unlinkCommand = CreateTemplateEmbed($"Player {DefaultKeys.Player.NamePlayerId} has {DiscordFormatting.Bold("unlinked")} from discord {DefaultKeys.User.Mention}", DiscordColor.Danger);
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Announcements.Unlink.Command, unlinkCommand, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate unlinkAdmin = CreateTemplateEmbed($"Player {DefaultKeys.Player.NamePlayerId} was {DiscordFormatting.Bold("unlinked")} from discord {DefaultKeys.User.Mention} by an admin", DiscordColor.Danger);
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Announcements.Unlink.Admin, unlinkAdmin, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate unlinkApi = CreateTemplateEmbed($"Player {DefaultKeys.Player.NamePlayerId}has {DiscordFormatting.Bold("unlinked")} from discord {DefaultKeys.User.Mention}", DiscordColor.Danger);
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Announcements.Unlink.Api, unlinkApi, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));

            DiscordMessageTemplate unlinkLeftGuild = CreateTemplateEmbed($"Player {DefaultKeys.Player.NamePlayerId} has {DiscordFormatting.Bold("unlinked")} from discord {DefaultKeys.User.Fullname}({DefaultKeys.User.Id}) because they left the {DiscordFormatting.Bold(DefaultKeys.Guild.Name)} Discord server", DiscordColor.Danger);
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Announcements.Unlink.LeftGuild, unlinkLeftGuild, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate unlinkInactive = CreateTemplateEmbed($"Player {DefaultKeys.Player.NamePlayerId} has {DiscordFormatting.Bold("unlinked")} from discord {DefaultKeys.User.Fullname}({DefaultKeys.User.Id}) because they were inactive since {DefaultKeys.Timestamp.LongDateTime}", DiscordColor.Danger);
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Announcements.Unlink.Inactive, unlinkInactive, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate playerBanned = CreateTemplateEmbed($"Player {DefaultKeys.Player.NamePlayerId} has been linked banned for too many declined link attempts. The players ban will end on {DefaultKeys.Timestamp.LongDateTime}.", DiscordColor.Danger);
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Announcements.Ban.PlayerBanned, playerBanned, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate userBanned = CreateTemplateEmbed($"User {DefaultKeys.User.Mention} has been linked banned for too many declined link attempts. The players ban will end on {DefaultKeys.Timestamp.LongDateTime}.", DiscordColor.Danger);
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Announcements.Ban.UserBanned, userBanned, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
        }

        public void RegisterWelcomeMessages()
        {
            DiscordMessageTemplate pmWelcomeMessage = CreateTemplateEmbed($"Welcome to the {DiscordFormatting.Bold(DefaultKeys.Guild.Name)} Discord server. " +
                                                                                          $"If you would like to link your player and Discord accounts please click on the {DiscordFormatting.Bold("Link Accounts")} button below to start the process." +
                                                                                          $"{DiscordFormatting.Underline("\nNote: You must be in game to complete the link.")}", DiscordColor.Success);
            pmWelcomeMessage.Components = new List<BaseComponentTemplate>
            {
                new ButtonTemplate("Link Accounts", ButtonStyle.Success, WelcomeMessageLinkAccountsButtonId, AcceptEmoji)
            };
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.WelcomeMessage.PmWelcomeMessage, pmWelcomeMessage, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate guildWelcomeMessage = CreateTemplateEmbed($"Welcome to the {DiscordFormatting.Bold(DefaultKeys.Guild.Name)} Discord server. " +
                                                                                             "This server supports linking your Discord and in game accounts. " +
                                                                                             $"If you would like to link your player and Discord accounts please click on the {DiscordFormatting.Bold("Link Accounts")} button below to start the process." + 
                                                                                             $"{DiscordFormatting.Underline("\nNote: You must be in game to complete the link.")}", DiscordColor.Success);
            guildWelcomeMessage.Components = new List<BaseComponentTemplate>
            {
                new ButtonTemplate("Link Accounts", ButtonStyle.Success, GuildWelcomeMessageLinkAccountsButtonId, AcceptEmoji)
            };
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.WelcomeMessage.GuildWelcomeMessage, guildWelcomeMessage, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate welcomeMessageAlreadyLinked = CreateTemplateEmbed($"You are unable to link your {DefaultKeys.User.Mention} Discord user because you're already linked to {DefaultKeys.Player.Name}", DiscordColor.Danger);
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.WelcomeMessage.Error.AlreadyLinked, welcomeMessageAlreadyLinked, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
        }

        public void RegisterCommandMessages()
        {
            DiscordMessageTemplate codeSuccess = CreateTemplateEmbed($"Please join the {DiscordFormatting.Bold(DefaultKeys.Server.Name)} game server and type {DiscordFormatting.Bold($"/{DefaultKeys.Plugin.Lang.WithFormat(ServerLang.Commands.DcCommand)} {DefaultKeys.Plugin.Lang.WithFormat(ServerLang.Commands.LinkCommand)} {PlaceholderKeys.LinkCode}")} in server chat.", DiscordColor.Success);
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Commands.Code.Success, codeSuccess, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate userSuccess = CreateTemplateEmbed($"We have sent a message to {DiscordFormatting.Bold(DefaultKeys.Player.Name)} on the {DiscordFormatting.Bold(DefaultKeys.Server.Name)} server. Please follow the directions to complete your link.", DiscordColor.Success);
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Commands.User.Success, userSuccess, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate userInvalidPlayer = CreateTemplateEmbed($"You have not selected a valid player from the dropdown. Please try the command again.", DiscordColor.Danger);
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Commands.User.Error.PlayerIsInvalid, userInvalidPlayer, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate userNotConnected = CreateTemplateEmbed($"Player {DiscordFormatting.Bold(DefaultKeys.Player.Name)} is not connected to the {DiscordFormatting.Bold(DefaultKeys.Server.Name)} server. Please join the server and try the command again.", DiscordColor.Danger);
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Commands.User.Error.PlayerNotConnected, userNotConnected, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate leaveNotLinked = CreateTemplateEmbed($"You are not able to unlink because you are not currently linked.", DiscordColor.Danger);
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Commands.Leave.Error.UserNotLinked, leaveNotLinked, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
        }

        public void RegisterAdminCommandMessages()
        {
            DiscordMessageTemplate playerNotFound = CreateTemplateEmbed($"Failed to link. Player with '{DiscordFormatting.Bold(DefaultKeys.Player.Name)}' ID was not found.", DiscordColor.Danger);
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Commands.Admin.Link.Error.PlayerNotFound, playerNotFound, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate playerAlreadyLinked = CreateTemplateEmbed($"Failed to link. Player '{DiscordFormatting.Bold($"{DefaultKeys.Player.NamePlayerId}")}' is already linked to {DefaultKeys.User.Mention}. If you would like to link this player please unlink first.", DiscordColor.Danger);
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Commands.Admin.Link.Error.PlayerAlreadyLinked, playerAlreadyLinked, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate userAlreadyLinked = CreateTemplateEmbed($"Failed to link. User {DefaultKeys.User.Mention} is already linked to {DefaultKeys.Player.NamePlayerId}. If you would like to link this user please unlink them first.", DiscordColor.Danger);
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Commands.Admin.Link.Error.UserAlreadyLinked, userAlreadyLinked, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate adminLinkSuccess = CreateTemplateEmbed($"You have successfully linked Player '{DiscordFormatting.Bold($"{DefaultKeys.Player.NamePlayerId}")}' to {DefaultKeys.User.Mention}", DiscordColor.Success);
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Commands.Admin.Link.Success, adminLinkSuccess, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate unlinkMustSpecify = CreateTemplateEmbed($"Failed to unlink. You must specify either player or user or both.", DiscordColor.Danger);
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Commands.Admin.Unlink.Error.MustSpecifyOne, unlinkMustSpecify, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));

            DiscordMessageTemplate unlinkPlayerNotLinked = CreateTemplateEmbed($"Failed to unlink.'{DiscordFormatting.Bold($"{DefaultKeys.Player.NamePlayerId}")}' is not linked.", DiscordColor.Danger);
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Commands.Admin.Unlink.Error.PlayerIsNotLinked, unlinkPlayerNotLinked, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));

            DiscordMessageTemplate unlinkUserNotLinked = CreateTemplateEmbed($"Failed to unlink. {DefaultKeys.User.Mention} is not linked.", DiscordColor.Danger);
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Commands.Admin.Unlink.Error.UserIsNotLinked, unlinkUserNotLinked, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate unlinkNotSame = CreateTemplateEmbed($"Failed to unlink. The specified player and user are not linked to each other.\n" +
                                                                                       $"Player '{DiscordFormatting.Bold($"{DefaultKeys.Player.NamePlayerId}")}' is linked to {DefaultKeys.UserTarget.Mention}.\n" +
                                                                                       $"User {DefaultKeys.User.Mention} is linked to '{DiscordFormatting.Bold($"{DefaultKeys.PlayerTarget.Name}({DefaultKeys.PlayerTarget.Id})")}'", DiscordColor.Danger);
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Commands.Admin.Unlink.Error.LinkNotSame, unlinkNotSame, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));

            DiscordMessageTemplate adminUnlinkSuccess = CreateTemplateEmbed($"You have successfully unlink Player '{DiscordFormatting.Bold($"{DefaultKeys.Player.NamePlayerId}")}' from {DefaultKeys.User.Mention}", DiscordColor.Success);
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Commands.Admin.Unlink.Success, adminUnlinkSuccess, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));

            DiscordMessageTemplate playerUnbanSuccess = CreateTemplateEmbed($"You have successfully unbanned Player '{DiscordFormatting.Bold($"{DefaultKeys.Player.NamePlayerId}")}'. The player can now link again.", DiscordColor.Success);
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Commands.Admin.Unban.Player, playerUnbanSuccess, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate userUnbanSuccess = CreateTemplateEmbed($"You have successfully unbanned User {DefaultKeys.User.Mention}. The user can now link again.", DiscordColor.Success);
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Commands.Admin.Unban.User, userUnbanSuccess, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate playerUnbanNotFound = CreateTemplateEmbed($"Failed to find Player with '{DiscordFormatting.Bold(PlaceholderKeys.NotFound)}' ID", DiscordColor.Danger);
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Commands.Admin.Unban.Error.PlayerNotFound, playerUnbanNotFound, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate playerUnbanNotBanned = CreateTemplateEmbed($"Failed to find unban player '{DiscordFormatting.Bold(DefaultKeys.Player.NamePlayerId)}' because they are not banned", DiscordColor.Danger);
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Commands.Admin.Unban.Error.PlayerNotBanned, playerUnbanNotBanned, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate userUnbanNotBanned = CreateTemplateEmbed($"Failed to find unban user {DefaultKeys.User.Mention} because they are not banned", DiscordColor.Danger);
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Commands.Admin.Unban.Error.UserNotBanned, userUnbanNotBanned, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate playerSearchNotFound = CreateTemplateEmbed($"Failed to find Player with '{DiscordFormatting.Bold(PlaceholderKeys.NotFound)}' ID", DiscordColor.Danger);
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Commands.Admin.Search.Error.PlayerNotFound, playerSearchNotFound, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate searchSuccess = new DiscordMessageTemplate
            {
                Embeds = new List<DiscordEmbedTemplate>
                {
                    new DiscordEmbedTemplate
                    {
                        Color = DiscordColor.Danger.ToHex(),
                        Fields =
                        {
                            new DiscordEmbedFieldTemplate("Player", $"{DefaultKeys.Player.NameClan}"),
                            new DiscordEmbedFieldTemplate("Player ID", $"{DefaultKeys.Player.Id}"),
                            new DiscordEmbedFieldTemplate("User", DefaultKeys.User.Fullname),
                            new DiscordEmbedFieldTemplate("Is Linked", DefaultKeys.Player.IsLinked),
                        }
                    }
                },
                Components =
                {
                    new ButtonTemplate("Steam Profile", ButtonStyle.Link, DefaultKeys.Player.SteamProfile),
                    new ButtonTemplate("BattleMetrics Profile", ButtonStyle.Link, DefaultKeys.Player.BattleMetricsPlayerId),
                    new ButtonTemplate("Server Armor", ButtonStyle.Link, DefaultKeys.Player.ServerArmorProfile),
                }
            };
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Commands.Admin.Search.Success, searchSuccess, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
        }

        public void RegisterLinkMessages()
        {
            DiscordMessageTemplate linkCommand = CreateTemplateEmbed($"You have successfully linked {DiscordFormatting.Bold(DefaultKeys.Player.Name)} with your Discord user {DefaultKeys.User.Mention}", DiscordColor.Success);
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Link.Completed.Command, linkCommand, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate linkAdmin = CreateTemplateEmbed($"You have been successfully linked with {DiscordFormatting.Bold(DefaultKeys.Player.Name)} and Discord user {DefaultKeys.User.Mention} by an admin", DiscordColor.Success);
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Link.Completed.Admin, linkAdmin, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate linkApi = CreateTemplateEmbed($"You have successfully linked {DiscordFormatting.Bold(DefaultKeys.Player.Name)} with your Discord user {DefaultKeys.User.Mention}", DiscordColor.Success);
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Link.Completed.Api, linkApi, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate linkRejoin = CreateTemplateEmbed($"Your {DiscordFormatting.Bold(DefaultKeys.Player.Name)} game account has been relinked with your Discord user {DefaultKeys.User.Mention} because you rejoined the {DiscordFormatting.Bold(DefaultKeys.Guild.Name)} Discord server", DiscordColor.Success);
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Link.Completed.GuildRejoin, linkRejoin, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate linkInactive = CreateTemplateEmbed($"Your {DiscordFormatting.Bold(DefaultKeys.Player.Name)} game account has been relinked with your Discord user {DefaultKeys.User.Mention} because you rejoined the {DiscordFormatting.Bold(DefaultKeys.Server.Name)} game server", DiscordColor.Success);
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Link.Completed.InactiveRejoin, linkInactive, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate unlinkCommand = CreateTemplateEmbed($"You have successfully unlinked {DiscordFormatting.Bold(DefaultKeys.Player.Name)} from your Discord user {DefaultKeys.User.Mention}", DiscordColor.Success);
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Unlink.Completed.Command, unlinkCommand, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate unlinkAdmin = CreateTemplateEmbed($"You have successfully been unlinked {DiscordFormatting.Bold(DefaultKeys.Player.Name)} from your Discord user {DefaultKeys.User.Mention} by an admin", DiscordColor.Success);
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Unlink.Completed.Admin, unlinkAdmin, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));

            DiscordMessageTemplate unlinkApi = CreateTemplateEmbed($"You have successfully unlinked {DiscordFormatting.Bold(DefaultKeys.Player.Name)} from your Discord user {DefaultKeys.User.Mention}", DiscordColor.Success);
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Unlink.Completed.Api, unlinkApi, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate unlinkInactive = CreateTemplateEmbed($"You have been successfully unlinked from {DiscordFormatting.Bold(DefaultKeys.Player.Name)} and Discord user {DefaultKeys.User.Mention} because you have been inactive since {DefaultKeys.Timestamp.LongDateTime}", DiscordColor.Success);
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Unlink.Completed.Inactive, unlinkInactive, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate declineUser = CreateTemplateEmbed($"We have successfully declined the link request from {DefaultKeys.Player.Name}. We're sorry for the inconvenience.", DiscordColor.Danger);
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Link.Declined.JoinWithUser, declineUser, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate declinePlayer = CreateTemplateEmbed($"{DefaultKeys.Player.Name} has declined your link request. Repeated declined attempts may result in a link ban.", DiscordColor.Danger);
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Link.Declined.JoinWithPlayer, declinePlayer, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate dmLinkAccounts = CreateTemplateEmbed($"To complete the link process please join the {DiscordFormatting.Bold(DefaultKeys.Server.Name)} game server and type {DiscordFormatting.Bold($"/{DefaultKeys.Plugin.Lang.WithFormat(ServerLang.Commands.DcCommand)} {DefaultKeys.Plugin.Lang.WithFormat(ServerLang.Commands.LinkCommand)} {PlaceholderKeys.LinkCode}")} in server chat.", DiscordColor.Success);
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Link.WelcomeMessage.DmLinkAccounts, dmLinkAccounts, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate guildLinkAccounts = CreateTemplateEmbed($"To complete the link process please join the {DiscordFormatting.Bold(DefaultKeys.Server.Name)} game server and type {DiscordFormatting.Bold($"/{DefaultKeys.Plugin.Lang.WithFormat(ServerLang.Commands.DcCommand)} {DefaultKeys.Plugin.Lang.WithFormat(ServerLang.Commands.LinkCommand)} {PlaceholderKeys.LinkCode}")} in server chat.", DiscordColor.Success);
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Link.WelcomeMessage.GuildLinkAccounts, guildLinkAccounts, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
        }

        public void RegisterBanMessages()
        {
            DiscordMessageTemplate banned = CreateTemplateEmbed($"You have been banned from making any more player link requests for until {DefaultKeys.Timestamp.LongDateTime} due to multiple declined requests.", DiscordColor.Danger);
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Banned.PlayerBanned, banned, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
        }

        public void RegisterJoinMessages()
        {
            DiscordMessageTemplate byUsername = CreateTemplateEmbed($"The player {DiscordFormatting.Bold(DefaultKeys.Player.Name)} is trying to link their game account to this discord user.\n" +
                                                                                    $"If you would like to accept please click on the {DiscordFormatting.Bold("Accept")} button.\n" +
                                                                                    $"If you did not initiate this link please click on the {DiscordFormatting.Bold("Decline")} button", DiscordColor.Success);
            byUsername.Components = new List<BaseComponentTemplate>
            {
                new ButtonTemplate("Accept", ButtonStyle.Success, AcceptLinkButtonId, AcceptEmoji),
                new ButtonTemplate("Decline", ButtonStyle.Danger, DeclineLinkButtonId, DeclineEmoji)
            };
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Join.CompleteLink, byUsername, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
        }

        public void RegisterErrorMessages()
        {
            DiscordMessageTemplate userAlreadyLinked = CreateTemplateEmbed($"You are unable to link because you are already linked to player {DiscordFormatting.Bold(DefaultKeys.Player.Name)}", DiscordColor.Danger);
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Errors.UserAlreadyLinked, userAlreadyLinked, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate playerAlreadyLinked = CreateTemplateEmbed($"You are unable to link to player {DiscordFormatting.Bold(DefaultKeys.Player.Name)} because they are already linked", DiscordColor.Danger);
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Errors.PlayerAlreadyLinked, playerAlreadyLinked, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));

            DiscordMessageTemplate codeActivationNotFound = CreateTemplateEmbed($"We failed to find a pending link activation for the code {DiscordFormatting.Bold(PlaceholderKeys.LinkCode.Placeholder)}. Please confirm you have the correct code and try again.", DiscordColor.Danger);
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Errors.CodActivationNotFound, codeActivationNotFound, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
        
            DiscordMessageTemplate lookupActivationNotFound = CreateTemplateEmbed($"We failed to find a pending link activation for user {DiscordFormatting.Bold(DefaultKeys.User.Fullname)}. Please confirm you have started that activation from the game server for this user.", DiscordColor.Danger);
            _templates.RegisterLocalizedTemplateAsync(this, TemplateKeys.Errors.LookupActivationNotFound, lookupActivationNotFound, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
        }

        public DiscordMessageTemplate CreateTemplateEmbed(string description, DiscordColor color)
        {
            return new DiscordMessageTemplate
            {
                Embeds = new List<DiscordEmbedTemplate>
                {
                    new DiscordEmbedTemplate
                    {
                        Description = $"[{DefaultKeys.Plugin.Title}] {description}",
                        Color = color.ToHex()
                    }
                }
            };
        }
        
        public void SendTemplateMessage(string templateName, DiscordInteraction interaction, PlaceholderData placeholders = null)
        {
            InteractionCallbackData response = new InteractionCallbackData
            {
                AllowedMentions = AllowedMentions.None
            };
            if (interaction.GuildId.HasValue)
            {
                response.Flags = MessageFlags.Ephemeral;
            }
            
            interaction.CreateTemplateResponse(Client, InteractionResponseType.ChannelMessageWithSource, templateName, response, placeholders);
        }

        public void SendTemplateMessage(string templateName, DiscordUser user, IPlayer player = null, PlaceholderData placeholders = null)
        {
            AddDefaultPlaceholders(ref placeholders, user, player);
            user.SendTemplateDirectMessage(Client, templateName, _lang.GetPlayerLanguage(player).Id, new MessageCreate
            {
                AllowedMentions = AllowedMentions.None
            }, placeholders);
        }

        public void SendGlobalTemplateMessage(string templateName, DiscordUser user, IPlayer player = null, PlaceholderData placeholders = null)
        {
            AddDefaultPlaceholders(ref placeholders, user, player);
            user.SendGlobalTemplateDirectMessage(Client, templateName, new MessageCreate
            {
                AllowedMentions = AllowedMentions.None
            }, placeholders);
        }
        
        public IPromise<DiscordMessage> SendGlobalTemplateMessage(string templateName, Snowflake channelId, DiscordUser user = null, IPlayer player = null, PlaceholderData placeholders = null)
        {
            DiscordChannel channel = Guild.Channels[channelId];
            if (channel != null)
            {
                AddDefaultPlaceholders(ref placeholders, user, player);
                return channel.CreateGlobalTemplateMessage(Client, templateName, new MessageCreate
                {
                    AllowedMentions = AllowedMentions.None
                }, placeholders);
            }

            return Promise<DiscordMessage>.Rejected(new Exception("Channel Not Found"));
        }
        
        public void UpdateGuildTemplateMessage(string templateName, DiscordMessage message, PlaceholderData placeholders = null)
        {
            AddDefaultPlaceholders(ref placeholders, null, null);
            message.EditGlobalTemplateMessage(Client, templateName, placeholders);
        }

        private void AddDefaultPlaceholders(ref PlaceholderData placeholders, DiscordUser user, IPlayer player)
        {
            placeholders = placeholders ?? GetDefault();
            placeholders.AddUser(user).AddPlayer(player).AddGuild(Guild);
        }
    }
}