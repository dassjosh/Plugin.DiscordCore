using System;
using System.Collections.Generic;
using DiscordCorePlugin.AppCommands;
using DiscordCorePlugin.Enums;
using DiscordCorePlugin.Link;
using DiscordCorePlugin.Localization;
using DiscordCorePlugin.Templates;
using Oxide.Core.Libraries.Covalence;
using Oxide.Ext.Discord.Attributes.ApplicationCommands;
using Oxide.Ext.Discord.Builders.ApplicationCommands;
using Oxide.Ext.Discord.Builders.Interactions;
using Oxide.Ext.Discord.Entities.Interactions;
using Oxide.Ext.Discord.Entities.Interactions.ApplicationCommands;
using Oxide.Ext.Discord.Entities.Permissions;
using Oxide.Ext.Discord.Entities.Users;
using Oxide.Ext.Discord.Extensions;
using Oxide.Ext.Discord.Libraries.Placeholders;

namespace DiscordCorePlugin.Plugins
{
    //Define:FileOrder=35
    public partial class DiscordCore
    {
        public void RegisterUserApplicationCommands()
        {
            ApplicationCommandBuilder builder = new ApplicationCommandBuilder(UserAppCommands.Command, "Discord Core Commands", ApplicationCommandType.ChatInput)
                                                .AddDefaultPermissions(PermissionFlags.None)
                                                .AddNameLocalizations(this, UserAppCommandKeys.Command)
                                                .AddDescriptionLocalizations(this, UserAppCommandKeys.Description);

            AddUserCodeCommand(builder);
            AddUserUserCommand(builder);
            AddUserLeaveCommand(builder);
            AddUserLinkCommand(builder);

            Client.Bot.Application.CreateGlobalCommand(Client, builder.Build(), command =>
            {
                _appCommand = command;
                command.GetPermissions(Client, Guild.Id, CreateAllowedChannels);
            });
        }

        public void AddUserCodeCommand(ApplicationCommandBuilder builder)
        {
            builder.AddSubCommand(UserAppCommands.Code.Command, UserAppCommands.Code.Description)
                   .AddNameLocalizations(this, UserAppCommandKeys.Code.Command)
                   .AddDescriptionLocalizations(this, UserAppCommandKeys.Code.Description);
        } 
        
        public void AddUserUserCommand(ApplicationCommandBuilder builder)
        {
            builder.AddSubCommand(UserAppCommands.User.Command, UserAppCommands.User.Description)
                   .AddNameLocalizations(this, UserAppCommandKeys.User.Command)
                   .AddDescriptionLocalizations(this, UserAppCommandKeys.User.Description)
                   .AddOption(CommandOptionType.String, UserAppCommands.User.Args.Player.Name, UserAppCommands.User.Args.Player.Description)
                   .Required()
                   .AutoComplete()
                   .AddNameLocalizations(this, UserAppCommandKeys.User.Args.Player.Name)
                   .AddDescriptionLocalizations(this, UserAppCommandKeys.User.Args.Player.Description);
        }
        
        public void AddUserLeaveCommand(ApplicationCommandBuilder builder)
        {
            builder.AddSubCommand(UserAppCommands.Leave.Command, UserAppCommands.Leave.Description)
                   .AddNameLocalizations(this, UserAppCommandKeys.Leave.Command)
                   .AddDescriptionLocalizations(this, UserAppCommandKeys.Leave.Description);
        }

        public void AddUserLinkCommand(ApplicationCommandBuilder builder)
        {
            builder.AddSubCommand(UserAppCommands.Link.Command, UserAppCommands.Link.Description)
                   .AddNameLocalizations(this, UserAppCommandKeys.Link.Command)
                   .AddDescriptionLocalizations(this, UserAppCommandKeys.Link.Description)
                   .AddOption(CommandOptionType.String, UserAppCommands.Link.Args.Code.Name, UserAppCommands.Link.Args.Code.Description)
                   .Required()
                   .SetMinLength(_pluginConfig.LinkSettings.LinkCodeLength)
                   .SetMaxLength(_pluginConfig.LinkSettings.LinkCodeLength)
                   .AddNameLocalizations(this, UserAppCommandKeys.Link.Args.Code.Name)
                   .AddDescriptionLocalizations(this, UserAppCommandKeys.Link.Args.Code.Description);
        }

        public void CreateAllowedChannels(GuildCommandPermissions permissions)
        {
            List<string> channels = new List<string>();
            for (int index = 0; index < permissions.Permissions.Count; index++)
            {
                CommandPermissions perm = permissions.Permissions[index];
                if (perm.Type == CommandPermissionType.Channel)
                {
                    string name = Guild.Channels[perm.Id]?.Name;
                    if (!string.IsNullOrEmpty(name))
                    {
                        channels.Add(name);
                    }
                }
            }

            _allowedChannels = string.Join(", ", channels);
            _placeholders.RegisterPlaceholder(this, "dc.command.channels", _allowedChannels);
        }

        [DiscordApplicationCommand(UserAppCommands.Command, UserAppCommands.Code.Command)]
        private void DiscordCodeCommand(DiscordInteraction interaction)
        {
            DiscordUser user = interaction.User;
            if (user.IsLinked())
            {
                SendTemplateMessage(TemplateKeys.Errors.UserAlreadyLinked, interaction, GetDefault(user.Player, user));
                return;
            }

            JoinData join = _joinHandler.CreateActivation(user);
            SendTemplateMessage(TemplateKeys.Commands.Code.Success, interaction, GetDefault(user).Add(CodeKey, join.Code));
        }

        [DiscordApplicationCommand(UserAppCommands.Command, UserAppCommands.User.Command)]
        private void DiscordUserCommand(DiscordInteraction interaction, InteractionDataParsed parsed)
        {
            DiscordUser user = interaction.User;
            if (user.IsLinked())
            {
                SendTemplateMessage(TemplateKeys.Errors.UserAlreadyLinked, interaction, GetDefault(user));
                return;
            }

            if (_banHandler.IsBanned(user))
            {
                SendTemplateMessage(TemplateKeys.Banned.PlayerBanned, interaction,GetDefault(user).Add(BanDurationKey, _banHandler.GetRemainingBan(user)));
                return;
            }

            string playerId = parsed.Args.GetString(UserAppCommands.User.Args.Player.Name);
            IPlayer player = covalence.Players.FindPlayerById(playerId);
            if (player == null)
            {
                SendTemplateMessage(TemplateKeys.Commands.User.Error.PlayerIsInvalid, interaction, GetDefault(user));
                return;
            }

            if (player.IsLinked())
            {
                SendTemplateMessage(TemplateKeys.Errors.PlayerAlreadyLinked, interaction, GetDefault(player, user));
                return;
            }

            if (!player.IsConnected)
            {
                SendTemplateMessage(TemplateKeys.Commands.User.Error.PlayerNotConnected, interaction, GetDefault(player, user));
                return;
            }

            _joinHandler.CreateActivation(player, user, JoinedFrom.Discord);
            
            using (PlaceholderData data = GetDefault(player, user))
            {
                data.ManualPool();
                Chat(player, ServerLang.Join.ByPlayer, GetDefault(player, user), data);
                SendTemplateMessage(TemplateKeys.Commands.User.Success, interaction, data);
            }
        }

        [DiscordAutoCompleteCommand(UserAppCommands.Command, UserAppCommands.User.Args.Player.Name, UserAppCommands.User.Command)]
        private void HandleNameAutoComplete(DiscordInteraction interaction, InteractionDataOption focused)
        {
            string search = (string)focused.Value;
            InteractionAutoCompleteBuilder response = interaction.GetAutoCompleteBuilder();
            response.AddAllOnlineFirstPlayers(search, StringComparison.OrdinalIgnoreCase, AutoCompleteSearchMode.Contains, AutoCompletePlayerSearchOptions.IncludeClanName);
            interaction.CreateInteractionResponse(Client, response);
        }

        [DiscordApplicationCommand(UserAppCommands.Command, UserAppCommands.Leave.Command)]
        private void DiscordLeaveCommand(DiscordInteraction interaction)
        {
            DiscordUser user = interaction.User;
            if (!user.IsLinked())
            {
                SendTemplateMessage(TemplateKeys.Commands.Leave.Error.UserNotLinked, interaction, GetDefault(user));
                return;
            }

            IPlayer player = user.Player;
            _linkHandler.HandleUnlink(player, user, UnlinkedReason.Command, interaction);
        }

        [DiscordApplicationCommand(UserAppCommands.Command, UserAppCommands.Link.Command)]
        private void DiscordLinkCommand(DiscordInteraction interaction, InteractionDataParsed parsed)
        {
            DiscordUser user = interaction.User;
            if (user.IsLinked())
            {
                SendTemplateMessage(TemplateKeys.Errors.UserAlreadyLinked, interaction, GetDefault(user.Player, user));
                return;
            }
            
            string code = parsed.Args.GetString(UserAppCommands.Link.Args.Code.Name);
            JoinData join = _joinHandler.FindByCode(code);
            if (join == null)
            {
                SendTemplateMessage(TemplateKeys.Errors.CodActivationNotFound, interaction, GetDefault(user).Add(CodeKey, code));
                return;
            }

            join.Discord = user;
            
            _joinHandler.CompleteLink(join, interaction);
        }
    }
}