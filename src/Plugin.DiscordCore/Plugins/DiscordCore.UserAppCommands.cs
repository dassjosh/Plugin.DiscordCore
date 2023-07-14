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
using Oxide.Ext.Discord.Builders.Interactions.AutoComplete;
using Oxide.Ext.Discord.Entities.Interactions;
using Oxide.Ext.Discord.Entities.Interactions.ApplicationCommands;
using Oxide.Ext.Discord.Entities.Permissions;
using Oxide.Ext.Discord.Entities.Users;
using Oxide.Ext.Discord.Extensions;
using Oxide.Ext.Discord.Libraries.Placeholders;
using Oxide.Ext.Discord.Libraries.Templates;
using Oxide.Ext.Discord.Libraries.Templates.Commands;
using Oxide.Plugins.Placeholders;

namespace DiscordCorePlugin.Plugins
{
    //Define:FileOrder=35
    public partial class DiscordCore
    {
        public void RegisterUserApplicationCommands()
        {
            ApplicationCommandBuilder builder = new ApplicationCommandBuilder(UserAppCommands.Command, "Discord Core Commands", ApplicationCommandType.ChatInput)
                .AddDefaultPermissions(PermissionFlags.Administrator);

            AddUserCodeCommand(builder);
            AddUserUserCommand(builder);
            AddUserLeaveCommand(builder);
            AddUserLinkCommand(builder);

            CommandCreate build = builder.Build();
            DiscordCommandLocalization localization = builder.BuildCommandLocalization();
            
            _local.RegisterCommandLocalizationAsync(this, "User", localization, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0)).Then(_ =>
            {
                _local.ApplyCommandLocalizationsAsync(this, build, "User").Then(() =>
                {
                    Client.Bot.Application.CreateGlobalCommand(Client, build).Then(command =>
                    {
                        _appCommand = command;
                        command.GetPermissions(Client, Guild.Id).Then(CreateAllowedChannels);
                    });
                });
            });
        }

        public void AddUserCodeCommand(ApplicationCommandBuilder builder)
        {
            builder.AddSubCommand(UserAppCommands.CodeCommand, "Start the link between discord and the game server using a link code");
        }
        
        public void AddUserUserCommand(ApplicationCommandBuilder builder)
        {
            builder.AddSubCommand(UserAppCommands.UserCommand, "Start the link between discord and the game server by game server player name", sub =>
            {
                sub.AddOption(CommandOptionType.String, PlayerArg, "Player name on the game server",
                    options => options.AutoComplete().Required());
            });
        }
        
        public void AddUserLeaveCommand(ApplicationCommandBuilder builder)
        {
            builder.AddSubCommand(UserAppCommands.LeaveCommand, "Unlink your discord and game server accounts");
        }

        public void AddUserLinkCommand(ApplicationCommandBuilder builder)
        {
            builder.AddSubCommand(UserAppCommands.LinkCommand, "Complete the link using the given link code", sub =>
            {
                sub.AddOption(CommandOptionType.String, CodeArg, "Code to complete the link",
                    options => options.Required()
                                      .MinLength(_pluginConfig.LinkSettings.LinkCodeLength)
                                      .MaxLength(_pluginConfig.LinkSettings.LinkCodeLength));
            });
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

        [DiscordApplicationCommand(UserAppCommands.Command, UserAppCommands.CodeCommand)]
        private void DiscordCodeCommand(DiscordInteraction interaction, InteractionDataParsed parsed)
        {
            DiscordUser user = interaction.User;
            if (user.IsLinked())
            {
                SendTemplateMessage(TemplateKeys.Errors.UserAlreadyLinked, interaction, GetDefault(user.Player, user));
                return;
            }

            JoinData join = _joinHandler.CreateActivation(user);
            SendTemplateMessage(TemplateKeys.Commands.Code.Success, interaction, GetDefault(user).Add(PlaceholderKeys.Data.CodeKey, join.Code));
        }

        [DiscordApplicationCommand(UserAppCommands.Command, UserAppCommands.UserCommand)]
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
                SendTemplateMessage(TemplateKeys.Banned.PlayerBanned, interaction,GetDefault(user).AddTimestamp(_banHandler.GetRemainingBan(user)));
                return;
            }

            string playerId = parsed.Args.GetString(PlayerArg);
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

            _joinHandler.CreateActivation(player, user, JoinSource.Discord);
            
            using (PlaceholderData data = GetDefault(player, user))
            {
                data.ManualPool();
                Chat(player, ServerLang.Join.ByPlayer, GetDefault(player, user), data);
                SendTemplateMessage(TemplateKeys.Commands.User.Success, interaction, data);
            }
        }

        [DiscordAutoCompleteCommand(UserAppCommands.Command, PlayerArg, UserAppCommands.UserCommand)]
        private void HandleNameAutoComplete(DiscordInteraction interaction, InteractionDataOption focused)
        {
            string search = focused.GetString();
            InteractionAutoCompleteBuilder response = interaction.GetAutoCompleteBuilder();
            response.AddAllOnlineFirstPlayers(search, PlayerNameFormatter.ClanName);
            interaction.CreateResponse(Client, response);
        }

        [DiscordApplicationCommand(UserAppCommands.Command, UserAppCommands.LeaveCommand)]
        private void DiscordLeaveCommand(DiscordInteraction interaction, InteractionDataParsed parsed)
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

        [DiscordApplicationCommand(UserAppCommands.Command, UserAppCommands.LinkCommand)]
        private void DiscordLinkCommand(DiscordInteraction interaction, InteractionDataParsed parsed)
        {
            DiscordUser user = interaction.User;
            if (user.IsLinked())
            {
                SendTemplateMessage(TemplateKeys.Errors.UserAlreadyLinked, interaction, GetDefault(user.Player, user));
                return;
            }
            
            string code = parsed.Args.GetString(CodeArg);
            JoinData join = _joinHandler.FindByCode(code);
            if (join == null)
            {
                SendTemplateMessage(TemplateKeys.Errors.CodActivationNotFound, interaction, GetDefault(user).Add(PlaceholderKeys.Data.CodeKey, code));
                return;
            }

            join.Discord = user;
            
            _joinHandler.CompleteLink(join, interaction);
        }
    }
}