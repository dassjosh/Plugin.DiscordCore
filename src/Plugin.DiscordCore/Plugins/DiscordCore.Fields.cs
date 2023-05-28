﻿using System.Text;
using DiscordCorePlugin.Configuration;
using DiscordCorePlugin.Data;
using DiscordCorePlugin.Link;
using Oxide.Ext.Discord;
using Oxide.Ext.Discord.Entities.Gateway;
using Oxide.Ext.Discord.Entities.Guilds;
using Oxide.Ext.Discord.Entities.Interactions.ApplicationCommands;
using Oxide.Ext.Discord.Entities.Users;
using Oxide.Ext.Discord.Libraries.Langs;
using Oxide.Ext.Discord.Libraries.Linking;
using Oxide.Ext.Discord.Libraries.Placeholders;
using Oxide.Ext.Discord.Libraries.Templates.Commands;
using Oxide.Ext.Discord.Libraries.Templates.Messages;
using Oxide.Ext.Discord.Pooling;

namespace DiscordCorePlugin.Plugins
{
    //Define:FileOrder=5
    public partial class DiscordCore
    {
        public DiscordClient Client { get; set; }

        private PluginData _pluginData;
        private PluginConfig _pluginConfig;
        
        private DiscordUser _bot;
        
        public DiscordGuild Guild;
        
        private readonly DiscordSettings _discordSettings = new DiscordSettings
        {
            Intents = GatewayIntents.Guilds | GatewayIntents.GuildMembers
        };
        
        private readonly DiscordLink _link = GetLibrary<DiscordLink>();
        private readonly DiscordMessageTemplates _templates = GetLibrary<DiscordMessageTemplates>();
        private readonly DiscordPlaceholders _placeholders = GetLibrary<DiscordPlaceholders>();
        private readonly DiscordLang _lang = GetLibrary<DiscordLang>();
        private readonly DiscordCommandLocalizations _local = GetLibrary<DiscordCommandLocalizations>();
        private DiscordPluginPool _pool;
        private readonly StringBuilder _sb = new StringBuilder();
        

        private JoinHandler _joinHandler;
        private JoinBanHandler _banHandler;
        private LinkHandler _linkHandler;

        private const string UsePermission = "discordcore.use";
        private const string AccentColor = "de8732";
        private const string DiscordSuccess = "43b581";
        private const string DiscordDanger = "f04747";
        private const string PlayerArg = "player";
        private const string UserArg = "user";
        private const string CodeArg = "code";

        private DiscordApplicationCommand _appCommand;
        private string _allowedChannels;

        public static DiscordCore Instance;
    }
}