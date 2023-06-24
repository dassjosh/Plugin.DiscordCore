using System;
using System.Collections.Generic;
using DiscordCorePlugin.Localization;
using Oxide.Core.Libraries.Covalence;
using Oxide.Ext.Discord.Libraries.Placeholders;

namespace DiscordCorePlugin.Plugins
{
    //Define:FileOrder=15
    public partial class DiscordCore
    {
        public void Chat(IPlayer player, string key)
        {
            if (player.IsConnected)
            {
                player.Reply(Lang(ServerLang.Format, player, Lang(key, player)));
            }
        }
        
        public void Chat(IPlayer player, string key, params object[] args)
        {
            if (player.IsConnected)
            {
                player.Reply(Lang(ServerLang.Format, player, Lang(key, player, args)));
            }
        }
        
        public void Chat(IPlayer player, string key, PlaceholderData data)
        {
            if (player.IsConnected)
            {
                player.Reply(Lang(ServerLang.Format, player, _placeholders.ProcessPlaceholders(Lang(key, player), data)));
            }
        }
        
        public void Chat(IPlayer player, string key, PlaceholderData data, params object[] args)
        {
            if (!player.IsConnected) return;
            
            try
            {
                string placeholder = Lang(key, player);
                placeholder = _placeholders.ProcessPlaceholders(placeholder, data);
                placeholder = string.Format(placeholder, args);
                player.Reply(Lang(ServerLang.Format, player, placeholder));
            }
            catch (Exception ex)
            {
                PrintError($"Placeholder Lang Key '{key}' threw exception\n:{ex}");
                throw;
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
            
            message = Lang(ServerLang.Format, null, message);
            server.Broadcast(message);
        }

        public string Lang(string key, IPlayer player = null)
        {
            return lang.GetMessage(key, this, player?.Id);
        }

        public string Lang(string key, IPlayer player = null, params object[] args)
        {
            try
            {
                return string.Format(Lang(key, player), args);
            }
            catch(Exception ex)
            {
                PrintError($"Lang Key '{key}' threw exception\n:{ex}");
                throw;
            }
        }

        public void RegisterChatLangCommand(string command, string langKey)
        {
            foreach (string langType in lang.GetLanguages(this))
            {
                Dictionary<string, string> langKeys = lang.GetMessages(langType, this);
                string commandValue;
                if (langKeys.TryGetValue(langKey, out commandValue) && !string.IsNullOrEmpty(commandValue))
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
                
                [ServerLang.Commands.Code.LinkInfo] = $"To complete your activation please open Discord use the following command: <color=#{AccentColor}>/{{plugin.lang:{ServerLang.Discord.DiscordCommand}}} {{plugin.lang:{ServerLang.Discord.LinkCommand}}} {{discordcore.link.code}}</color>.\n",
                [ServerLang.Commands.Code.LinkServer] = $"In order to use this command you must be in the <color=#{AccentColor}>{{guild.name}}</color> discord server. " +
                                                        $"You can join @ <color=#{Success}>discord.gg/{{discordcore.invite.code}}</color>.\n",
                [ServerLang.Commands.Code.LinkInGuild] = "This command can be used in the following guild channels {dc.command.channels} .\n",
                [ServerLang.Commands.Code.LinkInDm] = "This command can be used in the following in a direct message to {user.fullname} bot",
                
                [ServerLang.Commands.User.Errors.InvalidSyntax] = "Invalid User Join Syntax\n. " +
                                                                  $"[#{AccentColor}]/{{plugin.lang:{ServerLang.Commands.DcCommand}}} {{plugin.lang:{ServerLang.Commands.UserCommand}}} username#discriminator[/#] to start the link process by your discord username\n" +
                                                                  $"[#{AccentColor}]/{{plugin.lang:{ServerLang.Commands.DcCommand}}} {{plugin.lang:{ServerLang.Commands.UserCommand}}} userid[/#] to start the link process by your discord user ID",
                [ServerLang.Commands.User.Errors.UserIdNotFound] = "Failed to find a discord user in the {guild.name} Discord server with user ID {snowflake.id}",
                [ServerLang.Commands.User.Errors.UserNotFound] = "Failed to find a any discord users in the {guild.name} Discord server with the username {0}",
                [ServerLang.Commands.User.Errors.MultipleUsersFound] = "Multiple discord users found in the the {guild.name} Discord server matching {0}. " +
                                                                       "Please include more of the username and/or the discriminator in your search.",
                [ServerLang.Commands.User.Errors.SearchError] = "An error occured while trying to search by username. " +
                                                                "Please try a different username or try again later. " +
                                                                "If the issue persists please notify an admin.",
                
                [ServerLang.Commands.Leave.Errors.NotLinked] = "We were unable to unlink your account as you do not appear to have been linked.",
                
                [ServerLang.Announcements.Link.Command] = "{player.name} has successfully linked their game account with their discord user {user.fullname}. If you would would like to be linked type /dc to learn more.",
                [ServerLang.Announcements.Link.Admin] = "{player.name} has successfully been unlinked by and admin from discord user {user.fullname}.",
                [ServerLang.Announcements.Link.Api] = "{player.name} has successfully linked their game account with their discord user {user.fullname}. If you would would like to be linked type /dc to learn more.",
                [ServerLang.Announcements.Link.GuildRejoin] = "{player.name} has been relinked with discord user {user.fullname} for rejoining the {guild.name} discord server",
                [ServerLang.Announcements.Link.InactiveRejoin] = "{player.name} has been relinked with discord user {user.fullname} for rejoining the {server.name} game server",
                [ServerLang.Announcements.Unlink.Command] = "{player.name} has successfully unlinked their game account from their discord user {user.fullname}.",
                [ServerLang.Announcements.Unlink.Admin] = "{player.name} has successfully been unlinked by and admin from discord user {user.fullname}.",
                [ServerLang.Announcements.Unlink.Api] = "{player.name} has successfully unlinked their game account from their discord user {user.fullname}.",
                [ServerLang.Announcements.Unlink.LeftGuild] = "{player.name} has been unlinked from discord user {user.fullname} they left the {guild.name} Discord server",
                [ServerLang.Announcements.Unlink.Inactive] = "{player.name} has been unlinked from discord user {user.fullname} because they haven't been active on {server.name} game server for {discordcore.inactive.duration} days",
                
                [ServerLang.Link.Completed.Command] = "You have successfully linked your player {player.name} with discord user {user.fullname}",
                [ServerLang.Link.Completed.Admin] = "You have been successfully linked by an admin with player {player.name} and discord user {user.fullname}",
                [ServerLang.Link.Completed.Api] = "You have successfully linked your player {player.name} with discord user {user.fullname}",
                [ServerLang.Link.Completed.GuildRejoin] = "Your player {player.name} has been relinked with discord user {user.fullname} because rejoined the {guild.name} Discord server",
                [ServerLang.Link.Completed.InactiveRejoin] = "Your player {player.name} has been relinked with discord user {user.fullname} because rejoined {server.name} server",
                [ServerLang.Unlink.Completed.Command] = "You have successfully unlinked your player {player.name} from discord user {user.fullname}",
                [ServerLang.Unlink.Completed.Admin] = "You have been successfully unlinked by an admin from discord user {user.fullname}",
                [ServerLang.Unlink.Completed.Api] = "You have successfully unlinked your player {player.name} from discord user {user.fullname}",
                [ServerLang.Unlink.Completed.LeftGuild] = "Your player {player.name} has been unlinked from discord user {user.fullname} because you left the {guild.name} Discord server",

                [ServerLang.Link.Declined.JoinWithPlayer] = "We have declined the discord link between {player.name} and {user.fullname}",
                [ServerLang.Link.Declined.JoinWithUser] = "{user.fullname} has declined your link to {player.name}",
                
                [ServerLang.Link.Errors.InvalidSyntax] = "Invalid Link Syntax. Please type the command you were given in Discord. " +
                                                         "Command should be in the following format:" +
                                                         $"[#{AccentColor}]/{{plugin.lang:{ServerLang.Commands.DcCommand}}} {{discordcore.server.link.arg}} {{code}}[/#] where {{code}} is the code sent to you in Discord.",

                [ServerLang.Banned.IsUserBanned] = "You have been banned from joining by Discord user due to multiple declined join attempts. " +
                                                   "Your ban will end in {discordcore.join.banned.duration:d} days {discordcore.join.banned.duration:h} hours {discordcore.join.banned.duration:m} minutes {discordcore.join.banned.duration:s} Seconds.",

                [ServerLang.Join.ByPlayer] = "{user.fullname} is trying to link their Discord account with your game account. " +
                                             $"If you wish to [#{Success}]accept[/#] this link please type [#{Success}]/{{plugin.lang:{ServerLang.Commands.DcCommand}}} {{plugin.lang:{ServerLang.Commands.AcceptCommand}}}[/#]. " +
                                             $"If you wish to [#{Danger}]decline[/#] this link please type [#{Danger}]/{{plugin.lang:{ServerLang.Commands.DcCommand}}} {{plugin.lang:{ServerLang.Commands.DeclineCommand}}}[/#]",
                [ServerLang.Discord.DiscordCommand] = "dc",
                [ServerLang.Discord.LinkCommand] = "link",

                [ServerLang.Join.Errors.PlayerJoinActivationNotFound] = "There are no pending joins in progress for this game account. Please start the link in Discord and try again.",
                
                [ServerLang.Errors.PlayerAlreadyLinked] = "This player is already linked to Discord user {user.fullname}. " +
                                                          $"If you wish to link yourself to another account please type [#{AccentColor}]/{{plugin.lang:{ServerLang.Commands.DcCommand}}} {{plugin.lang:{ServerLang.Commands.LeaveCommand}}}[/#]",
                [ServerLang.Errors.DiscordAlreadyLinked] = "This Discord user is already linked to player {player.name}. " +
                                                           $"If you wish to link yourself to another account please type [#{AccentColor}]/{{plugin.lang:{ServerLang.Commands.DcCommand}}} {{plugin.lang:{ServerLang.Commands.LeaveCommand}}}[/#]",
                [ServerLang.Errors.ActivationNotFound] = $"We failed to find any pending joins with code [#{AccentColor}]/{{plugin.lang:{ServerLang.Commands.DcCommand}}}[/#]. " +
                                                          "Please verify the code is correct and try again.",
                [ServerLang.Errors.MustBeCompletedInDiscord] = "You need to complete the steps provided in Discord since you started the link from the game server.",
                [ServerLang.Errors.ConsolePlayerNotSupported] = "This command cannot be ran in the server console. ",
                
                [ServerLang.Commands.HelpMessage] = "Allows players to link their player and discord accounts together. " +
                                                    $"Players must first join the {{guild.name}} Discord @ [#{AccentColor}]discord.gg/{{discordcore.invite.code}}[/#]\n" +
                                                    $"[#{AccentColor}]/{{plugin.lang:{ServerLang.Commands.DcCommand}}} {{plugin.lang:{ServerLang.Commands.CodeCommand}}}[/#] to start the link process using a code\n" +
                                                    $"[#{AccentColor}]/{{plugin.lang:{ServerLang.Commands.DcCommand}}} {{plugin.lang:{ServerLang.Commands.UserCommand}}} username#discriminator[/#] to start the link process by your discord username\n" +
                                                    $"[#{AccentColor}]/{{plugin.lang:{ServerLang.Commands.DcCommand}}} {{plugin.lang:{ServerLang.Commands.UserCommand}}} userid[/#] to start the link process by your discord user ID\n" +
                                                    $"[#{AccentColor}]/{{plugin.lang:{ServerLang.Commands.DcCommand}}} {{plugin.lang:{ServerLang.Commands.LeaveCommand}}}[/#] to to unlink yourself from discord\n" +
                                                    $"[#{AccentColor}]/{{plugin.lang:{ServerLang.Commands.DcCommand}}}[/#] to see this message again",
            }, this);
        }
    }
}