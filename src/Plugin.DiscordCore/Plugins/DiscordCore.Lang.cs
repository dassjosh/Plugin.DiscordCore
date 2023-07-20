using System.Collections.Generic;
using DiscordCorePlugin.Localization;
using DiscordCorePlugin.Placeholders;
using Oxide.Core.Libraries.Covalence;
using Oxide.Ext.Discord.Extensions;
using Oxide.Ext.Discord.Libraries.Placeholders;
using Oxide.Ext.Discord.Libraries.Placeholders.Keys;

namespace DiscordCorePlugin.Plugins
{
    //Define:FileOrder=15
    public partial class DiscordCore
    {
        public void Chat(IPlayer player, string key, PlaceholderData data = null)
        {
            if (player.IsConnected)
            {
                player.Reply(string.Format(Lang(ServerLang.Format, player), Lang(key, player, data)));
            }
        }

        public void BroadcastMessage(string key, PlaceholderData data)
        {
            string message = Lang(key);
            message = _placeholders.ProcessPlaceholders(message, data);
            if (string.IsNullOrEmpty(message))
            {
                return;
            }
            
            message = string.Format(Lang(ServerLang.Format), message);
            server.Broadcast(message);
        }

        public string Lang(string key, IPlayer player = null, PlaceholderData data = null)
        {
            string message = lang.GetMessage(key, this, player?.Id);
            if (data != null)
            {
                message = _placeholders.ProcessPlaceholders(message, data);
            }

            return message;
        }

        public void RegisterChatLangCommand(string command, string langKey)
        {
            HashSet<string> registeredCommands = new HashSet<string>();
            foreach (string langType in lang.GetLanguages(this))
            {
                Dictionary<string, string> langKeys = lang.GetMessages(langType, this);
                string commandValue;
                if (langKeys.TryGetValue(langKey, out commandValue) && !string.IsNullOrEmpty(commandValue) && registeredCommands.Add(commandValue))
                {
                    AddCovalenceCommand(commandValue, command);
                }
            }
        }
        
        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                [ServerLang.Format] = $"[#CCCCCC][[#{AccentColor}]{Title}[/#]] {{0}}[/#]",
                [ServerLang.NoPermission] = "You do not have permission to use this command",
                
                [ServerLang.Commands.DcCommand] = "dc",
                [ServerLang.Commands.CodeCommand] = "code",
                [ServerLang.Commands.UserCommand] = "user",
                [ServerLang.Commands.LeaveCommand] = "leave",
                [ServerLang.Commands.AcceptCommand] = "accept",
                [ServerLang.Commands.DeclineCommand] = "decline",
                
                [ServerLang.Commands.Code.LinkInfo] = $"To complete your activation please open Discord use the following command: [#{AccentColor}]/{DefaultKeys.Plugin.Lang.WithFormat(ServerLang.Discord.DiscordCommand)} {DefaultKeys.Plugin.Lang.WithFormat(ServerLang.Discord.LinkCommand)} {PlaceholderKeys.LinkCode}[/#].\n",
                [ServerLang.Commands.Code.LinkServer] = $"In order to use this command you must be in the [#{AccentColor}]{DefaultKeys.Guild.Name}[/#] discord server. " +
                                                        $"You can join @ [#{Success}]discord.gg/{PlaceholderKeys.InviteCode}[/#].\n",
                [ServerLang.Commands.Code.LinkInGuild] = $"This command can be used in the following guild channels {PlaceholderKeys.CommandChannels}.\n",
                [ServerLang.Commands.Code.LinkInDm] = $"This command can be used in the following in a direct message to {DefaultKeys.User.Fullname} bot",
                
                [ServerLang.Commands.User.MatchFound] = $"We found a match by username. " +
                                                        $"We have a sent a discord message to [#{AccentColor}]{DefaultKeys.User.Fullname}[/#] to complete the link.\n" +
                                                        $"If you haven't received a message make sure you allow DM's from [#{AccentColor}]{DefaultKeys.Bot.Fullname}[/#].",
                [ServerLang.Commands.User.Errors.InvalidSyntax] = "Invalid User Join Syntax\n. " +
                                                                  $"[#{AccentColor}]/{DefaultKeys.Plugin.Lang.WithFormat(ServerLang.Commands.DcCommand)} {DefaultKeys.Plugin.Lang.WithFormat(ServerLang.Commands.UserCommand)} username#discriminator[/#] to start the link process by your discord username\n" +
                                                                  $"[#{AccentColor}]/{DefaultKeys.Plugin.Lang.WithFormat(ServerLang.Commands.DcCommand)} {DefaultKeys.Plugin.Lang.WithFormat(ServerLang.Commands.UserCommand)} userid[/#] to start the link process by your discord user ID",
                [ServerLang.Commands.User.Errors.UserIdNotFound] = $"Failed to find a discord user in the {DefaultKeys.Guild.Name} Discord server with user ID {DefaultKeys.Snowflake.Id}",
                [ServerLang.Commands.User.Errors.UserNotFound] = $"Failed to find a any discord users in the {DefaultKeys.Guild.Name} Discord server with the username {PlaceholderKeys.NotFound}",
                [ServerLang.Commands.User.Errors.MultipleUsersFound] = $"Multiple discord users found in the the {DefaultKeys.Guild.Name} Discord server matching {PlaceholderKeys.NotFound}. " +
                                                                       "Please include more of the username and/or the discriminator in your search.",
                [ServerLang.Commands.User.Errors.SearchError] = "An error occured while trying to search by username. " +
                                                                "Please try a different username or try again later. " +
                                                                "If the issue persists please notify an admin.",
                
                [ServerLang.Commands.Leave.Errors.NotLinked] = "We were unable to unlink your account as you do not appear to have been linked.",
                
                [ServerLang.Announcements.Link.Command] = $"{DefaultKeys.Player.Name} has successfully linked their game account with their discord user {DefaultKeys.User.Fullname}. If you would would like to be linked type /dc to learn more.",
                [ServerLang.Announcements.Link.Admin] = $"{DefaultKeys.Player.Name} has successfully been unlinked by and admin from discord user {DefaultKeys.User.Fullname}.",
                [ServerLang.Announcements.Link.Api] = $"{DefaultKeys.Player.Name} has successfully linked their game account with their discord user {DefaultKeys.User.Fullname}. If you would would like to be linked type /dc to learn more.",
                [ServerLang.Announcements.Link.GuildRejoin] = $"{DefaultKeys.Player.Name} has been relinked with discord user {DefaultKeys.User.Fullname} for rejoining the {DefaultKeys.Guild.Name} discord server",
                [ServerLang.Announcements.Link.InactiveRejoin] = $"{DefaultKeys.Player.Name} has been relinked with discord user {DefaultKeys.User.Fullname} for rejoining the {DefaultKeys.Server.Name} game server",
                [ServerLang.Announcements.Unlink.Command] = $"{DefaultKeys.Player.Name} has successfully unlinked their game account from their discord user {DefaultKeys.User.Fullname}.",
                [ServerLang.Announcements.Unlink.Admin] = $"{DefaultKeys.Player.Name} has successfully been unlinked by and admin from discord user {DefaultKeys.User.Fullname}.",
                [ServerLang.Announcements.Unlink.Api] = $"{DefaultKeys.Player.Name} has successfully unlinked their game account from their discord user {DefaultKeys.User.Fullname}.",
                [ServerLang.Announcements.Unlink.LeftGuild] = $"{DefaultKeys.Player.Name} has been unlinked from discord user {DefaultKeys.User.Fullname} they left the {DefaultKeys.Guild.Name} Discord server",
                [ServerLang.Announcements.Unlink.Inactive] = $"{DefaultKeys.Player.Name} has been unlinked from discord user {DefaultKeys.User.Fullname} because they haven't been active on {DefaultKeys.Server.Name} game server for {DefaultKeys.Timespan.TotalDays} days",
                
                [ServerLang.Link.Completed.Command] = $"You have successfully linked your player {DefaultKeys.Player.Name} with discord user {DefaultKeys.User.Fullname}",
                [ServerLang.Link.Completed.Admin] = $"You have been successfully linked by an admin with player {DefaultKeys.Player.Name} and discord user {DefaultKeys.User.Fullname}",
                [ServerLang.Link.Completed.Api] = $"You have successfully linked your player {DefaultKeys.Player.Name} with discord user {DefaultKeys.User.Fullname}",
                [ServerLang.Link.Completed.GuildRejoin] = $"Your player {DefaultKeys.Player.Name} has been relinked with discord user {DefaultKeys.User.Fullname} because rejoined the {DefaultKeys.Guild.Name} Discord server",
                [ServerLang.Link.Completed.InactiveRejoin] = $"Your player {DefaultKeys.Player.Name} has been relinked with discord user {DefaultKeys.User.Fullname} because rejoined {DefaultKeys.Server.Name} server",
                [ServerLang.Unlink.Completed.Command] = $"You have successfully unlinked your player {DefaultKeys.Player.Name} from discord user {DefaultKeys.User.Fullname}",
                [ServerLang.Unlink.Completed.Admin] = $"You have been successfully unlinked by an admin from discord user {DefaultKeys.User.Fullname}",
                [ServerLang.Unlink.Completed.Api] = $"You have successfully unlinked your player {DefaultKeys.Player.Name} from discord user {DefaultKeys.User.Fullname}",
                [ServerLang.Unlink.Completed.LeftGuild] = $"Your player {DefaultKeys.Player.Name} has been unlinked from discord user {DefaultKeys.User.Fullname} because you left the {DefaultKeys.Guild.Name} Discord server",

                [ServerLang.Link.Declined.JoinWithPlayer] = $"We have declined the discord link between {DefaultKeys.Player.Name} and {DefaultKeys.User.Fullname}",
                [ServerLang.Link.Declined.JoinWithUser] = $"{DefaultKeys.User.Fullname} has declined your link to {DefaultKeys.Player.Name}",
                
                [ServerLang.Link.Errors.InvalidSyntax] = "Invalid Link Syntax. Please type the command you were given in Discord. " +
                                                         "Command should be in the following format:" +
                                                         $"[#{AccentColor}]/{DefaultKeys.Plugin.Lang.WithFormat(ServerLang.Commands.DcCommand)} {PlaceholderKeys.ServerLinkArg} {{code}}[/#] where {{code}} is the code sent to you in Discord.",

                [ServerLang.Banned.IsUserBanned] = "You have been banned from joining by Discord user due to multiple declined join attempts. " +
                                                   $"Your ban will end in {DefaultKeys.Timespan.Days} days {DefaultKeys.Timespan.Hours} hours {DefaultKeys.Timespan.Minutes} minutes {DefaultKeys.Timespan.Seconds} Seconds.",

                [ServerLang.Join.ByPlayer] = $"{DefaultKeys.User.Fullname} is trying to link their Discord account with your game account. " +
                                             $"If you wish to [#{Success}]accept[/#] this link please type [#{Success}]/{DefaultKeys.Plugin.Lang.WithFormat(ServerLang.Commands.DcCommand)} {DefaultKeys.Plugin.Lang.WithFormat(ServerLang.Commands.AcceptCommand)}[/#]. " +
                                             $"If you wish to [#{Danger}]decline[/#] this link please type [#{Danger}]/{DefaultKeys.Plugin.Lang.WithFormat(ServerLang.Commands.DcCommand)} {DefaultKeys.Plugin.Lang.WithFormat(ServerLang.Commands.DeclineCommand)}[/#]",
                [ServerLang.Discord.DiscordCommand] = "dc",
                [ServerLang.Discord.LinkCommand] = "link",

                [ServerLang.Join.Errors.PlayerJoinActivationNotFound] = "There are no pending joins in progress for this game account. Please start the link in Discord and try again.",
                
                [ServerLang.Errors.PlayerAlreadyLinked] = $"This player is already linked to Discord user {DefaultKeys.User.Fullname}. " +
                                                          $"If you wish to link yourself to another account please type [#{AccentColor}]/{DefaultKeys.Plugin.Lang.WithFormat(ServerLang.Commands.DcCommand)} {DefaultKeys.Plugin.Lang.WithFormat(ServerLang.Commands.LeaveCommand)}[/#]",
                [ServerLang.Errors.DiscordAlreadyLinked] = $"This Discord user is already linked to player {DefaultKeys.Player.Name}. " +
                                                           $"If you wish to link yourself to another account please type [#{AccentColor}]/{DefaultKeys.Plugin.Lang.WithFormat(ServerLang.Commands.DcCommand)} {DefaultKeys.Plugin.Lang.WithFormat(ServerLang.Commands.LeaveCommand)}[/#]",
                [ServerLang.Errors.ActivationNotFound] = $"We failed to find any pending joins with code [#{AccentColor}]/{DefaultKeys.Plugin.Lang.WithFormat(ServerLang.Commands.DcCommand)}[/#]. " +
                                                          "Please verify the code is correct and try again.",
                [ServerLang.Errors.MustBeCompletedInDiscord] = "You need to complete the steps provided in Discord since you started the link from the game server.",
                [ServerLang.Errors.ConsolePlayerNotSupported] = "This command cannot be ran in the server console. ",
                
                [ServerLang.Commands.HelpMessage] = "Allows players to link their player and discord accounts together. " +
                                                    $"Players must first join the {DefaultKeys.Guild.Name} Discord @ [#{AccentColor}]discord.gg/{PlaceholderKeys.InviteCode}[/#]\n" +
                                                    $"[#{AccentColor}]/{DefaultKeys.Plugin.Lang.WithFormat(ServerLang.Commands.DcCommand)} {DefaultKeys.Plugin.Lang.WithFormat(ServerLang.Commands.CodeCommand)}[/#] to start the link process using a code\n" +
                                                    $"[#{AccentColor}]/{DefaultKeys.Plugin.Lang.WithFormat(ServerLang.Commands.DcCommand)} {DefaultKeys.Plugin.Lang.WithFormat(ServerLang.Commands.UserCommand)} username#discriminator[/#] to start the link process by your discord username\n" +
                                                    $"[#{AccentColor}]/{DefaultKeys.Plugin.Lang.WithFormat(ServerLang.Commands.DcCommand)} {DefaultKeys.Plugin.Lang.WithFormat(ServerLang.Commands.UserCommand)} userid[/#] to start the link process by your discord user ID\n" +
                                                    $"[#{AccentColor}]/{DefaultKeys.Plugin.Lang.WithFormat(ServerLang.Commands.DcCommand)} {DefaultKeys.Plugin.Lang.WithFormat(ServerLang.Commands.LeaveCommand)}[/#] to to unlink yourself from discord\n" +
                                                    $"[#{AccentColor}]/{DefaultKeys.Plugin.Lang.WithFormat(ServerLang.Commands.DcCommand)}[/#] to see this message again",
            }, this);
        }
    }
}