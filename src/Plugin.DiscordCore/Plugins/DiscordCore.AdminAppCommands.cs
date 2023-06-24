using DiscordCorePlugin.AppCommands;
using DiscordCorePlugin.Enums;
using DiscordCorePlugin.Templates;
using Oxide.Core.Libraries.Covalence;
using Oxide.Ext.Discord.Attributes.ApplicationCommands;
using Oxide.Ext.Discord.Builders.ApplicationCommands;
using Oxide.Ext.Discord.Cache;
using Oxide.Ext.Discord.Entities.Interactions;
using Oxide.Ext.Discord.Entities.Interactions.ApplicationCommands;
using Oxide.Ext.Discord.Entities.Permissions;
using Oxide.Ext.Discord.Entities.Users;
using Oxide.Ext.Discord.Extensions;
using Oxide.Ext.Discord.Libraries.Templates;
using Oxide.Ext.Discord.Libraries.Templates.Commands;

namespace DiscordCorePlugin.Plugins
{
    //Define:FileOrder=45
    public partial class DiscordCore
    {
        public void RegisterAdminApplicationCommands()
        {
            ApplicationCommandBuilder builder = new ApplicationCommandBuilder(AdminAppCommands.Command, "Discord Core Admin Commands", ApplicationCommandType.ChatInput)
                .AddDefaultPermissions(PermissionFlags.Administrator);
            
            AddAdminLinkCommand(builder);
            AddAdminUnlinkCommand(builder);
            AddAdminSearchGroupCommand(builder);

            CommandCreate build = builder.Build();
            DiscordCommandLocalization localization = builder.BuildCommandLocalization();

            _local.RegisterCommandLocalizationAsync(this, "Admin", localization, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0)).Then(() =>
            {
                _local.ApplyCommandLocalizationsAsync(this, build, "Admin").Then(() =>
                {
                    Client.Bot.Application.CreateGlobalCommand(Client, build);
                });
            });
        }

        public void AddAdminLinkCommand(ApplicationCommandBuilder builder)
        {
            builder.AddSubCommand(AdminAppCommands.LinkCommand, "admin link player game account and Discord user", sub =>
            {
                sub.AddOption(CommandOptionType.String, PlayerArg, "player to link",
                    options => options.AutoComplete().Required());

                sub.AddOption(CommandOptionType.User, UserArg, "user to link",
                    options => options.Required());
            });
        }

        public void AddAdminUnlinkCommand(ApplicationCommandBuilder builder)
        {
            builder.AddSubCommand(AdminAppCommands.UnlinkCommand, "admin unlink player game account and Discord user", sub =>
            {
                sub.AddOption(CommandOptionType.String, PlayerArg, "player to unlink",
                    options => options.AutoComplete());

                sub.AddOption(CommandOptionType.User, UserArg, "user to unlink");
            });
        }

        public void AddAdminSearchGroupCommand(ApplicationCommandBuilder builder)
        {
            builder.AddSubCommandGroup(AdminAppCommands.SearchCommand, "search linked accounts by discord or player", group =>
            {
                AddAdminSearchByPlayerCommand(group);
                AddAdminSearchByUserCommand(group);
            });
        }

        public void AddAdminSearchByPlayerCommand( ApplicationCommandGroupBuilder builder)
        {
            builder.AddSubCommand(AdminAppCommands.PlayerCommand, "search by player", sub =>
            {
                sub.AddOption(CommandOptionType.String, PlayerArg, "player to search",
                    options => options.AutoComplete());
            });
        }
        
        public void AddAdminSearchByUserCommand( ApplicationCommandGroupBuilder builder)
        {
            builder.AddSubCommand(AdminAppCommands.UserCommand, "search by user", sub =>
            {
                sub.AddOption(CommandOptionType.User, UserArg, "user to search");
            });
        }

        [DiscordApplicationCommand(AdminAppCommands.Command, AdminAppCommands.LinkCommand)]
        private void DiscordAdminLinkCommand(DiscordInteraction interaction, InteractionDataParsed parsed)
        {
            string playerId = parsed.Args.GetString(PlayerArg);
            DiscordUser user = parsed.Args.GetUser(UserArg);
            IPlayer player = players.FindPlayerById(playerId);
            if (player == null)
            {
                SendTemplateMessage(TemplateKeys.Commands.Admin.Link.Error.PlayerNotFound, interaction, GetDefault(ServerPlayerCache.Instance.GetPlayerById(playerId), user));
                return;
            }
            
            if (player.IsLinked())
            {
                SendTemplateMessage(TemplateKeys.Commands.Admin.Link.Error.PlayerAlreadyLinked, interaction, GetDefault(player, user));
                return;
            }
            
            if (user.IsLinked())
            {
                SendTemplateMessage(TemplateKeys.Commands.Admin.Link.Error.UserAlreadyLinked, interaction, GetDefault(player, user));
                return;
            }

            _linkHandler.HandleLink(player, user, LinkReason.Admin, null);
            SendTemplateMessage(TemplateKeys.Commands.Admin.Link.Success, interaction, GetDefault(player, user));
        }

        [DiscordApplicationCommand(AdminAppCommands.Command, AdminAppCommands.UnlinkCommand)]
        private void DiscordAdminUnlinkCommand(DiscordInteraction interaction, InteractionDataParsed parsed)
        {
            string playerId = parsed.Args.GetString(PlayerArg);
            IPlayer player = players.FindPlayerById(playerId);
            DiscordUser user = parsed.Args.GetUser(UserArg);

            if (player == null && user == null)
            {
                SendTemplateMessage(TemplateKeys.Commands.Admin.Unlink.Error.MustSpecifyOne, interaction, GetDefault(ServerPlayerCache.Instance.GetPlayerById(playerId)));
                return;
            }
            
            bool isPlayerLinked = player?.IsLinked() ?? false;
            bool isUserLinked = user?.IsLinked() ?? false;

            if (player != null && !isPlayerLinked)
            {
                SendTemplateMessage(TemplateKeys.Commands.Admin.Unlink.Error.PlayerIsNotLinked, interaction, GetDefault(player));
                return;
            }
            
            if (user != null && !isUserLinked)
            {
                SendTemplateMessage(TemplateKeys.Commands.Admin.Unlink.Error.UserIsNotLinked, interaction, GetDefault(user));
                return;
            }

            DiscordUser linkedUser = player.GetDiscordUser();
            if (player != null && user != null && linkedUser.Id != user.Id)
            {
                IPlayer otherPlayer = user.Player;
                SendTemplateMessage(TemplateKeys.Commands.Admin.Unlink.Error.LinkNotSame, interaction, GetDefault(player, user).Add(OtherPlayerDataKey, otherPlayer).Add(OtherUserDataKey, linkedUser));
                return;
            }

            if (player != null && user == null)
            {
                user = player.GetDiscordUser();
            }
            else if (user != null && player == null)
            {
                player = user.Player;
            }
            
            _linkHandler.HandleUnlink(player, user, UnlinkedReason.Admin, null);
            SendTemplateMessage(TemplateKeys.Commands.Admin.Unlink.Success, interaction, GetDefault(player, user));
        }

        [DiscordApplicationCommand(AdminAppCommands.Command, AdminAppCommands.PlayerCommand, AdminAppCommands.SearchCommand)]
        private void DiscordAdminSearchByPlayer(DiscordInteraction interaction, InteractionDataParsed parsed)
        {
            string playerId = parsed.Args.GetString(PlayerArg);
            IPlayer player = !string.IsNullOrEmpty(playerId) ? players.FindPlayerById(playerId) : null;
            if (player == null)
            {
                SendTemplateMessage(TemplateKeys.Commands.Admin.Search.Error.PlayerNotFound, interaction, GetDefault(ServerPlayerCache.Instance.GetPlayerById(playerId)));
                return;
            }

            DiscordUser user = player.GetDiscordUser();
            SendTemplateMessage(TemplateKeys.Commands.Admin.Search.Success, interaction, GetDefault(player, user));
        }
        
        [DiscordApplicationCommand(AdminAppCommands.Command, AdminAppCommands.UserCommand, AdminAppCommands.SearchCommand)]
        private void DiscordAdminSearchByUser(DiscordInteraction interaction, InteractionDataParsed parsed)
        {
            DiscordUser user = parsed.Args.GetUser(UserArg);
            IPlayer player = user.Player;
            SendTemplateMessage(TemplateKeys.Commands.Admin.Search.Success, interaction, GetDefault(player, user));
        }
    }
}