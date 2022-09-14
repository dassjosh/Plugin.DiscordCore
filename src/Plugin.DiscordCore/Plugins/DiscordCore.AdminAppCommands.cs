using DiscordCorePlugin.AppCommands;
using DiscordCorePlugin.Enums;
using DiscordCorePlugin.Localization;
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

namespace DiscordCorePlugin.Plugins
{
    //Define:FileOrder=45
    public partial class DiscordCore
    {
        public void RegisterAdminApplicationCommands()
        {
            ApplicationCommandBuilder builder = new ApplicationCommandBuilder(AdminAppCommands.Command, "Discord Core Admin Commands", ApplicationCommandType.ChatInput)
                                                .AddDefaultPermissions(PermissionFlags.None)
                                                .AddNameLocalizations(this, AdminAppCommandKeys.Command)
                                                .AddDescriptionLocalizations(this, AdminAppCommandKeys.Description);
            AddAdminLinkCommand(builder);
            AddAdminUnlinkCommand(builder);
            AddAdminSearchGroupCommand(builder);

            Client.Bot.Application.CreateGlobalCommand(Client, builder.Build());
        }

        public void AddAdminLinkCommand(ApplicationCommandBuilder builder)
        {
            builder.AddSubCommand(AdminAppCommands.Link.Command, AdminAppCommands.Link.Description)
                   .AddNameLocalizations(this, AdminAppCommandKeys.Link.Command)
                   .AddDescriptionLocalizations(this, AdminAppCommandKeys.Link.Description)

                   .AddOption(CommandOptionType.String, AdminAppCommands.Link.Args.Player.Name, AdminAppCommands.Link.Args.Player.Description)
                   .AutoComplete()
                   .Required()
                   .AddNameLocalizations(this, AdminAppCommandKeys.Link.Args.Player.Name)
                   .AddDescriptionLocalizations(this, AdminAppCommandKeys.Link.Args.Player.Description)
                   .Build()

                   .AddOption(CommandOptionType.User, AdminAppCommands.Link.Args.User.Name, AdminAppCommands.Link.Args.User.Description)
                   .Required()
                   .AddNameLocalizations(this, AdminAppCommandKeys.Link.Args.User.Name)
                   .AddDescriptionLocalizations(this, AdminAppCommandKeys.Link.Args.User.Description)
                   .Build();
        }

        public void AddAdminUnlinkCommand(ApplicationCommandBuilder builder)
        {
            builder.AddSubCommand(AdminAppCommands.Unlink.Command, AdminAppCommands.Unlink.Description)
                   .AddNameLocalizations(this, AdminAppCommandKeys.Unlink.Command)
                   .AddDescriptionLocalizations(this, AdminAppCommandKeys.Unlink.Description)

                   .AddOption(CommandOptionType.String, AdminAppCommands.Unlink.Args.Player.Name, AdminAppCommands.Unlink.Args.Player.Description)
                   .AutoComplete()
                   .AddNameLocalizations(this, AdminAppCommandKeys.Unlink.Args.Player.Name)
                   .AddDescriptionLocalizations(this, AdminAppCommandKeys.Unlink.Args.Player.Description)
                   .Build()

                   .AddOption(CommandOptionType.User, AdminAppCommands.Unlink.Args.User.Name, AdminAppCommands.Unlink.Args.User.Name)
                   .AddNameLocalizations(this, AdminAppCommandKeys.Unlink.Args.User.Name)
                   .AddDescriptionLocalizations(this, AdminAppCommandKeys.Unlink.Args.User.Description)
                   .Build();
        }

        public void AddAdminSearchGroupCommand(ApplicationCommandBuilder builder)
        {
            SubCommandGroupBuilder group = builder.AddSubCommandGroup(AdminAppCommands.Search.Command, AdminAppCommands.Search.Description)
                                                  .AddNameLocalizations(this, AdminAppCommandKeys.Search.Command)
                                                  .AddDescriptionLocalizations(this, AdminAppCommandKeys.Search.CommandDescription);
            AddAdminSearchByPlayerCommand(group);
            AddAdminSearchByUserCommand(group);
        }

        public void AddAdminSearchByPlayerCommand(SubCommandGroupBuilder builder)
        {
            builder.AddSubCommand(AdminAppCommands.Search.SubCommand.Player.Command, AdminAppCommands.Search.SubCommand.Player.Description)
                   .AddNameLocalizations(this, AdminAppCommandKeys.Search.SubCommand.Player.Command)
                   .AddDescriptionLocalizations(this, AdminAppCommandKeys.Search.SubCommand.Player.Description)

                   .AddOption(CommandOptionType.String, AdminAppCommands.Search.SubCommand.Player.Args.Players.Name, AdminAppCommands.Search.SubCommand.Player.Args.Players.Description)
                   .AutoComplete()
                   .AddNameLocalizations(this, AdminAppCommandKeys.Search.SubCommand.Player.Args.Players.Name)
                   .AddDescriptionLocalizations(this, AdminAppCommandKeys.Search.SubCommand.Player.Args.Players.Description)
                   .Build();
        }
        
        public void AddAdminSearchByUserCommand(SubCommandGroupBuilder builder)
        {
            builder.AddSubCommand(AdminAppCommands.Search.SubCommand.User.Command, AdminAppCommands.Search.SubCommand.User.Description)
                   .AddNameLocalizations(this, AdminAppCommandKeys.Search.SubCommand.User.Command)
                   .AddDescriptionLocalizations(this, AdminAppCommandKeys.Search.SubCommand.User.Description)

                   .AddOption(CommandOptionType.User, AdminAppCommands.Search.SubCommand.User.Args.Users.Name, AdminAppCommands.Search.SubCommand.User.Args.Users.Description)
                   .AddNameLocalizations(this, AdminAppCommandKeys.Search.SubCommand.User.Args.Users.Name)
                   .AddDescriptionLocalizations(this, AdminAppCommandKeys.Search.SubCommand.User.Args.Users.Description)
                   .Build();
        }

        [DiscordApplicationCommand(AdminAppCommands.Command, AdminAppCommands.Link.Command)]
        private void DiscordAdminLinkCommand(DiscordInteraction interaction, InteractionDataParsed parsed)
        {
            string playerId = parsed.Args.GetString(AdminAppCommands.Link.Args.Player.Name);
            DiscordUser user = parsed.Args.GetUser(AdminAppCommands.Link.Args.User.Name);
            IPlayer player = players.FindPlayerById(playerId);
            if (player == null)
            {
                SendTemplateMessage(TemplateKeys.Commands.Admin.Link.Error.PlayerNotFound, interaction, GetDefault(ServerPlayerCache.GetPlayer(playerId), user));
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

        [DiscordApplicationCommand(AdminAppCommands.Command, AdminAppCommands.Unlink.Command)]
        private void DiscordAdminUnlinkCommand(DiscordInteraction interaction, InteractionDataParsed parsed)
        {
            string playerId = parsed.Args.GetString(AdminAppCommands.Unlink.Args.Player.Name);
            IPlayer player = players.FindPlayerById(playerId);
            DiscordUser user = parsed.Args.GetUser(AdminAppCommands.Unlink.Args.User.Name);

            if (player == null && user == null)
            {
                SendTemplateMessage(TemplateKeys.Commands.Admin.Unlink.Error.MustSpecifyOne, interaction, GetDefault(ServerPlayerCache.GetPlayer(playerId)));
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

        [DiscordApplicationCommand(AdminAppCommands.Command, AdminAppCommands.Search.SubCommand.Player.Command, AdminAppCommands.Search.Command)]
        private void DiscordAdminSearchByPlayer(DiscordInteraction interaction, InteractionDataParsed parsed)
        {
            string playerId = parsed.Args.GetString(AdminAppCommands.Search.SubCommand.Player.Args.Players.Name);
            IPlayer player = !string.IsNullOrEmpty(playerId) ? players.FindPlayerById(playerId) : null;
            if (player == null)
            {
                SendTemplateMessage(TemplateKeys.Commands.Admin.Search.Error.PlayerNotFound, interaction, GetDefault(ServerPlayerCache.GetPlayer(playerId)));
                return;
            }

            DiscordUser user = player.GetDiscordUser();
            SendTemplateMessage(TemplateKeys.Commands.Admin.Search.Success, interaction, GetDefault(player, user));
        }
        
        [DiscordApplicationCommand(AdminAppCommands.Command, AdminAppCommands.Search.SubCommand.User.Command, AdminAppCommands.Search.Command)]
        private void DiscordAdminSearchByUser(DiscordInteraction interaction, InteractionDataParsed parsed)
        {
            DiscordUser user = parsed.Args.GetUser(AdminAppCommands.Search.SubCommand.User.Args.Users.Name);
            IPlayer player = user.Player;
            SendTemplateMessage(TemplateKeys.Commands.Admin.Search.Success, interaction, GetDefault(player, user));
        }
    }
}