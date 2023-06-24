using System.Text;
using DiscordCorePlugin.Configuration;
using DiscordCorePlugin.Data;
using DiscordCorePlugin.Link;
using Oxide.Ext.Discord.Attributes.Pooling;
using Oxide.Ext.Discord.Builders.Interactions.AutoComplete;
using Oxide.Ext.Discord.Clients;
using Oxide.Ext.Discord.Connections;
using Oxide.Ext.Discord.Entities.Gateway;
using Oxide.Ext.Discord.Entities.Guilds;
using Oxide.Ext.Discord.Entities.Interactions.ApplicationCommands;
using Oxide.Ext.Discord.Entities.Users;
using Oxide.Ext.Discord.Libraries.Linking;
using Oxide.Ext.Discord.Libraries.Locale;
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
        
        private readonly BotConnection _discordSettings = new BotConnection
        {
            Intents = GatewayIntents.Guilds | GatewayIntents.GuildMembers
        };
        
        [DiscordPool]
        private DiscordPluginPool _pool;
        
        private readonly DiscordLink _link = GetLibrary<DiscordLink>();
        private readonly DiscordMessageTemplates _templates = GetLibrary<DiscordMessageTemplates>();
        private readonly DiscordPlaceholders _placeholders = GetLibrary<DiscordPlaceholders>();
        private readonly DiscordLocales _lang = GetLibrary<DiscordLocales>();
        private readonly DiscordCommandLocalizations _local = GetLibrary<DiscordCommandLocalizations>();
        private readonly StringBuilder _sb = new StringBuilder();

        private JoinHandler _joinHandler;
        private JoinBanHandler _banHandler;
        private LinkHandler _linkHandler;

        private const string UsePermission = "discordcore.use";
        private const string AccentColor = "de8732";
        private const string Success = "43b581";
        private const string Danger = "f04747";
        private const string PlayerArg = "player";
        private const string UserArg = "user";
        private const string CodeArg = "code";

        private DiscordApplicationCommand _appCommand;
        private string _allowedChannels;

        public static DiscordCore Instance;
    }
}