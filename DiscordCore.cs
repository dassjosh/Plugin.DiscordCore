using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Ext.Discord;
using Oxide.Ext.Discord.Attributes;
using Oxide.Ext.Discord.Attributes.ApplicationCommands;
using Oxide.Ext.Discord.Builders.ApplicationCommands;
using Oxide.Ext.Discord.Builders.Interactions;
using Oxide.Ext.Discord.Cache;
using Oxide.Ext.Discord.Constants;
using Oxide.Ext.Discord.Entities;
using Oxide.Ext.Discord.Entities.Applications;
using Oxide.Ext.Discord.Entities.Channels;
using Oxide.Ext.Discord.Entities.Gatway;
using Oxide.Ext.Discord.Entities.Gatway.Events;
using Oxide.Ext.Discord.Entities.Guilds;
using Oxide.Ext.Discord.Entities.Interactions;
using Oxide.Ext.Discord.Entities.Interactions.ApplicationCommands;
using Oxide.Ext.Discord.Entities.Interactions.MessageComponents;
using Oxide.Ext.Discord.Entities.Interactions.Response;
using Oxide.Ext.Discord.Entities.Messages;
using Oxide.Ext.Discord.Entities.Permissions;
using Oxide.Ext.Discord.Entities.Users;
using Oxide.Ext.Discord.Extensions;
using Oxide.Ext.Discord.Helpers;
using Oxide.Ext.Discord.Libraries.Langs;
using Oxide.Ext.Discord.Libraries.Linking;
using Oxide.Ext.Discord.Libraries.Placeholders;
using Oxide.Ext.Discord.Libraries.Placeholders.Default;
using Oxide.Ext.Discord.Libraries.Templates;
using Oxide.Ext.Discord.Libraries.Templates.Components;
using Oxide.Ext.Discord.Libraries.Templates.Messages;
using Oxide.Ext.Discord.Libraries.Templates.Messages.Embeds;
using Oxide.Ext.Discord.Logging;
using Oxide.Ext.Discord.Pooling;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

//DiscordCore created with PluginMerge v(1.0.5.0) by MJSU @ https://github.com/dassjosh/Plugin.Merge
namespace Oxide.Plugins
{
    [Info("Discord Core", "MJSU", "2.5.0")]
    [Description("Creates a link between a player and discord")]
    public partial class DiscordCore : CovalencePlugin, IDiscordLinkPlugin
    {
        #region Plugins\DiscordCore.Fields.cs
        [DiscordClient]
        public DiscordClient Client;
        
        private PluginData _pluginData;
        private PluginConfig _pluginConfig;
        
        private DiscordUser _bot;
        
        public DiscordGuild Guild;
        
        private readonly DiscordSettings _discordSettings = new DiscordSettings
        {
            Intents = GatewayIntents.Guilds | GatewayIntents.GuildMembers
        };
        
        private readonly DiscordLink _link = Interface.Oxide.GetLibrary<DiscordLink>();
        private readonly DiscordMessageTemplates _templates = Interface.Oxide.GetLibrary<DiscordMessageTemplates>();
        private readonly DiscordPlaceholders _placeholders = Interface.Oxide.GetLibrary<DiscordPlaceholders>();
        private readonly DiscordLang _lang = Interface.Oxide.GetLibrary<DiscordLang>();
        private readonly StringBuilder _sb = new StringBuilder();
        
        private JoinHandler _joinHandler;
        private JoinBanHandler _banHandler;
        private LinkHandler _linkHandler;
        
        private const string UsePermission = "discordcore.use";
        private const string AccentColor = "de8732";
        private const string DiscordSuccess = "43b581";
        private const string DiscordDanger = "f04747";
        
        private DiscordApplicationCommand _appCommand;
        private string _allowedChannels;
        
        public static DiscordCore Instance;
        #endregion

        #region Plugins\DiscordCore.Setup.cs
        private void Init()
        {
            Instance = this;
            _pluginData = Interface.Oxide.DataFileSystem.ReadObject<PluginData>(Name);
            
            permission.RegisterPermission(UsePermission, this);
            
            _banHandler = new JoinBanHandler(_pluginConfig.LinkBanSettings);
            _linkHandler = new LinkHandler(_pluginData, _pluginConfig);
            _joinHandler = new JoinHandler(_pluginConfig.LinkSettings, _linkHandler, _banHandler);
            
            _discordSettings.ApiToken = _pluginConfig.ApiKey;
            _discordSettings.LogLevel = _pluginConfig.ExtensionDebugging;
        }
        
        protected override void LoadDefaultConfig()
        {
            PrintWarning("Loading Default Config");
        }
        
        protected override void LoadConfig()
        {
            base.LoadConfig();
            Config.Settings.DefaultValueHandling = DefaultValueHandling.Populate;
            _pluginConfig = AdditionalConfig(Config.ReadObject<PluginConfig>());
            Config.WriteObject(_pluginConfig);
        }
        
        public PluginConfig AdditionalConfig(PluginConfig config)
        {
            config.LinkSettings = new LinkSettings(config.LinkSettings);
            config.WelcomeMessageSettings = new WelcomeMessageSettings(config.WelcomeMessageSettings);
            config.LinkMessageSettings = new GuildLinkMessageSettings(config.LinkMessageSettings);
            config.PermissionSettings = new LinkPermissionSettings(config.PermissionSettings);
            config.LinkBanSettings = new LinkBanSettings(config.LinkBanSettings);
            return config;
        }
        
        private void OnServerInitialized()
        {
            RegisterChatLangCommand(nameof(DiscordCoreChatCommand), ServerLang.Commands.DcCommand);
            
            if (string.IsNullOrEmpty(_pluginConfig.ApiKey))
            {
                PrintWarning("Please set the Discord Bot Token in the config and reload the plugin");
                return;
            }
            
            foreach (DiscordInfo info in _pluginData.PlayerDiscordInfo.Values)
            {
                if (info.LastOnline == DateTime.MinValue)
                {
                    info.LastOnline = DateTime.UtcNow;
                }
            }
            
            _link.AddLinkPlugin(this);
            RegisterPlaceholders();
            
            Client.Connect(_discordSettings);
        }
        
        private void Unload()
        {
            SaveData();
            Instance = null;
        }
        #endregion

        #region Plugins\DiscordCore.Lang.cs
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
                
                [ServerLang.Commands.Code.LinkInfo] = $"To complete your activation please open Discord use the following command: <color=#{AccentColor}>/{{plugin.lang:{UserAppCommandKeys.Command}}} {{plugin.lang:{UserAppCommandKeys.Link.Command}}} {{discordcore.link.code}}</color>.\n",
                [ServerLang.Commands.Code.LinkServer] = $"In order to use this command you must be in the <color=#{AccentColor}>{{guild.name}}</color> discord server. " +
                $"You can join @ <color=#{DiscordSuccess}>discord.gg/{{discordcore.invite.code}}</color>.\n",
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
                $"If you wish to [#{DiscordSuccess}]accept[/#] this link please type [#{DiscordSuccess}]/{{plugin.lang:{ServerLang.Commands.DcCommand}}} {{plugin.lang:{ServerLang.Commands.AcceptCommand}}}[/#]. " +
                $"If you wish to [#{DiscordDanger}]decline[/#] this link please type [#{DiscordDanger}]/{{plugin.lang:{ServerLang.Commands.DcCommand}}} {{plugin.lang:{ServerLang.Commands.DeclineCommand}}}[/#]",
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
                
                [UserAppCommandKeys.Command] = UserAppCommands.Command,
                [UserAppCommandKeys.Description] = UserAppCommands.Description,
                [UserAppCommandKeys.Code.Command] = UserAppCommands.Code.Command,
                [UserAppCommandKeys.Code.Description] = UserAppCommands.Code.Description,
                [UserAppCommandKeys.User.Command] = UserAppCommands.User.Command,
                [UserAppCommandKeys.User.Description] = UserAppCommands.User.Description,
                [UserAppCommandKeys.User.Args.Player.Name] = UserAppCommands.User.Args.Player.Name,
                [UserAppCommandKeys.User.Args.Player.Description] = UserAppCommands.User.Args.Player.Description,
                [UserAppCommandKeys.Leave.Command] = UserAppCommands.Leave.Command,
                [UserAppCommandKeys.Leave.Description] = UserAppCommands.Leave.Description,
                [UserAppCommandKeys.Link.Command] = UserAppCommands.Link.Command,
                [UserAppCommandKeys.Link.Description] = UserAppCommands.Link.Description,
                [UserAppCommandKeys.Link.Args.Code.Name] = UserAppCommands.Link.Args.Code.Name,
                [UserAppCommandKeys.Link.Args.Code.Description] = UserAppCommands.Link.Args.Code.Description,
                
                [AdminAppCommandKeys.Command] = AdminAppCommands.Command,
                [AdminAppCommandKeys.Description] = AdminAppCommands.Description,
                [AdminAppCommandKeys.Link.Command] = AdminAppCommands.Link.Command,
                [AdminAppCommandKeys.Link.Description] = AdminAppCommands.Link.Description,
                [AdminAppCommandKeys.Link.Args.Player.Name] = AdminAppCommands.Link.Args.Player.Name,
                [AdminAppCommandKeys.Link.Args.Player.Description] = AdminAppCommands.Link.Args.Player.Description,
                [AdminAppCommandKeys.Link.Args.User.Name] = AdminAppCommands.Link.Args.User.Name,
                [AdminAppCommandKeys.Link.Args.User.Description] = AdminAppCommands.Link.Args.User.Description,
                [AdminAppCommandKeys.Unlink.Command] = AdminAppCommands.Unlink.Command,
                [AdminAppCommandKeys.Unlink.Description] = AdminAppCommands.Unlink.Description,
                [AdminAppCommandKeys.Unlink.Args.Player.Name] = AdminAppCommands.Unlink.Args.Player.Name,
                [AdminAppCommandKeys.Unlink.Args.Player.Description] = AdminAppCommands.Unlink.Args.Player.Description,
                [AdminAppCommandKeys.Unlink.Args.User.Name] = AdminAppCommands.Unlink.Args.User.Name,
                [AdminAppCommandKeys.Unlink.Args.User.Description] = AdminAppCommands.Unlink.Args.User.Description,
                [AdminAppCommandKeys.Search.Command] = AdminAppCommands.Search.Command,
                [AdminAppCommandKeys.Search.CommandDescription] = AdminAppCommands.Search.Description,
                [AdminAppCommandKeys.Search.SubCommand.Player.Command] = AdminAppCommands.Search.SubCommand.Player.Command,
                [AdminAppCommandKeys.Search.SubCommand.Player.Description] = AdminAppCommands.Search.SubCommand.Player.Description,
                [AdminAppCommandKeys.Search.SubCommand.Player.Args.Players.Name] = AdminAppCommands.Search.SubCommand.Player.Args.Players.Name,
                [AdminAppCommandKeys.Search.SubCommand.Player.Args.Players.Description] = AdminAppCommands.Search.SubCommand.Player.Args.Players.Description,
                [AdminAppCommandKeys.Search.SubCommand.User.Command]= AdminAppCommands.Search.SubCommand.User.Command,
                [AdminAppCommandKeys.Search.SubCommand.User.Description] = AdminAppCommands.Search.SubCommand.User.Description,
                [AdminAppCommandKeys.Search.SubCommand.User.Args.Users.Name] = AdminAppCommands.Search.SubCommand.User.Args.Users.Name,
                [AdminAppCommandKeys.Search.SubCommand.User.Args.Users.Description] = AdminAppCommands.Search.SubCommand.User.Args.Users.Description,
            }, this);
        }
        #endregion

        #region Plugins\DiscordCore.ChatCommands.cs
        private const string ServerLinkArgument = "link";
        
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
            
            if (subCommand.Equals(ServerLinkArgument, StringComparison.OrdinalIgnoreCase))
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
            
            JoinData join = _joinHandler.CreateActivation(player);
            using (PlaceholderData data = GetDefault(player).AddUser(_bot).Add(CodeKey, join.Code))
            {
                data.ManualPool();
                _sb.Clear();
                _sb.Append(LangPlaceholder(ServerLang.Commands.Code.LinkInfo, data));
                _sb.Append(LangPlaceholder(ServerLang.Commands.Code.LinkServer, data));
                if (!string.IsNullOrEmpty(_allowedChannels))
                {
                    _sb.Append(LangPlaceholder(ServerLang.Commands.Code.LinkInGuild, data));
                }
                
                if (_appCommand.DmPermission.HasValue && _appCommand.DmPermission.Value)
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
                Chat(player, ServerLang.Banned.IsUserBanned, GetDefault(player));
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
                
                SendTemplateMessage(TemplateKeys.Join.ByUsername, member.User, player);
                return;
            }
            
            int discriminatorIndex = search.IndexOf('#');
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
            
            GuildSearchMembers guildSearch = new GuildSearchMembers
            {
                Query = userName,
                Limit = 1000
            };
            
            Guild.SearchGuildMembers(Client, guildSearch, members =>
            {
                HandleChatJoinUserResults(player, members, userName, discriminator);
            }, error =>
            {
                Chat(player, ServerLang.Commands.User.Errors.SearchError, GetDefault(player));
            });
        }
        
        public void HandleChatJoinUserResults(IPlayer player, List<GuildMember> members, string userName, string discriminator)
        {
            if (members.Count == 0)
            {
                string name = !string.IsNullOrEmpty(discriminator) ? $"{userName}#{discriminator}" : userName;
                Chat(player, ServerLang.Commands.User.Errors.UserNotFound, GetDefault(player), name);
                return;
            }
            
            if (members.Count == 1)
            {
                SendTemplateMessage(TemplateKeys.Join.ByUsername, members[0].User, player);
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
                else if (searchUser.Username.Equals(userName, StringComparison.OrdinalIgnoreCase) && searchUser.Discriminator.Equals(discriminator))
                {
                    user = searchUser;
                    break;
                }
            }
            
            if (user == null || count > 1)
            {
                string name = !string.IsNullOrEmpty(discriminator) ? $"{userName}#{discriminator}" : userName;
                Chat(player, ServerLang.Commands.User.Errors.MultipleUsersFound, GetDefault(player), name);
                return;
            }
            
            SendTemplateMessage(TemplateKeys.Join.ByUsername, user, player);
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
            
            if (join.From == JoinedFrom.Server)
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
            
            if (join.From == JoinedFrom.Server)
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
        #endregion

        #region Plugins\DiscordCore.Hooks.cs
        private void OnUserConnected(IPlayer player)
        {
            _linkHandler.OnUserConnected(player);
        }
        #endregion

        #region Plugins\DiscordCore.DiscordHooks.cs
        [HookMethod(DiscordExtHooks.OnDiscordGatewayReady)]
        private void OnDiscordGatewayReady(GatewayReadyEvent ready)
        {
            _bot = ready.User;
            
            DiscordGuild guild = null;
            if (ready.Guilds.Count == 1 && !_pluginConfig.GuildId.IsValid())
            {
                guild = ready.Guilds.Values.FirstOrDefault();
            }
            
            if (guild == null)
            {
                guild = ready.Guilds[_pluginConfig.GuildId];
                if (guild == null)
                {
                    PrintError("Failed to find a matching guild for the Discord Server Id. " +
                    "Please make sure your guild Id is correct and the bot is in the discord server.");
                    return;
                }
            }
            
            Guild = guild;
            
            DiscordApplication app = Client.Bot.Application;
            if (!app.HasApplicationFlag(ApplicationFlags.GatewayGuildMembersLimited) && !app.HasApplicationFlag(ApplicationFlags.GatewayGuildMembers))
            {
                PrintError($"You need to enable \"Server Members Intent\" for {Client.Bot.BotUser.Username} @ https://discord.com/developers/applications\n" +
                $"{Name} will not function correctly until that is fixed. Once updated please reload {Name}.");
                return;
            }
            
            if (!app.HasApplicationFlag(ApplicationFlags.GatewayMessageContentLimited) && !app.HasApplicationFlag(ApplicationFlags.GatewayMessageContent))
            {
                PrintWarning($"You will need to enable \"Message Content Intent\" for {_bot.GetFullUserName} @ https://discord.com/developers/applications\n by April 2022" +
                $"{Name} will stop function correctly after that date until that is fixed.");
            }
            
            Puts($"Connected to bot: {_bot.Username}");
        }
        
        [HookMethod(DiscordExtHooks.OnDiscordBotFullyLoaded)]
        private void OnDiscordBotFullyLoaded()
        {
            RegisterTemplates();
            RegisterUserApplicationCommands();
            RegisterAdminApplicationCommands();
            _linkHandler.ProcessLeaveAndRejoin();
            SetupGuildWelcomeMessage();
        }
        
        [HookMethod(DiscordExtHooks.OnDiscordGuildMemberAdded)]
        private void OnDiscordGuildMemberAdded(GuildMemberAddedEvent member, DiscordGuild guild)
        {
            if (Guild?.Id != guild.Id)
            {
                return;
            }
            
            _linkHandler.OnGuildMemberJoin(member.User);
            if (!_pluginConfig.WelcomeMessageSettings.EnableWelcomeMessage || !_pluginConfig.WelcomeMessageSettings.SendOnGuildJoin)
            {
                return;
            }
            
            if (member.User.IsLinked())
            {
                return;
            }
            
            SendTemplateMessage(TemplateKeys.WelcomeMessage.PmWelcomeMessage, member.User);
        }
        
        [HookMethod(DiscordExtHooks.OnDiscordGuildMemberRemoved)]
        private void OnDiscordGuildMemberRemoved(GuildMemberRemovedEvent member, DiscordGuild guild)
        {
            if (Guild?.Id != guild.Id)
            {
                return;
            }
            
            _linkHandler.OnGuildMemberLeft(member.User);
        }
        
        [HookMethod(DiscordExtHooks.OnDiscordGuildMemberRoleAdded)]
        private void OnDiscordGuildMemberRoleAdded(GuildMember member, Snowflake roleId, DiscordGuild guild)
        {
            if (Guild?.Id != guild.Id)
            {
                return;
            }
            
            if (!_pluginConfig.WelcomeMessageSettings.EnableWelcomeMessage || !_pluginConfig.WelcomeMessageSettings.SendOnRoleAdded.Contains(roleId))
            {
                return;
            }
            
            if (member.User.IsLinked())
            {
                return;
            }
            
            SendGlobalTemplateMessage(TemplateKeys.WelcomeMessage.PmWelcomeMessage, member.User);
        }
        #endregion

        #region Plugins\DiscordCore.UserAppCommands.cs
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
        #endregion

        #region Plugins\DiscordCore.MessageComponentCommands.cs
        private const string WelcomeMessageLinkAccountsButtonId = nameof(DiscordCore) + "_PmLinkAccounts";
        private const string GuildWelcomeMessageLinkAccountsButtonId = nameof(DiscordCore) + "_GuildLinkAccounts";
        private const string AcceptLinkButtonId = nameof(DiscordCore) + "_AcceptLink";
        private const string DeclineLinkButtonId = nameof(DiscordCore) + "_DeclineLink";
        
        [DiscordMessageComponentCommand(WelcomeMessageLinkAccountsButtonId)]
        private void HandleWelcomeMessageLinkAccounts(DiscordInteraction interaction)
        {
            DiscordUser user = interaction.User;
            if (user.IsLinked())
            {
                SendTemplateMessage(TemplateKeys.Errors.UserAlreadyLinked,interaction, GetDefault(user.Player, user));
                return;
            }
            
            JoinData join = _joinHandler.CreateActivation(user);
            SendTemplateMessage(TemplateKeys.Link.WelcomeMessage.DmLinkAccounts, interaction, GetDefault(user).Add(CodeKey, join.Code));
        }
        
        [DiscordMessageComponentCommand(GuildWelcomeMessageLinkAccountsButtonId)]
        private void HandleGuildWelcomeMessageLinkAccounts(DiscordInteraction interaction)
        {
            DiscordUser user = interaction.User;
            if (user.IsLinked())
            {
                SendTemplateMessage(TemplateKeys.Errors.UserAlreadyLinked, interaction, GetDefault(user.Player, user));
                return;
            }
            
            JoinData join = _joinHandler.CreateActivation(user);
            SendTemplateMessage(TemplateKeys.Link.WelcomeMessage.GuildLinkAccounts, interaction, GetDefault(user).Add(CodeKey, join.Code));
        }
        
        [DiscordMessageComponentCommand(AcceptLinkButtonId)]
        private void HandleAcceptLinkButton(DiscordInteraction interaction)
        {
            DiscordUser user = interaction.User;
            if (user.IsLinked())
            {
                SendTemplateMessage(TemplateKeys.Errors.UserAlreadyLinked, interaction, GetDefault(user.Player, user));
                return;
            }
            
            JoinData join = _joinHandler.FindCompletedByUser(user);
            if (join == null)
            {
                SendTemplateMessage(TemplateKeys.Errors.LookupActivationNotFound, interaction, GetDefault(user));
                return;
            }
            
            _joinHandler.CompleteLink(join, interaction);
        }
        
        [DiscordMessageComponentCommand(DeclineLinkButtonId)]
        private void HandleDeclineLinkButton(DiscordInteraction interaction)
        {
            DiscordUser user = interaction.User;
            if (user.IsLinked())
            {
                SendTemplateMessage(TemplateKeys.Errors.UserAlreadyLinked, interaction, GetDefault(user.Player, user));
                return;
            }
            
            JoinData join = _joinHandler.FindCompletedByUser(user);
            if (join == null)
            {
                SendTemplateMessage(TemplateKeys.Errors.LookupActivationNotFound, interaction, GetDefault(user));
                return;
            }
            
            _joinHandler.DeclineLink(join, interaction);
        }
        #endregion

        #region Plugins\DiscordCore.AdminAppCommands.cs
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
        #endregion

        #region Plugins\DiscordCore.Templates.cs
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
            DiscordMessageTemplate linkCommand = CreateTemplateEmbed($"Player {{player.name}}({{player.id}}) has {DiscordFormatting.Bold("linked")} with discord {{user.mention}}", DiscordSuccess, new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalMessageTemplate(this, TemplateKeys.Announcements.Link.Command, linkCommand, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate linkAdmin = CreateTemplateEmbed($"Player {{player.name}}({{player.id}}) was {DiscordFormatting.Bold("linked")} with discord {{user.mention}} by an admin", DiscordSuccess, new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalMessageTemplate(this, TemplateKeys.Announcements.Link.Admin, linkAdmin, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate linkApi = CreateTemplateEmbed($"Player {{player.name}}({{player.id}}) has {DiscordFormatting.Bold("linked")} with discord {{user.mention}}", DiscordSuccess, new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalMessageTemplate(this, TemplateKeys.Announcements.Link.Api, linkApi, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate linkGuildRejoin = CreateTemplateEmbed($"Player {{player.name}}({{player.id}}) has {DiscordFormatting.Bold("linked")} with discord {{user.mention}} because they rejoined the {DiscordFormatting.Bold("{guild.name}")} Discord server", DiscordSuccess, new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalMessageTemplate(this, TemplateKeys.Announcements.Link.GuildRejoin, linkGuildRejoin, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate linkInactiveRejoin = CreateTemplateEmbed($"Player {{player.name}}({{player.id}}) has {DiscordFormatting.Bold("linked")} with discord {{user.mention}} because they rejoined the {DiscordFormatting.Bold("{server.name}")} game server", DiscordSuccess, new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalMessageTemplate(this, TemplateKeys.Announcements.Link.InactiveRejoin, linkInactiveRejoin, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate unlinkCommand = CreateTemplateEmbed($"Player {{player.name}}({{player.id}}) has {DiscordFormatting.Bold("unlinked")} from discord {{user.mention}}", DiscordDanger, new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalMessageTemplate(this, TemplateKeys.Announcements.Unlink.Command, unlinkCommand, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate unlinkAdmin = CreateTemplateEmbed($"Player {{player.name}}({{player.id}}) was {DiscordFormatting.Bold("unlinked")} from discord {{user.mention}} by an admin", DiscordDanger, new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalMessageTemplate(this, TemplateKeys.Announcements.Unlink.Admin, unlinkAdmin, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate unlinkApi = CreateTemplateEmbed($"Player {{player.name}}({{player.id}}) has {DiscordFormatting.Bold("unlinked")} from discord {{user.mention}}", DiscordDanger, new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalMessageTemplate(this, TemplateKeys.Announcements.Unlink.Api, unlinkApi, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate unlinkLeftGuild = CreateTemplateEmbed($"Player {{player.name}}({{player.id}}) has {DiscordFormatting.Bold("unlinked")} from discord {{user.fullname}}({{user.id}}) because they left the {DiscordFormatting.Bold("{guild.name}")} Discord server", DiscordDanger, new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalMessageTemplate(this, TemplateKeys.Announcements.Unlink.LeftGuild, unlinkLeftGuild, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate unlinkInactive = CreateTemplateEmbed($"Player {{player.name}}({{player.id}}) has {DiscordFormatting.Bold("unlinked")} from discord {{user.fullname}}({{user.id}}) because they were inactive for {{discordcore.inactive.duration}} days", DiscordDanger, new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalMessageTemplate(this, TemplateKeys.Announcements.Unlink.Inactive, unlinkInactive, new TemplateVersion(1, 0, 0));
        }
        
        public void RegisterWelcomeMessages()
        {
            DiscordMessageTemplate pmWelcomeMessage = CreateTemplateEmbed($"Welcome to the {DiscordFormatting.Bold("{guild.name}")} Discord server. " +
            $"If you would link to link your player and Discord accounts please click on the {DiscordFormatting.Bold("Link Accounts")} button below to start the process." +
            $"{DiscordFormatting.Underline("\nNote: You must be in game to complete the link.")}", DiscordSuccess, new TemplateVersion(1, 0, 0));
            pmWelcomeMessage.Components = new List<BaseComponentTemplate>
            {
                new ButtonTemplate("Link Accounts", ButtonStyle.Success, WelcomeMessageLinkAccountsButtonId, AcceptEmoji)
            };
            _templates.RegisterGlobalMessageTemplate(this, TemplateKeys.WelcomeMessage.PmWelcomeMessage, pmWelcomeMessage, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate guildWelcomeMessage = CreateTemplateEmbed($"Welcome to the {DiscordFormatting.Bold("{guild.name}")} Discord server. " +
            "This server supports linking your Discord and in game accounts. " +
            $"If you would link to link your player and Discord accounts please click on the {DiscordFormatting.Bold("Link Accounts")} button below to start the process." +
            $"{DiscordFormatting.Underline("\nNote: You must be in game to complete the link.")}", DiscordSuccess, new TemplateVersion(1, 0, 0));
            guildWelcomeMessage.Components = new List<BaseComponentTemplate>
            {
                new ButtonTemplate("Link Accounts", ButtonStyle.Success, GuildWelcomeMessageLinkAccountsButtonId, AcceptEmoji)
            };
            _templates.RegisterGlobalMessageTemplate(this, TemplateKeys.WelcomeMessage.GuildWelcomeMessage, guildWelcomeMessage, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate welcomeMessageAlreadyLinked = CreateTemplateEmbed("You are unable to link your {user.mention} Discord user because you're already linked to {player.name}", DiscordDanger, new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedMessageTemplate(this, TemplateKeys.WelcomeMessage.Error.AlreadyLinked, welcomeMessageAlreadyLinked, new TemplateVersion(1, 0, 0));
        }
        
        public void RegisterCommandMessages()
        {
            DiscordMessageTemplate codeSuccess = CreateTemplateEmbed($"Please join the {DiscordFormatting.Bold("{server.name}")} game server and type {DiscordFormatting.Bold($"/{{plugin.lang:{ServerLang.Commands.DcCommand}}} {ServerLinkArgument} {{discordcore.link.code}}")} in server chat.", DiscordSuccess, new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedMessageTemplate(this, TemplateKeys.Commands.Code.Success, codeSuccess, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate userSuccess = CreateTemplateEmbed($"We have sent a message to {DiscordFormatting.Bold("{player.name}")} on the {DiscordFormatting.Bold("{server.name}")} server. Please follow the directions to complete your link.", DiscordSuccess, new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedMessageTemplate(this, TemplateKeys.Commands.User.Success, userSuccess, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate userInvalidPlayer = CreateTemplateEmbed($"You have not selected a valid player from the dropdown. Please try the command again.", DiscordDanger, new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedMessageTemplate(this, TemplateKeys.Commands.User.Error.PlayerIsInvalid, userInvalidPlayer, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate userNotConnected = CreateTemplateEmbed($"Player {DiscordFormatting.Bold("{player.name}")} is not connected to the {DiscordFormatting.Bold("{server.name}")} server. Please join the server and try the command again.", DiscordDanger, new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedMessageTemplate(this, TemplateKeys.Commands.User.Error.PlayerNotConnected, userNotConnected, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate leaveNotLinked = CreateTemplateEmbed($"You are not able to unlink because you are not currently linked.", DiscordDanger, new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedMessageTemplate(this, TemplateKeys.Commands.Leave.Error.UserNotLinked, leaveNotLinked, new TemplateVersion(1, 0, 0));
        }
        
        public void RegisterAdminCommandMessages()
        {
            DiscordMessageTemplate playerNotFound = CreateTemplateEmbed($"Failed to link. Player with '{DiscordFormatting.Bold("{player.id}")}' ID was not found.", DiscordDanger, new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedMessageTemplate(this, TemplateKeys.Commands.Admin.Link.Error.PlayerNotFound, playerNotFound, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate playerAlreadyLinked = CreateTemplateEmbed($"Failed to link. Player '{DiscordFormatting.Bold("{player.name}({player.id})")}' is already linked to {{user.mention}}. If you would like to link this player please unlink first.", DiscordDanger, new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedMessageTemplate(this, TemplateKeys.Commands.Admin.Link.Error.PlayerAlreadyLinked, playerAlreadyLinked, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate userAlreadyLinked = CreateTemplateEmbed($"Failed to link. User {{user.mention}} is already linked to {{player.name}}({{player.id}}). If you would like to link this user please unlink them first.", DiscordDanger, new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedMessageTemplate(this, TemplateKeys.Commands.Admin.Link.Error.UserAlreadyLinked, userAlreadyLinked, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate adminLinkSuccess = CreateTemplateEmbed($"You have successfully linked Player '{DiscordFormatting.Bold("{player.name}({player.id})")}' to {{user.mention}}", DiscordSuccess, new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedMessageTemplate(this, TemplateKeys.Commands.Admin.Link.Success, adminLinkSuccess, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate unlinkMustSpecify = CreateTemplateEmbed($"Failed to unlink. You must specify either player or user or both.", DiscordDanger, new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedMessageTemplate(this, TemplateKeys.Commands.Admin.Unlink.Error.MustSpecifyOne, unlinkMustSpecify, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate unlinkPlayerNotLinked = CreateTemplateEmbed($"Failed to unlink.'{DiscordFormatting.Bold("{player.name}({player.id})")}' is not linked.", DiscordDanger, new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedMessageTemplate(this, TemplateKeys.Commands.Admin.Unlink.Error.PlayerIsNotLinked, unlinkPlayerNotLinked, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate unlinkUserNotLinked = CreateTemplateEmbed($"Failed to unlink. {{user.mention}} is not linked.", DiscordDanger, new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedMessageTemplate(this, TemplateKeys.Commands.Admin.Unlink.Error.UserIsNotLinked, unlinkUserNotLinked, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate unlinkNotSame = CreateTemplateEmbed($"Failed to unlink. The specified player and user are not linked to each other.\n" +
            $"Player '{DiscordFormatting.Bold("{player.name}({player.id})")}' is linked to {{discordcore.other.user.mention}}.\n" +
            $"User {{user.mention}} is linked to '{DiscordFormatting.Bold("{discordcore.other.player.name}({discordcore.other.player.id})")}'", DiscordDanger, new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedMessageTemplate(this, TemplateKeys.Commands.Admin.Unlink.Error.LinkNotSame, unlinkNotSame, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate adminUnlinkSuccess = CreateTemplateEmbed($"You have successfully unlink Player '{DiscordFormatting.Bold("{player.name}({player.id})")}' from {{user.mention}}", DiscordSuccess, new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedMessageTemplate(this, TemplateKeys.Commands.Admin.Unlink.Success, adminUnlinkSuccess, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate playerSearchNotFound = CreateTemplateEmbed($"Failed to find Player with '{DiscordFormatting.Bold("{player.id}")}' ID", DiscordDanger, new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedMessageTemplate(this, TemplateKeys.Commands.Admin.Search.Error.PlayerNotFound, playerSearchNotFound, new TemplateVersion(1, 0, 0));
            
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
                            new EmbedFieldTemplate("Player", "{player.name}({player.id})"),
                            new EmbedFieldTemplate("User", "{user.fullname}"),
                            new EmbedFieldTemplate("Is Linked", "{player.islinked}"),
                        }
                    }
                },
                Components =
                {
                    new ButtonTemplate("Steam Profile", ButtonStyle.Link, "https://steamcommunity.com/profiles/{player.id}"),
                    new ButtonTemplate("BattleMetrics Profile", ButtonStyle.Link, "https://www.battlemetrics.com/rcon/players?filter[search]={player.id}"),
                },
                Version = new TemplateVersion(1, 0, 0)
            };
            _templates.RegisterLocalizedMessageTemplate(this, TemplateKeys.Commands.Admin.Search.Success, searchSuccess, new TemplateVersion(1, 0, 0));
        }
        
        public void RegisterLinkMessages()
        {
            DiscordMessageTemplate linkCommand = CreateTemplateEmbed($"You have successfully linked {DiscordFormatting.Bold("{player.name}")} with your Discord user {{user.mention}}", DiscordSuccess, new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedMessageTemplate(this, TemplateKeys.Link.Completed.Command, linkCommand, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate linkAdmin = CreateTemplateEmbed($"You have been successfully linked with {DiscordFormatting.Bold("{player.name}")} and Discord user {{user.mention}} by an admin", DiscordSuccess, new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedMessageTemplate(this, TemplateKeys.Link.Completed.Admin, linkAdmin, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate linkApi = CreateTemplateEmbed($"You have successfully linked {DiscordFormatting.Bold("{player.name}")} with your Discord user {{user.mention}}", DiscordSuccess, new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedMessageTemplate(this, TemplateKeys.Link.Completed.Api, linkApi, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate linkRejoin = CreateTemplateEmbed($"Your {DiscordFormatting.Bold("{player.name}")} game account has been relinked with your Discord user {{user.mention}} because you rejoined the {DiscordFormatting.Bold("{guild.name}")} Discord server", DiscordSuccess, new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedMessageTemplate(this, TemplateKeys.Link.Completed.GuildRejoin, linkRejoin, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate linkInactive = CreateTemplateEmbed($"Your {DiscordFormatting.Bold("{player.name}")} game account has been relinked with your Discord user {{user.mention}} because you rejoined the {DiscordFormatting.Bold("{server.name}")} game server", DiscordSuccess, new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedMessageTemplate(this, TemplateKeys.Link.Completed.InactiveRejoin, linkInactive, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate unlinkCommand = CreateTemplateEmbed($"You have successfully unlinked {DiscordFormatting.Bold("{player.name}")} from your Discord user {{user.mention}}", DiscordSuccess, new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedMessageTemplate(this, TemplateKeys.Unlink.Completed.Command, unlinkCommand, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate unlinkAdmin = CreateTemplateEmbed($"You have successfully been unlinked {DiscordFormatting.Bold("{player.name}")} from your Discord user {{user.mention}} by an admin", DiscordSuccess, new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedMessageTemplate(this, TemplateKeys.Unlink.Completed.Admin, unlinkAdmin, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate unlinkApi = CreateTemplateEmbed($"You have successfully unlinked {DiscordFormatting.Bold("{player.name}")} from your Discord user {{user.mention}}", DiscordSuccess, new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedMessageTemplate(this, TemplateKeys.Unlink.Completed.Api, unlinkApi, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate unlinkInactive = CreateTemplateEmbed($"You have been successfully unlinked from {DiscordFormatting.Bold("{player.name}")} and Discord user {{user.mention}} because you have been inactive for {{discordcore.inactive.duration}} days", DiscordSuccess, new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedMessageTemplate(this, TemplateKeys.Unlink.Completed.Inactive, unlinkInactive, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate declineUser = CreateTemplateEmbed("We have successfully declined the link request from {player.name}. We're sorry for the inconvenience.", DiscordDanger, new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedMessageTemplate(this, TemplateKeys.Link.Declined.JoinWithUser, declineUser, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate declinePlayer = CreateTemplateEmbed("{player.name} has declined your link request. Repeated declined attempts may result in a link ban.", DiscordDanger, new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedMessageTemplate(this, TemplateKeys.Link.Declined.JoinWithPlayer, declinePlayer, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate dmLinkAccounts = CreateTemplateEmbed($"To complete the link process please join the {DiscordFormatting.Bold("{server.name}")} game server and type {DiscordFormatting.Bold($"/{{plugin.lang:{ServerLang.Commands.DcCommand}}} {ServerLinkArgument} {{discordcore.link.code}}")} in server chat.", DiscordSuccess, new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedMessageTemplate(this, TemplateKeys.Link.WelcomeMessage.DmLinkAccounts, dmLinkAccounts, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate guildLinkAccounts = CreateTemplateEmbed($"To complete the link process please join the {DiscordFormatting.Bold("{server.name}")} game server and type {DiscordFormatting.Bold($"/{{plugin.lang:{ServerLang.Commands.DcCommand}}} {ServerLinkArgument} {{discordcore.link.code}}")} in server chat.", DiscordSuccess, new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedMessageTemplate(this, TemplateKeys.Link.WelcomeMessage.GuildLinkAccounts, guildLinkAccounts, new TemplateVersion(1, 0, 0));
        }
        
        public void RegisterBanMessages()
        {
            DiscordMessageTemplate banned = CreateTemplateEmbed($"You have been banned from making any more player link requests for {{discordcore.join.banned.duration:0.##}} hours due to multiple declined requests.", DiscordDanger, new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedMessageTemplate(this, TemplateKeys.Banned.PlayerBanned, banned, new TemplateVersion(1, 0, 0));
        }
        
        public void RegisterJoinMessages()
        {
            DiscordMessageTemplate byUsername = CreateTemplateEmbed($"The player {DiscordFormatting.Bold("{player.name}")} is trying to link their game account to this discord user.\n" +
            $"If you could like to accept please click on the {DiscordFormatting.Bold("Accept")} button.\n" +
            $"If you did not initiate this link please click on the {DiscordFormatting.Bold("Decline")} button", DiscordSuccess, new TemplateVersion(1, 0, 0));
            byUsername.Components = new List<BaseComponentTemplate>
            {
                new ButtonTemplate("Accept", ButtonStyle.Success, AcceptLinkButtonId, AcceptEmoji),
                new ButtonTemplate("Decline", ButtonStyle.Danger, DeclineLinkButtonId, DeclineEmoji)
            };
            _templates.RegisterLocalizedMessageTemplate(this, TemplateKeys.Join.ByUsername, byUsername, new TemplateVersion(1, 0, 0));
        }
        
        public void RegisterErrorMessages()
        {
            DiscordMessageTemplate userAlreadyLinked = CreateTemplateEmbed($"You are unable to link because you are already linked to player {DiscordFormatting.Bold("{player.name}")}", DiscordDanger, new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedMessageTemplate(this, TemplateKeys.Errors.UserAlreadyLinked, userAlreadyLinked, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate playerAlreadyLinked = CreateTemplateEmbed($"You are unable to link to player {DiscordFormatting.Bold("{player.name}")} because they are already linked" , DiscordDanger, new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedMessageTemplate(this, TemplateKeys.Errors.PlayerAlreadyLinked, playerAlreadyLinked, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate codeActivationNotFound = CreateTemplateEmbed($"We failed to find a pending link activation for the code {DiscordFormatting.Bold("{discordcore.error.invalid.code}")}. Please confirm you have the correct code and try again." , DiscordDanger, new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedMessageTemplate(this, TemplateKeys.Errors.CodActivationNotFound, codeActivationNotFound, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate lookupActivationNotFound = CreateTemplateEmbed($"We failed to find a pending link activation for the code {DiscordFormatting.Bold("{user.fullname}")}. Please confirm you have started that activation from the game server for this user." , DiscordDanger, new TemplateVersion(1, 0, 0));
            _templates.RegisterLocalizedMessageTemplate(this, TemplateKeys.Errors.LookupActivationNotFound, lookupActivationNotFound, new TemplateVersion(1, 0, 0));
        }
        
        public DiscordMessageTemplate CreateTemplateEmbed(string description, string color, TemplateVersion version)
        {
            return new DiscordMessageTemplate
            {
                Embeds = new List<DiscordEmbedTemplate>
                {
                    new DiscordEmbedTemplate
                    {
                        Description = $"[{{plugin.title}}] {description}",
                        Color = $"#{color}"
                    }
                },
                Version = version
            };
        }
        
        public void SendTemplateMessage(string key, DiscordInteraction interaction, PlaceholderData placeholders = null)
        {
            Puts($"Keys: {placeholders?.GetKeys()}");
            InteractionCallbackData response = new InteractionCallbackData();
            if (interaction.GuildId.HasValue)
            {
                response.Flags = MessageFlags.Ephemeral;
            }
            
            interaction.CreateTemplateInteractionResponse(Client, this, InteractionResponseType.ChannelMessageWithSource, key, response, placeholders);
        }
        
        public void SendTemplateMessage(string key, DiscordUser user, IPlayer player = null, PlaceholderData placeholders = null)
        {
            AddDefaultPlaceholders(ref placeholders, user, player);
            user.SendTemplateDirectMessage(Client, this, key, _lang.GetPlayerLanguage(player), null, placeholders);
        }
        
        public void SendGlobalTemplateMessage(string key, DiscordUser user, IPlayer player = null, PlaceholderData placeholders = null)
        {
            AddDefaultPlaceholders(ref placeholders, user, player);
            user.SendGlobalTemplateDirectMessage(Client, this, key, null, placeholders);
        }
        
        public void SendGlobalTemplateMessage(string key, Snowflake channelId, DiscordUser user = null, IPlayer player = null, PlaceholderData placeholders = null, Action<DiscordMessage> callback = null)
        {
            DiscordChannel channel = Guild.Channels[channelId];
            if (channel != null)
            {
                AddDefaultPlaceholders(ref placeholders, user, player);
                channel.CreateGlobalTemplateMessage(Client, this, key, null, placeholders, callback);
            }
        }
        
        public void UpdateGuildTemplateMessage(string key, DiscordMessage message, PlaceholderData placeholders = null)
        {
            AddDefaultPlaceholders(ref placeholders, null, null);
            message.EditGlobalTemplateMessage(Client, this, key, placeholders);
        }
        
        private void AddDefaultPlaceholders(ref PlaceholderData placeholders, DiscordUser user, IPlayer player)
        {
            placeholders = placeholders ?? GetDefault();
            placeholders.AddUser(user).AddPlayer(player).AddGuild(Guild);
        }
        #endregion

        #region Plugins\DiscordCore.Placeholders.cs
        private const string BanDurationKey = "ban.duration";
        private const string CodeKey = "dc.code";
        private const string OtherPlayerDataKey = "discordcore.other.player";
        private const string OtherUserDataKey = "discordcore.other.user";
        
        public void RegisterPlaceholders()
        {
            if (!string.IsNullOrEmpty(_pluginConfig.ServerNameOverride))
            {
                _placeholders.RegisterPlaceholder(this, "guild.name", _pluginConfig.ServerNameOverride);
            }
            
            _placeholders.RegisterPlaceholder(this, "discordcore.invite.code", _pluginConfig.InviteCode);
            _placeholders.RegisterPlaceholder(this, "discordcore.server.link.arg", ServerLinkArgument);
            _placeholders.RegisterPlaceholder(this, "discordcore.inactive.duration", InactiveDays);
            _placeholders.RegisterPlaceholder<TimeSpan>(this, "discordcore.join.banned.duration", BanDurationKey, BanDuration);
            _placeholders.RegisterPlaceholder<string>(this, "discordcore.link.code", CodeKey, Code);
            _placeholders.RegisterPlaceholder<string>(this, "discordcore.error.invalid.code", CodeKey, Code);
            PlayerPlaceholders.RegisterPlaceholders(this, "discordcore.other.player", OtherPlayerDataKey);
            UserPlaceholders.RegisterPlaceholders(this, "discordcore.other.user", OtherUserDataKey);
        }
        
        private void InactiveDays(StringBuilder builder, PlaceholderState state) => PlaceholderFormatting.Replace(builder, state, _pluginConfig.LinkSettings.UnlinkInactiveDays);
        private static void BanDuration(StringBuilder builder, PlaceholderState state, TimeSpan duration) => PlaceholderFormatting.Replace(builder, state, duration.TotalHours);
        private static void Code(StringBuilder builder, PlaceholderState state, string code) => PlaceholderFormatting.Replace(builder, state, code);
        
        public string LangPlaceholder(string key, PlaceholderData data)
        {
            return _placeholders.ProcessPlaceholders(Lang(key), data);
        }
        
        public PlaceholderData GetDefault()
        {
            return _placeholders.CreateData(this).AddGuild(Guild);
        }
        
        public PlaceholderData GetDefault(IPlayer player)
        {
            return GetDefault().AddPlayer(player);
        }
        
        public PlaceholderData GetDefault(DiscordUser user)
        {
            return GetDefault().AddUser(user);
        }
        
        public PlaceholderData GetDefault(IPlayer player, DiscordUser user)
        {
            return GetDefault(player).AddUser(user);
        }
        #endregion

        #region Plugins\DiscordCore.Link.cs
        public IDictionary<string, Snowflake> GetSteamToDiscordIds()
        {
            Hash<string, Snowflake> data = new Hash<string, Snowflake>();
            foreach (DiscordInfo info in _pluginData.PlayerDiscordInfo.Values)
            {
                data[info.PlayerId] = info.DiscordId;
            }
            
            return data;
        }
        #endregion

        #region Plugins\DiscordCore.Helpers.cs
        public void SaveData()
        {
            if (_pluginData != null)
            {
                Interface.Oxide.DataFileSystem.WriteObject(Name, _pluginData);
            }
        }
        
        public void LogGuildLeaveUnlink(IPlayer player, DiscordUser user)
        {
            Puts($"Player {player.Name}({player.Id}) Discord {user.GetFullUserName}({user.Id}) is no longer in the guild and has been unlinked.");
        }
        
        public void LogServerInactiveUnlink(IPlayer player, DiscordUser user)
        {
            Puts($"Player {player.Name}({player.Id}) Discord {user.GetFullUserName}({user.Id}) has been inactive for {{0}} days and has been unlinked.");
        }
        
        public void LogApiUnlink(IPlayer player, DiscordUser user)
        {
            Puts($"Player {player.Name}({player.Id}) Discord {user.GetFullUserName}({user.Id}) has been unlinked using through the API.");
        }
        #endregion

        #region Plugins\DiscordCore.API.cs
        //Define:FileOrder=65
        private string API_Link(IPlayer player, DiscordUser user)
        {
            if (player.IsLinked())
            {
                return ApiErrorCodes.PlayerIsLinked;
            }
            
            if (user.IsLinked())
            {
                return  ApiErrorCodes.UserIsLinked;
            }
            
            _linkHandler.HandleLink(player, user, LinkReason.Api, null);
            return null;
        }
        
        private string API_Unlink(IPlayer player, DiscordUser user)
        {
            if (!player.IsLinked())
            {
                return  ApiErrorCodes.PlayerIsNotLinked;
            }
            
            if (!user.IsLinked())
            {
                return ApiErrorCodes.UserIsNotLinked;
            }
            
            _linkHandler.HandleUnlink(player, user, UnlinkedReason.Api, null);
            return null;
        }
        #endregion

        #region Plugins\DiscordCore.DiscordSetup.cs
        //Define:FileOrder=25
        public void SetupGuildWelcomeMessage()
        {
            GuildLinkMessageSettings settings = _pluginConfig.LinkMessageSettings;
            if (!settings.Enabled)
            {
                return;
            }
            
            if (!settings.ChannelId.IsValid())
            {
                PrintWarning("Link message is enabled but link message channel ID is not valid");
                return;
            }
            
            DiscordChannel channel = Guild.Channels[settings.ChannelId];
            if (channel == null)
            {
                PrintWarning($"Link message failed to find channel with ID {settings.ChannelId}");
                return;
            }
            
            if (_pluginData.MessageData == null)
            {
                CreateGuildWelcomeMessage(settings);
                return;
            }
            
            channel.GetChannelMessage(Client, _pluginData.MessageData.MessageId, message =>
            {
                UpdateGuildTemplateMessage(TemplateKeys.WelcomeMessage.GuildWelcomeMessage, message);
            },
            error =>
            {
                if (error.HttpStatusCode == 404)
                {
                    error.SuppressErrorMessage();
                    PrintWarning("The previous link message has been removed. Recreating the message.");
                    CreateGuildWelcomeMessage(settings);
                }
            });
        }
        
        private void CreateGuildWelcomeMessage(GuildLinkMessageSettings settings)
        {
            SendGlobalTemplateMessage(TemplateKeys.WelcomeMessage.GuildWelcomeMessage, settings.ChannelId, null, null, null, message =>
            {
                _pluginData.MessageData = new LinkMessageData(message.ChannelId, message.Id);
            });
        }
        #endregion

        #region Api\ApiErrorCodes.cs
        public static class ApiErrorCodes
        {
            public const string PlayerIsLinked = "Error.Player.IsLinked";
            public const string PlayerIsNotLinked = "Error.Player.IsNotLinked";
            public const string UserIsLinked = "Error.User.IsLinked";
            public const string UserIsNotLinked = "Error.User.IsNotLinked";
        }
        #endregion

        #region AppCommands\AdminAppCommands.cs
        public static class AdminAppCommands
        {
            public const string Command = "dca";
            public const string Description =  "Discord Core Admin Commands";
            
            public static class Link
            {
                public const string Command = "link";
                public const string Description = "admin link player game account and Discord user";
                
                public static class Args
                {
                    public static class Player
                    {
                        public const string Name = "player";
                        public const string Description = "player to link";
                    }
                    
                    public static class User
                    {
                        public const string Name = "user";
                        public const string Description = "user to link";
                    }
                }
            }
            
            public static class Unlink
            {
                public const string Command = "unlink";
                public const string Description =  "admin unlink player game account and Discord user";
                
                public static class Args
                {
                    public static class Player
                    {
                        public const string Name = "player";
                        public const string Description = "player to unlink";
                    }
                    
                    public static class User
                    {
                        public const string Name = "user";
                        public const string Description = "user to unlink";
                    }
                }
            }
            
            public static class Search
            {
                public const string Command = "search";
                public const string Description =  "search linked accounts by discord or player";
                
                public static class SubCommand
                {
                    public static class Player
                    {
                        public const string Command = "player";
                        public const string Description = "search by player";
                        
                        public static class Args
                        {
                            public static class Players
                            {
                                public const string Name = "player";
                                public const string Description = "player to search";
                            }
                        }
                    }
                    
                    public static class User
                    {
                        public const string Command = "user";
                        public const string Description = "search by user";
                        
                        public static class Args
                        {
                            public static class Users
                            {
                                public const string Name = "user";
                                public const string Description = "user to search";
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region AppCommands\UserAppCommands.cs
        public static class UserAppCommands
        {
            public const string Command = "dc";
            public const string Description = "Discord Core Commands";
            
            public static class Code
            {
                public const string Command = "code";
                public const string Description = "start the link between discord and the game server using a link code";
            }
            
            public static class User
            {
                public const string Command = "user";
                public const string Description = "start the link between discord and the game server by game server player name";
                
                public static class Args
                {
                    public static class Player
                    {
                        public const string Name = "player";
                        public const string Description = "Player name on the game server";
                    }
                }
            }
            
            public static class Leave
            {
                public const string Command = "leave";
                public const string Description = "unlink your discord and game server accounts";
            }
            
            public static class Link
            {
                public const string Command = "link";
                public const string Description =  "complete the link using the given link code";
                
                public static class Args
                {
                    
                    public static class Code
                    {
                        public const string Name = "code";
                        public const string Description = "code to complete the link";
                    }
                }
            }
        }
        #endregion

        #region Configuration\GuildLinkMessageSettings.cs
        public class GuildLinkMessageSettings
        {
            [JsonProperty(PropertyName = "Enable Guild Link Message")]
            public bool Enabled { get; set; }
            
            [JsonProperty(PropertyName = "Message Channel ID")]
            public Snowflake ChannelId { get; set; }
            
            public GuildLinkMessageSettings(GuildLinkMessageSettings settings)
            {
                Enabled = settings?.Enabled ?? false;
                ChannelId = settings?.ChannelId ?? default(Snowflake);
            }
        }
        #endregion

        #region Configuration\LinkBanSettings.cs
        public class LinkBanSettings
        {
            [JsonProperty(PropertyName = "Enable Link Ban")]
            public bool EnableLinkBanning { get; set; }
            
            [JsonProperty(PropertyName = "Ban Link After X Join Declines")]
            public int BanDeclineAmount { get; set; }
            
            [JsonProperty(PropertyName = "Ban Duration (Hours)")]
            public int BanDuration { get; set; }
            
            public LinkBanSettings(LinkBanSettings settings)
            {
                EnableLinkBanning = settings?.EnableLinkBanning ?? true;
                BanDeclineAmount = settings?.BanDeclineAmount ?? 3;
                BanDuration = settings?.BanDuration ?? 24;
            }
        }
        #endregion

        #region Configuration\LinkPermissionSettings.cs
        public class LinkPermissionSettings
        {
            [JsonProperty(PropertyName = "On Link Permissions To Add")]
            public List<string> LinkPermissions { get; set; }
            
            [JsonProperty(PropertyName = "On Unlink Permissions To Remove")]
            public List<string> UnlinkPermissions { get; set; }
            
            [JsonProperty(PropertyName = "On Link Groups To Add")]
            public List<string> LinkGroups { get; set; }
            
            [JsonProperty(PropertyName = "On Unlink Groups To Remove")]
            public List<string> UnlinkGroups { get; set; }
            
            [JsonProperty(PropertyName = "On Link Roles To Add")]
            public List<Snowflake> LinkRoles { get; set; }
            
            [JsonProperty(PropertyName = "On Unlink Roles To Remove")]
            public List<Snowflake> UnlinkRoles { get; set; }
            
            public LinkPermissionSettings(LinkPermissionSettings settings)
            {
                LinkPermissions = settings?.LinkPermissions ?? new List<string>();
                LinkGroups = settings?.LinkGroups ?? new List<string>();
                LinkRoles = settings?.LinkRoles ?? new List<Snowflake>();
                UnlinkPermissions = settings?.UnlinkPermissions ?? new List<string>();
                UnlinkGroups = settings?.UnlinkGroups ?? new List<string>();
                UnlinkRoles = settings?.UnlinkRoles ?? new List<Snowflake>();
            }
        }
        #endregion

        #region Configuration\LinkSettings.cs
        public class LinkSettings
        {
            [JsonProperty(PropertyName = "Announcement Channel Id")]
            public Snowflake AnnouncementChannel { get; set; }
            
            [JsonProperty(PropertyName = "Link Code Generator Characters")]
            public string LinkCodeCharacters { get; set; }
            
            [JsonProperty(PropertyName = "Link Code Generator Length")]
            public int LinkCodeLength { get; set; }
            
            [JsonProperty(PropertyName = "Automatically Relink A Player If They Leave And Rejoin The Discord Server")]
            public bool AutoRelinkPlayer { get; set; }
            
            [JsonProperty(PropertyName = "Automatically Unlink Inactive Players After X Days")]
            public bool UnlinkInactive { get; set; }
            
            [JsonProperty(PropertyName = "Consider Player Inactive After X (Days)")]
            public float UnlinkInactiveDays { get; set; }
            
            [JsonProperty(PropertyName = "Automatically Relink Inactive Players If They Join The Game Server")]
            public bool AutoRelinkInactive { get; set; }
            
            public LinkSettings(LinkSettings settings)
            {
                AnnouncementChannel = settings?.AnnouncementChannel ?? default(Snowflake);
                LinkCodeCharacters = settings?.LinkCodeCharacters ?? "123456789";
                LinkCodeLength = settings?.LinkCodeLength ?? 6;
                if (LinkCodeLength <= 0)
                {
                    LinkCodeLength = 6;
                }
                AutoRelinkPlayer = settings?.AutoRelinkPlayer ?? true;
                UnlinkInactive = settings?.UnlinkInactive ?? false;
                UnlinkInactiveDays = settings?.UnlinkInactiveDays ?? 90;
                
            }
        }
        #endregion

        #region Configuration\PluginConfig.cs
        public class PluginConfig
        {
            [DefaultValue("")]
            [JsonProperty(PropertyName = "Discord Bot Token")]
            public string ApiKey { get; set; }
            
            [JsonProperty(PropertyName = "Discord Server ID (Optional if bot only in 1 guild)")]
            public Snowflake GuildId { get; set; }
            
            [DefaultValue("")]
            [JsonProperty(PropertyName = "Discord Server Name Override")]
            public string ServerNameOverride { get; set; }
            
            [DefaultValue("")]
            [JsonProperty(PropertyName = "Discord Server Invite Code")]
            public string InviteCode { get; set; }
            
            [JsonProperty(PropertyName = "Link Settings")]
            public LinkSettings LinkSettings { get; set; }
            
            [JsonProperty(PropertyName = "Welcome Message Settings")]
            public WelcomeMessageSettings WelcomeMessageSettings { get; set; }
            
            [JsonProperty(PropertyName = "Guild Link Message Settings")]
            public GuildLinkMessageSettings LinkMessageSettings { get; set; }
            
            [JsonProperty(PropertyName = "Link Permission Settings")]
            public LinkPermissionSettings PermissionSettings { get; set; }
            
            [JsonProperty(PropertyName = "Link Ban Settings")]
            public LinkBanSettings LinkBanSettings { get; set; }
            
            [JsonConverter(typeof(StringEnumConverter))]
            [DefaultValue(DiscordLogLevel.Info)]
            [JsonProperty(PropertyName = "Discord Extension Log Level (Verbose, Debug, Info, Warning, Error, Exception, Off)")]
            public DiscordLogLevel ExtensionDebugging { get; set; }
        }
        #endregion

        #region Configuration\WelcomeMessageSettings.cs
        public class WelcomeMessageSettings
        {
            [JsonProperty(PropertyName = "Enable Welcome DM Message")]
            public bool EnableWelcomeMessage { get; set; }
            
            [JsonProperty(PropertyName = "Send Welcome Message On Discord Server Join")]
            public bool SendOnGuildJoin { get; set; }
            
            [JsonProperty(PropertyName = "Send Welcome Message On Role ID Added")]
            public List<Snowflake> SendOnRoleAdded { get; set; }
            
            [JsonProperty(PropertyName = "Add Link Accounts Button In Welcome Message")]
            public bool EnableLinkButton { get; set; }
            
            public WelcomeMessageSettings(WelcomeMessageSettings settings)
            {
                EnableWelcomeMessage = settings?.EnableWelcomeMessage ?? true;
                SendOnGuildJoin = settings?.SendOnGuildJoin ?? false;
                SendOnRoleAdded = settings?.SendOnRoleAdded ?? new List<Snowflake> {new Snowflake(1234567890)};
                EnableLinkButton = settings?.EnableLinkButton ?? true;
            }
        }
        #endregion

        #region Data\DiscordInfo.cs
        public class DiscordInfo
        {
            public Snowflake DiscordId { get; set; }
            public string PlayerId { get; set; }
            public DateTime LastOnline { get; set; }
            
            [JsonConstructor]
            public DiscordInfo() { }
            
            public DiscordInfo(IPlayer player, DiscordUser user)
            {
                PlayerId = player.Id;
                DiscordId = user.Id;
                LastOnline = DateTime.UtcNow;
            }
        }
        #endregion

        #region Data\LinkMessageData.cs
        public class LinkMessageData
        {
            public Snowflake ChannelId { get; set; }
            public Snowflake MessageId { get; set; }
            
            [JsonConstructor]
            public LinkMessageData() { }
            
            public LinkMessageData(Snowflake channelId, Snowflake messageId)
            {
                ChannelId = channelId;
                MessageId = messageId;
            }
        }
        #endregion

        #region Data\PluginData.cs
        public class PluginData
        {
            public Hash<string, DiscordInfo> PlayerDiscordInfo = new Hash<string, DiscordInfo>();
            public Hash<Snowflake, DiscordInfo> LeftPlayerInfo = new Hash<Snowflake, DiscordInfo>();
            public Hash<string, DiscordInfo> InactivePlayerInfo = new Hash<string, DiscordInfo>();
            public LinkMessageData MessageData;
        }
        #endregion

        #region Enums\JoinedFrom.cs
        public enum JoinedFrom
        {
            Server,
            Discord
        }
        #endregion

        #region Enums\LinkReason.cs
        public enum LinkReason
        {
            Command,
            Admin,
            Api,
            GuildRejoin,
            InactiveRejoin
        }
        #endregion

        #region Enums\UnlinkedReason.cs
        public enum UnlinkedReason
        {
            Command,
            Admin,
            Api,
            LeftGuild,
            Inactive
        }
        #endregion

        #region Link\JoinBanData.cs
        public class JoinBanData
        {
            public int Times { get; private set; }
            private DateTime _bannedUntil;
            
            public void AddDeclined()
            {
                Times++;
            }
            
            public bool IsBanned()
            {
                return _bannedUntil > DateTime.UtcNow;
            }
            
            public TimeSpan GetRemainingBan()
            {
                return _bannedUntil - DateTime.UtcNow;
            }
            
            public void SetBanDuration(float hours)
            {
                _bannedUntil = DateTime.UtcNow.AddHours(hours);
            }
        }
        #endregion

        #region Link\JoinBanHandler.cs
        public class JoinBanHandler
        {
            private readonly Hash<string, JoinBanData> _playerBans = new Hash<string, JoinBanData>();
            private readonly Hash<Snowflake, JoinBanData> _discordBans = new Hash<Snowflake, JoinBanData>();
            private readonly LinkBanSettings _settings;
            
            public JoinBanHandler(LinkBanSettings settings)
            {
                _settings = settings;
            }
            
            public void AddBan(IPlayer player)
            {
                if (!_settings.EnableLinkBanning)
                {
                    return;
                }
                
                JoinBanData ban = GetBan(player);
                
                ban.AddDeclined();
                if (ban.Times >= _settings.BanDeclineAmount)
                {
                    ban.SetBanDuration(_settings.BanDuration);
                }
            }
            
            public void AddBan(DiscordUser user)
            {
                if (!_settings.EnableLinkBanning)
                {
                    return;
                }
                
                JoinBanData ban = GetBan(user);
                
                ban.AddDeclined();
                if (ban.Times >= _settings.BanDeclineAmount)
                {
                    ban.SetBanDuration(_settings.BanDuration);
                }
            }
            
            public bool IsBanned(IPlayer player)
            {
                if (!_settings.EnableLinkBanning)
                {
                    return false;
                }
                
                JoinBanData ban = _playerBans[player.Id];
                return ban != null && ban.IsBanned();
            }
            
            public bool IsBanned(DiscordUser user)
            {
                if (!_settings.EnableLinkBanning)
                {
                    return false;
                }
                
                JoinBanData ban = _discordBans[user.Id];
                return ban != null && ban.IsBanned();
            }
            
            public TimeSpan GetRemainingBan(IPlayer player)
            {
                if (!_settings.EnableLinkBanning)
                {
                    return TimeSpan.Zero;
                }
                
                return _playerBans[player.Id]?.GetRemainingBan() ?? TimeSpan.Zero;
            }
            
            public TimeSpan GetRemainingBan(DiscordUser user)
            {
                if (!_settings.EnableLinkBanning)
                {
                    return TimeSpan.Zero;
                }
                
                return _discordBans[user.Id]?.GetRemainingBan() ?? TimeSpan.Zero;
            }
            
            private JoinBanData GetBan(IPlayer player)
            {
                JoinBanData ban = _playerBans[player.Id];
                if (ban == null)
                {
                    ban = new JoinBanData();
                    _playerBans[player.Id] = ban;
                }
                
                return ban;
            }
            
            private JoinBanData GetBan(DiscordUser user)
            {
                JoinBanData ban = _discordBans[user.Id];
                if (ban == null)
                {
                    ban = new JoinBanData();
                    _discordBans[user.Id] = ban;
                }
                
                return ban;
            }
        }
        #endregion

        #region Link\JoinData.cs
        public class JoinData
        {
            public IPlayer Player { get; set; }
            public DiscordUser Discord { get; set; }
            public string Code { get; set; }
            public JoinedFrom From { get; }
            
            public JoinData(JoinedFrom from)
            {
                From = from;
            }
            
            public bool IsCompleted()
            {
                return Player != null && Discord != null && Discord.Id.IsValid();
            }
            
            public bool IsMatch(IPlayer player)
            {
                return Player != null && player != null && Player.Id == player.Id;
            }
            
            public bool IsMatch(DiscordUser user)
            {
                return Discord != null && user != null && Discord.Id == user.Id;
            }
        }
        #endregion

        #region Link\JoinHandler.cs
        public class JoinHandler
        {
            private readonly List<JoinData> _activations = new List<JoinData>();
            private readonly LinkSettings _settings;
            private readonly LinkHandler _linkHandler;
            private readonly JoinBanHandler _ban;
            private readonly DiscordCore _plugin = DiscordCore.Instance;
            
            public JoinHandler(LinkSettings settings, LinkHandler linkHandler, JoinBanHandler ban)
            {
                _settings = settings;
                _linkHandler = linkHandler;
                _ban = ban;
            }
            
            public JoinData FindByCode(string code)
            {
                for (int index = 0; index < _activations.Count; index++)
                {
                    JoinData activation = _activations[index];
                    if (activation.Code.Equals(code, StringComparison.OrdinalIgnoreCase))
                    {
                        return activation;
                    }
                }
                
                return null;
            }
            
            public JoinData FindCompletedByPlayer(IPlayer player)
            {
                for (int index = 0; index < _activations.Count; index++)
                {
                    JoinData activation = _activations[index];
                    if (activation.IsCompleted() && activation.Player.Id.Equals(player.Id, StringComparison.OrdinalIgnoreCase))
                    {
                        return activation;
                    }
                }
                
                return null;
            }
            
            public JoinData FindCompletedByUser(DiscordUser user)
            {
                for (int index = 0; index < _activations.Count; index++)
                {
                    JoinData activation = _activations[index];
                    if (activation.IsCompleted() && activation.Discord.Id == user.Id)
                    {
                        return activation;
                    }
                }
                
                return null;
            }
            
            public void RemoveByPlayer(IPlayer player)
            {
                for (int index = _activations.Count - 1; index >= 0; index--)
                {
                    JoinData activation = _activations[index];
                    if (activation.IsMatch(player))
                    {
                        _activations.RemoveAt(index);
                    }
                }
            }
            
            public void RemoveByUser(DiscordUser user)
            {
                for (int index = _activations.Count - 1; index >= 0; index--)
                {
                    JoinData activation = _activations[index];
                    if (activation.IsMatch(user))
                    {
                        _activations.RemoveAt(index);
                    }
                }
            }
            
            public JoinData CreateActivation(IPlayer player)
            {
                if (player == null) throw new ArgumentNullException(nameof(player));
                
                RemoveByPlayer(player);
                JoinData activation = new JoinData(JoinedFrom.Server)
                {
                    Code = GenerateCode(),
                    Player = player
                };
                _activations.Add(activation);
                return activation;
            }
            
            public JoinData CreateActivation(DiscordUser user)
            {
                if (user == null) throw new ArgumentNullException(nameof(user));
                
                RemoveByUser(user);
                JoinData activation = new JoinData(JoinedFrom.Discord)
                {
                    Code = GenerateCode(),
                    Discord = user
                };
                _activations.Add(activation);
                return activation;
            }
            
            public JoinData CreateActivation(IPlayer player, DiscordUser user, JoinedFrom from)
            {
                if (user == null) throw new ArgumentNullException(nameof(user));
                
                RemoveByPlayer(player);
                RemoveByUser(user);
                JoinData activation = new JoinData(from)
                {
                    Discord = user,
                    Player = player
                };
                _activations.Add(activation);
                return activation;
            }
            
            private string GenerateCode()
            {
                StringBuilder sb = DiscordPool.GetStringBuilder();
                for (int i = 0; i < _settings.LinkCodeLength; i++)
                {
                    sb.Append(_settings.LinkCodeCharacters[Oxide.Core.Random.Range(0, _settings.LinkCodeCharacters.Length)]);
                }
                
                string code = sb.ToString();
                DiscordPool.FreeStringBuilder(ref sb);
                return code;
            }
            
            public void CompleteLink(JoinData data, DiscordInteraction interaction)
            {
                IPlayer player = data.Player;
                DiscordUser user = data.Discord;
                
                _activations.Remove(data);
                RemoveByPlayer(data.Player);
                RemoveByUser(data.Discord);
                
                _linkHandler.HandleLink(player, user, LinkReason.Command, interaction);
            }
            
            public void DeclineLink(JoinData data, DiscordInteraction interaction)
            {
                _activations.Remove(data);
                
                if (data.From == JoinedFrom.Server)
                {
                    _ban.AddBan(data.Player);
                    RemoveByPlayer(data.Player);
                    _plugin.Chat(data.Player, ServerLang.Link.Declined.JoinWithUser, _plugin.GetDefault(data.Player, data.Discord));
                    _plugin.SendTemplateMessage(TemplateKeys.Link.Declined.JoinWithUser, interaction);
                }
                else if (data.From == JoinedFrom.Discord)
                {
                    _ban.AddBan(data.Discord);
                    RemoveByUser(data.Discord);
                    _plugin.Chat(data.Player, ServerLang.Link.Declined.JoinWithPlayer, _plugin.GetDefault(data.Player, data.Discord));
                    _plugin.SendTemplateMessage(TemplateKeys.Link.Declined.JoinWithPlayer, interaction);
                }
            }
        }
        #endregion

        #region Link\LinkHandler.cs
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
                _linkMessages[reason]?.SendMessages(player, user, interaction);
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
                
                if (reason == UnlinkedReason.LeftGuild)
                {
                    _pluginData.LeftPlayerInfo[info.DiscordId] = info;
                }
                else if (reason == UnlinkedReason.Inactive)
                {
                    _pluginData.InactivePlayerInfo[info.PlayerId] = info;
                }
                
                _pluginData.PlayerDiscordInfo.Remove(player.Id);
                _link.OnUnlinked(_plugin, player, user);
                _unlinkMessages[reason]?.SendMessages(player, user, interaction);
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
                
                if (_settings.AutoRelinkInactive)
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
                            HandleUnlink(player, user, UnlinkedReason.LeftGuild, null);;
                        }
                    }
                    
                    if (_settings.UnlinkInactive && info.LastOnline + TimeSpan.FromDays(_settings.UnlinkInactiveDays) < DateTime.UtcNow)
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
                    DiscordCore.Instance.Guild.AddGuildMemberRole(_plugin.Client, user.Id, role);
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
                        DiscordCore.Instance.Guild.RemoveGuildMemberRole(_plugin.Client, user.Id, role);
                    }
                }
            }
        }
        #endregion

        #region Link\LinkMessage.cs
        public class LinkMessage
        {
            private readonly string _chatLang;
            private readonly string _chatAnnouncement;
            private readonly string _discordTemplate;
            private readonly string _announcementTemplate;
            private readonly DiscordCore _plugin;
            private readonly LinkSettings _link;
            
            public LinkMessage(string chatLang, string chatAnnouncement, string discordTemplate, string announcementTemplate, DiscordCore plugin, LinkSettings link)
            {
                _chatLang = chatLang;
                _chatAnnouncement = chatAnnouncement;
                _discordTemplate = discordTemplate;
                _announcementTemplate = announcementTemplate;
                _plugin = plugin;
                _link = link;
            }
            
            public void SendMessages(IPlayer player, DiscordUser user, DiscordInteraction interaction)
            {
                using (PlaceholderData data = _plugin.GetDefault(player, user))
                {
                    data.ManualPool();
                    _plugin.BroadcastMessage(_chatAnnouncement, data);
                    _plugin.Chat(player, _chatLang, data);
                    if (!string.IsNullOrEmpty(_discordTemplate))
                    {
                        if (interaction != null)
                        {
                            _plugin.SendTemplateMessage(_discordTemplate, interaction, data);
                        }
                        else
                        {
                            _plugin.SendTemplateMessage(_discordTemplate, user, player, data);
                        }
                    }
                    
                    _plugin.SendGlobalTemplateMessage(_announcementTemplate, _link.AnnouncementChannel, user, player, data);
                }
            }
        }
        #endregion

        #region Localization\AdminAppCommandKeys.cs
        public static class AdminAppCommandKeys
        {
            private const string Base = "AppCommand.Admin.";
            
            public const string Command = Base + nameof(Command);
            public const string Description = Base + nameof(Description);
            
            public static class Link
            {
                private const string Base = AdminAppCommandKeys.Base + nameof(Link) + ".";
                
                public const string Command = Base + nameof(Command);
                public const string Description = Base + nameof(Description);
                
                public static class Args
                {
                    private const string Base = Link.Base + nameof(Args) + ".";
                    
                    public static class Player
                    {
                        private const string Base = Args.Base + nameof(Player) + ".";
                        
                        public const string Name = Base + nameof(Name);
                        public const string Description = Base + nameof(Description);
                    }
                    
                    public static class User
                    {
                        private const string Base = Args.Base + nameof(User) + ".";
                        
                        public const string Name = Base + nameof(Name);
                        public const string Description = Base + nameof(Description);
                    }
                }
            }
            
            public static class Unlink
            {
                private const string Base = AdminAppCommandKeys.Base + nameof(Unlink) + ".";
                
                public const string Command = Base + nameof(Command);
                public const string Description = Base + nameof(Description);
                
                public static class Args
                {
                    private const string Base = Unlink.Base + nameof(Args) + ".";
                    
                    public static class Player
                    {
                        private const string Base = Args.Base + nameof(Player) + ".";
                        
                        public const string Name = Base + nameof(Name);
                        public const string Description = Base + nameof(Description);
                    }
                    
                    public static class User
                    {
                        private const string Base = Args.Base + nameof(User) + ".";
                        
                        public const string Name = Base + nameof(Name);
                        public const string Description = Base + nameof(Description);
                    }
                }
            }
            
            public static class Search
            {
                private const string Base = AdminAppCommandKeys.Base + nameof(Search) + ".";
                
                public const string Command = Base + nameof(Command);
                public const string CommandDescription = Base + nameof(CommandDescription);
                
                public static class SubCommand
                {
                    private const string Base = Search.Base + nameof(SubCommand) + ".";
                    
                    public static class Player
                    {
                        private const string Base = SubCommand.Base + nameof(Player) + ".";
                        
                        public const string Command = Base + nameof(Command);
                        public const string Description = Base + nameof(Description);
                        
                        public static class Args
                        {
                            private const string Base = Player.Base + nameof(Args) + ".";
                            
                            public static class Players
                            {
                                private const string Base = Args.Base + nameof(Players) + ".";
                                
                                public const string Name = Base + nameof(Name);
                                public const string Description = Base + nameof(Description);
                            }
                        }
                    }
                    
                    public static class User
                    {
                        private const string Base = SubCommand.Base + nameof(User) + ".";
                        
                        public const string Command = Base + nameof(Command);
                        public const string Description = Base + nameof(Description);
                        
                        public static class Args
                        {
                            private const string Base = User.Base + nameof(Args) + ".";
                            
                            public static class Users
                            {
                                private const string Base = Args.Base + nameof(Users) + ".";
                                
                                public const string Name = Base + nameof(Name);
                                public const string Description = Base + nameof(Description);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Localization\ServerLang.cs
        public static class ServerLang
        {
            private const string Base = "Chat.";
            
            public const string Format = Base + nameof(Format);
            public const string NoPermission = Base + nameof(NoPermission);
            
            public static class Announcements
            {
                private const string Base = ServerLang.Base + nameof(Announcements) + ".";
                
                public static class Link
                {
                    private const string Base = Announcements.Base + nameof(Link) + ".";
                    public const string Command = Base + nameof(Command);
                    public const string Admin = Base + nameof(Admin);
                    public const string Api = Base + nameof(Api);
                    public const string GuildRejoin = Base + nameof(GuildRejoin);
                    public const string InactiveRejoin = Base + nameof(InactiveRejoin);
                }
                
                public static class Unlink
                {
                    private const string Base = Announcements.Base + nameof(Unlink) + ".";
                    
                    public const string Command = Base + nameof(Command);
                    public const string Admin = Base + nameof(Admin);
                    public const string Api = Base + nameof(Api);
                    public const string LeftGuild = Base + nameof(LeftGuild);
                    public const string Inactive = Base + nameof(Inactive);
                }
            }
            
            public static class Commands
            {
                private const string Base = ServerLang.Base + nameof(Commands) + ".";
                
                public const string DcCommand = Base + nameof(DcCommand);
                public const string CodeCommand = Base + nameof(CodeCommand);
                public const string UserCommand = Base + nameof(UserCommand);
                public const string LeaveCommand = Base + nameof(LeaveCommand);
                public const string AcceptCommand = Base + nameof(AcceptCommand);
                public const string DeclineCommand = Base + nameof(DeclineCommand);
                public const string HelpMessage = Base + nameof(HelpMessage);
                
                public static class Code
                {
                    private const string Base = Commands.Base + nameof(Code) + ".";
                    
                    public const string LinkInfo = Base + nameof(LinkInfo);
                    public const string LinkServer = Base + nameof(LinkServer);
                    public const string LinkInGuild = Base + nameof(LinkInGuild);
                    public const string LinkInDm = Base + nameof(LinkInDm);
                }
                
                public static class User
                {
                    private const string Base = Commands.Base + nameof(User) + ".";
                    
                    public static class Errors
                    {
                        private const string Base = User.Base + nameof(Errors) + ".";
                        
                        public const string InvalidSyntax = Base + nameof(InvalidSyntax);
                        public const string UserIdNotFound = Base + nameof(UserIdNotFound);
                        public const string UserNotFound = Base + nameof(UserNotFound);
                        public const string MultipleUsersFound = Base + nameof(MultipleUsersFound);
                        public const string SearchError = Base + nameof(SearchError);
                    }
                }
                
                public static class Leave
                {
                    private const string Base = Commands.Base + nameof(Leave) + ".";
                    
                    public static class Errors
                    {
                        private const string Base = Leave.Base + nameof(Errors) + ".";
                        
                        public const string NotLinked = Base + nameof(NotLinked);
                    }
                }
            }
            
            public static class Link
            {
                private const string Base = ServerLang.Base + nameof(Link) + ".";
                public static class Completed
                {
                    private const string Base = Link.Base + nameof(Completed) + ".";
                    
                    public const string Command = Base + nameof(Command);
                    public const string Admin = Base + nameof(Admin);
                    public const string Api = Base + nameof(Api);
                    public const string GuildRejoin = Base + nameof(GuildRejoin);
                    public const string InactiveRejoin = Base + nameof(InactiveRejoin);
                }
                
                public static class Declined
                {
                    private const string Base = Link.Base + nameof(Declined) + ".";
                    
                    public const string JoinWithPlayer = Base + nameof(JoinWithPlayer);
                    public const string JoinWithUser = Base + nameof(JoinWithUser);
                }
                
                public static class Errors
                {
                    private const string Base = Link.Base + nameof(Errors) + ".";
                    
                    public const string InvalidSyntax = Base + nameof(InvalidSyntax);
                }
            }
            
            public static class Unlink
            {
                private const string Base = ServerLang.Base + nameof(Unlink) + ".";
                
                public static class Completed
                {
                    private const string Base = Unlink.Base + nameof(Completed) + ".";
                    
                    public const string Command = Base + nameof(Command);
                    public const string LeftGuild = Base + nameof(LeftGuild);
                    public const string Admin = Base + nameof(Admin);
                    public const string Api = Base + nameof(Api);
                }
            }
            
            public static class Banned
            {
                private const string Base = ServerLang.Base + nameof(Banned) + ".";
                
                public const string IsUserBanned = Base + nameof(IsUserBanned);
            }
            
            public static class Join
            {
                private const string Base = ServerLang.Base + nameof(Join) + ".";
                
                public const string ByPlayer = Base + nameof(ByPlayer);
                
                public static class Errors
                {
                    private const string Base = Join.Base + nameof(Errors) + ".";
                    
                    public const string PlayerJoinActivationNotFound = Base + nameof(PlayerJoinActivationNotFound);
                }
            }
            
            public static class Errors
            {
                private const string Base = ServerLang.Base + nameof(Errors) + ".";
                
                public const string PlayerAlreadyLinked = Base + nameof(PlayerAlreadyLinked);
                public const string DiscordAlreadyLinked = Base + nameof(DiscordAlreadyLinked);
                public const string ActivationNotFound = Base + nameof(ActivationNotFound);
                public const string MustBeCompletedInDiscord = Base + nameof(MustBeCompletedInDiscord);
                public const string ConsolePlayerNotSupported = Base + nameof(ConsolePlayerNotSupported);
            }
        }
        #endregion

        #region Localization\UserAppCommandKeys.cs
        public static class UserAppCommandKeys
        {
            private const string Base = "AppCommand.User.";
            
            public const string Command = Base + nameof(Command);
            public const string Description = Base + nameof(Description);
            
            public static class Code
            {
                private const string Base = UserAppCommandKeys.Base + nameof(Code) + ".";
                
                public const string Command = Base + nameof(Command);
                public const string Description = Base + nameof(Description);
            }
            
            public static class User
            {
                private const string Base = UserAppCommandKeys.Base + nameof(User) + ".";
                
                public const string Command = Base + nameof(Command);
                public const string Description = Base + nameof(Description);
                
                public static class Args
                {
                    private const string Base = User.Base + nameof(Args) + ".";
                    
                    public static class Player
                    {
                        private const string Base = Args.Base + nameof(Player) + ".";
                        
                        public const string Name = Base + nameof(Name);
                        public const string Description = Base + nameof(Description);
                    }
                }
            }
            
            public static class Leave
            {
                private const string Base = UserAppCommandKeys.Base + nameof(Leave) + ".";
                
                public const string Command = Base + nameof(Command);
                public const string Description = Base + nameof(Description);
            }
            
            public static class Link
            {
                private const string Base = UserAppCommandKeys.Base + nameof(Link) + ".";
                
                public const string Command = Base + nameof(Command);
                public const string Description = Base + nameof(Description);
                
                public static class Args
                {
                    private const string Base = Link.Base + nameof(Args) + ".";
                    
                    public static class Code
                    {
                        private const string Base = Args.Base + nameof(Code) + ".";
                        
                        public const string Name = Base + nameof(Command);
                        public const string Description = Base + nameof(Description);
                    }
                }
            }
        }
        #endregion

        #region Templates\TemplateKeys.cs
        public static class TemplateKeys
        {
            public static class Announcements
            {
                private const string Base = nameof(Announcements) + ".";
                
                public static class Link
                {
                    private const string Base = Announcements.Base + nameof(Link) + ".";
                    public const string Command = Base + nameof(Command);
                    public const string Admin = Base + nameof(Admin);
                    public const string Api = Base + nameof(Api);
                    public const string GuildRejoin = Base + nameof(GuildRejoin);
                    public const string InactiveRejoin = Base + nameof(InactiveRejoin);
                }
                
                public static class Unlink
                {
                    private const string Base = Announcements.Base + nameof(Unlink) + ".";
                    
                    public const string Command = Base + nameof(Command);
                    public const string Admin = Base + nameof(Admin);
                    public const string Api = Base + nameof(Api);
                    public const string LeftGuild = Base + nameof(LeftGuild);
                    public const string Inactive = Base + nameof(Inactive);
                }
            }
            
            public static class WelcomeMessage
            {
                private const string Base = nameof(WelcomeMessage) + ".";
                
                public const string PmWelcomeMessage = Base + nameof(PmWelcomeMessage);
                public const string GuildWelcomeMessage = Base + nameof(GuildWelcomeMessage);
                
                public static class Error
                {
                    private const string Base = WelcomeMessage.Base + nameof(Error) + ".";
                    
                    public const string AlreadyLinked = Base + nameof(AlreadyLinked);
                }
            }
            
            public static class Commands
            {
                private const string Base = nameof(Commands) + ".";
                
                public static class Code
                {
                    private const string Base = Commands.Base + nameof(Code) + ".";
                    
                    public const string Success = Base + nameof(Success);
                }
                
                public static class User
                {
                    private const string Base = Commands.Base + nameof(User) + ".";
                    
                    public const string Success = Base + nameof(Success);
                    
                    public static class Error
                    {
                        private const string Base = User.Base + nameof(Error) + ".";
                        
                        public const string PlayerIsInvalid = Base + nameof(PlayerIsInvalid);
                        public const string PlayerNotConnected = Base + nameof(PlayerNotConnected);
                    }
                }
                
                public static class Leave
                {
                    private const string Base = Commands.Base + nameof(Leave) + ".";
                    
                    public static class Error
                    {
                        private const string Base = Leave.Base + nameof(Error) + ".";
                        
                        public const string UserNotLinked = Base + nameof(UserNotLinked);
                    }
                }
                
                public static class Admin
                {
                    private const string Base = Commands.Base + nameof(Admin) + ".";
                    
                    public static class Link
                    {
                        private const string Base = Admin.Base + nameof(Link) + ".";
                        
                        public const string Success = Base + nameof(Success);
                        
                        public static class Error
                        {
                            private const string Base = Link.Base + nameof(Error) + ".";
                            
                            public const string PlayerNotFound = Base + nameof(PlayerNotFound);
                            public const string PlayerAlreadyLinked = Base + nameof(PlayerAlreadyLinked);
                            public const string UserAlreadyLinked = Base + nameof(UserAlreadyLinked);
                        }
                    }
                    
                    public static class Unlink
                    {
                        private const string Base = Admin.Base + nameof(Unlink) + ".";
                        
                        public const string Success = Base + nameof(Success);
                        
                        public static class Error
                        {
                            private const string Base = Unlink.Base + nameof(Error) + ".";
                            
                            public const string MustSpecifyOne = Base + nameof(MustSpecifyOne);
                            public const string PlayerIsNotLinked = Base + nameof(PlayerIsNotLinked);
                            public const string UserIsNotLinked = Base + nameof(UserIsNotLinked);
                            public const string LinkNotSame = Base + nameof(LinkNotSame);
                        }
                    }
                    
                    public static class Search
                    {
                        private const string Base = Admin.Base + nameof(Search) + ".";
                        
                        public const string Success = Base + nameof(Success);
                        
                        public static class Error
                        {
                            private const string Base = Search.Base + nameof(Error) + ".";
                            
                            public const string PlayerNotFound = Base + nameof(PlayerNotFound);
                        }
                    }
                }
            }
            
            public static class Link
            {
                private const string Base = nameof(Link) + ".";
                
                public static class Completed
                {
                    private const string Base = Link.Base + nameof(Completed) + ".";
                    
                    public const string Command = Base + nameof(Command);
                    public const string Admin = Base + nameof(Admin);
                    public const string Api = Base + nameof(Api);
                    public const string GuildRejoin = Base + nameof(GuildRejoin);
                    public const string InactiveRejoin = Base + nameof(InactiveRejoin);
                }
                
                public static class Declined
                {
                    private const string Base = Link.Base + nameof(Declined) + ".";
                    
                    public const string JoinWithUser = Base + nameof(JoinWithUser);
                    public const string JoinWithPlayer = Base + nameof(JoinWithPlayer);
                }
                
                public static class WelcomeMessage
                {
                    private const string Base = Link.Base + nameof(WelcomeMessage) + ".";
                    
                    public const string DmLinkAccounts = Base + nameof(DmLinkAccounts);
                    public const string GuildLinkAccounts = Base + nameof(GuildLinkAccounts);
                }
            }
            
            public static class Unlink
            {
                private const string Base = nameof(Unlink) + ".";
                
                public static class Completed
                {
                    private const string Base = Unlink.Base + nameof(Completed) + ".";
                    public const string Command = Base + nameof(Command);
                    public const string Admin = Base + nameof(Admin);
                    public const string Api = Base + nameof(Api);
                    public const string Inactive = Base + nameof(Inactive);
                }
            }
            
            public static class Banned
            {
                private const string Base = nameof(Banned) + ".";
                
                public const string PlayerBanned = Base + nameof(PlayerBanned);
            }
            
            public static class Join
            {
                private const string Base = nameof(Join) + ".";
                
                public const string ByUsername = Base + nameof(ByUsername);
            }
            
            public static class Errors
            {
                private const string Base = nameof(Errors) + ".";
                
                public const string UserAlreadyLinked = Base + nameof(UserAlreadyLinked);
                public const string PlayerAlreadyLinked = Base + nameof(PlayerAlreadyLinked);
                public const string CodActivationNotFound = Base + nameof(CodActivationNotFound);
                public const string LookupActivationNotFound = Base + nameof(LookupActivationNotFound);
            }
        }
        #endregion

    }

}
