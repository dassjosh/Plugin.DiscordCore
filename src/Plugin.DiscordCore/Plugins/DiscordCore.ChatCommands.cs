using System;
using System.Collections.Generic;
using DiscordCorePlugin.Enums;
using DiscordCorePlugin.Link;
using DiscordCorePlugin.Localization;
using DiscordCorePlugin.Placeholders;
using DiscordCorePlugin.Templates;
using Oxide.Core.Libraries.Covalence;
using Oxide.Ext.Discord.Entities;
using Oxide.Ext.Discord.Extensions;
using Oxide.Ext.Discord.Libraries;

namespace DiscordCorePlugin.Plugins
{
    //Define:FileOrder=20
    public partial class DiscordCore
    {
        // ReSharper disable once UnusedParameter.Local
        private void DiscordCoreChatCommand(IPlayer player, string cmd, string[] args)
        {
            if (!player.HasPermission(UsePermission))
            {
                Chat(player, ServerLang.NoPermission);
                return;
            }
            
            if (player.Id == "server_console")
            {
                Chat(player, ServerLang.Errors.ConsolePlayerNotSupported, GetDefault(player));
                return;
            }

            if (args.Length == 0)
            {
                DisplayHelp(player);
                return;
            }

            string subCommand = args[0];
            if (subCommand.Equals(Lang(ServerLang.Commands.CodeCommand, player), StringComparison.OrdinalIgnoreCase))
            {
                HandleServerCodeJoin(player);
                return;
            }
            
            if (subCommand.Equals(Lang(ServerLang.Commands.UserCommand, player), StringComparison.OrdinalIgnoreCase))
            {
                HandleServerUserJoin(player, args);
                return;
            }
            
            if (subCommand.Equals(Lang(ServerLang.Commands.LeaveCommand, player), StringComparison.OrdinalIgnoreCase))
            {
                HandleServerLeave(player);
                return;
            }
            
            if (subCommand.Equals(Lang(ServerLang.Commands.AcceptCommand, player), StringComparison.OrdinalIgnoreCase))
            {
                HandleUserJoinAccept(player);
                return;
            }
            
            if (subCommand.Equals(Lang(ServerLang.Commands.DeclineCommand, player), StringComparison.OrdinalIgnoreCase))
            {
                HandleUserJoinDecline(player);
                return;
            }

            if (subCommand.Equals(Lang(ServerLang.Commands.LinkCommand, player), StringComparison.OrdinalIgnoreCase))
            {
                HandleServerCompleteLink(player, args);
                return;
            }
            
            DisplayHelp(player);
        }

        public void DisplayHelp(IPlayer player)
        {
            Chat(player, ServerLang.Commands.HelpMessage, GetDefault(player));
        }
        
        public void HandleServerCodeJoin(IPlayer player)
        {
            if (player.IsLinked())
            {
                Chat(player, ServerLang.Errors.PlayerAlreadyLinked, GetDefault(player, player.GetDiscordUser()));
                return;
            }
            
            //Puts("A");

            JoinData join = _joinHandler.CreateActivation(player);
            //Puts("B");
            using (PlaceholderData data = GetDefault(player).AddUser(_bot).Add(PlaceholderDataKeys.Code, join.Code))
            {
                data.ManualPool();
                _sb.Clear();
                _sb.Append(LangPlaceholder(ServerLang.Commands.Code.LinkInfo, data));
                _sb.Append(LangPlaceholder(ServerLang.Commands.Code.LinkServer, data));
                if (!string.IsNullOrEmpty(_allowedChannels))
                {
                    _sb.Append(LangPlaceholder(ServerLang.Commands.Code.LinkInGuild, data));
                }

                if (_appCommand?.DmPermission is true)
                {
                    _sb.Append(LangPlaceholder(ServerLang.Commands.Code.LinkInDm, data));
                }
            }

            Chat(player, _sb.ToString());
        }
        
        public void HandleServerUserJoin(IPlayer player, string[] args)
        {
            if (player.IsLinked())
            {
                Chat(player, ServerLang.Errors.PlayerAlreadyLinked, GetDefault(player, player.GetDiscordUser()));
                return;
            }

            if (_banHandler.IsBanned(player))
            {
                Chat(player, ServerLang.Banned.IsUserBanned, GetDefault(player).AddTimeSpan(_banHandler.GetRemainingDuration(player)));
                return;
            }

            if (args.Length < 2)
            {
                Chat(player, ServerLang.Commands.User.Errors.InvalidSyntax, GetDefault(player));
                return;
            }

            string search = args[1];
            
            Snowflake id;
            if (Snowflake.TryParse(search, out id))
            {
                GuildMember member = Guild.Members[id];
                if (member == null)
                {
                    Chat(player, ServerLang.Commands.User.Errors.UserIdNotFound, GetDefault(player).AddSnowflake(id));
                    return;
                }
                
                UserSearchMatchFound(player, member.User);
                return;
            }

            int discriminatorIndex = search.LastIndexOf('#');
            string userName;
            string discriminator;
            if (discriminatorIndex == -1)
            {
                userName = search;
                discriminator = null;
            }
            else
            {
                userName = search.Substring(0, discriminatorIndex);
                discriminator = search.Substring(discriminatorIndex, search.Length - discriminatorIndex);
            }

            GuildSearchMembers guildSearch = new()
            {
                Query = userName,
                Limit = 1000
            };

            Guild.SearchMembers(Client, guildSearch).Then(members =>
            {
                HandleChatJoinUserResults(player, members, userName, discriminator);
            }).Catch(error =>
            {
                Chat(player, ServerLang.Commands.User.Errors.SearchError, GetDefault(player));
            });
        }
        
        public void HandleChatJoinUserResults(IPlayer player, List<GuildMember> members, string userName, string discriminator)
        {
            if (members.Count == 0)
            {
                string name = !string.IsNullOrEmpty(discriminator) ? $"{userName}#{discriminator}" : userName;
                Chat(player, ServerLang.Commands.User.Errors.UserNotFound, GetDefault(player).Add(PlaceholderDataKeys.NotFound, name));
                return;
            }

            if (members.Count == 1)
            {
                UserSearchMatchFound(player, members[0].User);
                return;
            }
            
            DiscordUser user = null;

            int count = 0;
            for (int index = 0; index < members.Count; index++)
            {
                GuildMember member = members[index];
                DiscordUser searchUser = member.User;
                if (discriminator == null)
                {
                    if (searchUser.Username.StartsWith(userName, StringComparison.OrdinalIgnoreCase))
                    {
                        user = searchUser;
                        count++;
                        if (count > 1)
                        {
                            break;
                        }
                    }
                }
#pragma warning disable CS0618
                else if (searchUser.Username.Equals(userName, StringComparison.OrdinalIgnoreCase) && (searchUser.HasUpdatedUsername || searchUser.Discriminator.Equals(discriminator)))
#pragma warning restore CS0618
                {
                    user = searchUser;
                    break;
                }
            }

            if (user == null || count > 1)
            {
                string name = !string.IsNullOrEmpty(discriminator) ? $"{userName}#{discriminator}" : userName;
                Chat(player, ServerLang.Commands.User.Errors.MultipleUsersFound, GetDefault(player).Add(PlaceholderDataKeys.NotFound, name));
                return;
            }

            UserSearchMatchFound(player, user);
        }

        public void UserSearchMatchFound(IPlayer player, DiscordUser user)
        {
            _joinHandler.CreateActivation(player, user, JoinSource.Server);
            using (PlaceholderData data = GetDefault(player, user))
            {
                data.ManualPool();
                Chat(player, ServerLang.Commands.User.MatchFound, data);
                SendTemplateMessage(TemplateKeys.Join.CompleteLink, user, player, data);
            }
        }

        public void HandleServerLeave(IPlayer player)
        {
            DiscordUser user = player.GetDiscordUser();
            if (user == null)
            {
                Chat(player, ServerLang.Commands.Leave.Errors.NotLinked, GetDefault(player));
                return;
            }
            
            _linkHandler.HandleUnlink(player, user, UnlinkedReason.Command, null);
        }
        
        public void HandleUserJoinAccept(IPlayer player)
        {
            JoinData join = _joinHandler.FindCompletedByPlayer(player);
            if (join == null)
            {
                Chat(player, ServerLang.Join.Errors.PlayerJoinActivationNotFound, GetDefault(player));
                return;
            }

            if (join.From == JoinSource.Server)
            {
                Chat(player, ServerLang.Errors.MustBeCompletedInDiscord, GetDefault(player));
                return;
            }

            _joinHandler.CompleteLink(join, null);
        }
        
        public void HandleUserJoinDecline(IPlayer player)
        {
            JoinData join = _joinHandler.FindCompletedByPlayer(player);
            if (join == null)
            {
                Chat(player, ServerLang.Join.Errors.PlayerJoinActivationNotFound, GetDefault(player));
                return;
            }
            
            _joinHandler.DeclineLink(join, null);
        }
        
        public void HandleServerCompleteLink(IPlayer player, string[] args)
        {
            if (player.IsLinked())
            {
                Chat(player, ServerLang.Errors.PlayerAlreadyLinked, GetDefault(player, player.GetDiscordUser()));
                return;
            }

            if (args.Length < 2)
            {
                Chat(player, ServerLang.Link.Errors.InvalidSyntax, GetDefault(player));
                return;
            }

            string code = args[1];
            JoinData join = _joinHandler.FindByCode(code);
            if (join == null)
            {
                Chat(player, ServerLang.Errors.MustBeCompletedInDiscord, GetDefault(player));
                return;
            }

            if (join.From == JoinSource.Server)
            {
                Chat(player, ServerLang.Errors.MustBeCompletedInDiscord, GetDefault(player));
                return;
            }

            if (join.Discord.IsLinked())
            {
                Chat(player, ServerLang.Errors.DiscordAlreadyLinked, GetDefault(player, join.Discord));
                return;
            }

            join.Player = player;
            _joinHandler.CompleteLink(join, null);
        }
    }
}