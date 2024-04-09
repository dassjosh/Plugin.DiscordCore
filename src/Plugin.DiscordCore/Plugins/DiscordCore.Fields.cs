using System.Text;
using DiscordCorePlugin.Configuration;
using DiscordCorePlugin.Data;
using DiscordCorePlugin.Link;
using Oxide.Ext.Discord.Clients;
using Oxide.Ext.Discord.Connections;
using Oxide.Ext.Discord.Entities;
using Oxide.Ext.Discord.Libraries;

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
        
        private readonly BotConnection _discordSettings = new()
        {
            Intents = GatewayIntents.Guilds | GatewayIntents.GuildMembers
        };

        private readonly DiscordLink _link = GetLibrary<DiscordLink>();
        private readonly DiscordMessageTemplates _templates = GetLibrary<DiscordMessageTemplates>();
        private readonly DiscordPlaceholders _placeholders = GetLibrary<DiscordPlaceholders>();
        private readonly DiscordLocales _lang = GetLibrary<DiscordLocales>();
        private readonly DiscordCommandLocalizations _local = GetLibrary<DiscordCommandLocalizations>();
        private readonly StringBuilder _sb = new();

        private JoinHandler _joinHandler;
        private JoinBanHandler _banHandler;
        private LinkHandler _linkHandler;

        private const string UsePermission = "discordcore.use";
        private static readonly DiscordColor AccentColor = new("de8732");
        private static readonly DiscordColor Success = new("43b581");
        private static readonly DiscordColor Danger = new("f04747");
        private const string PlayerArg = "player";
        private const string UserArg = "user";
        private const string CodeArg = "code";

        private DiscordApplicationCommand _appCommand;
        private string _allowedChannels;

        public static DiscordCore Instance;
    }
}