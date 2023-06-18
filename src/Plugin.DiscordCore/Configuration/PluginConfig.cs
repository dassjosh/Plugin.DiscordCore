using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Oxide.Ext.Discord.Entities;
using Oxide.Ext.Discord.Logging;

namespace DiscordCorePlugin.Configuration
{
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
        public GuildMessageSettings LinkMessageSettings { get; set; }
        
        [JsonProperty(PropertyName = "Link Permission Settings")]
        public LinkPermissionSettings PermissionSettings { get; set; }

        [JsonProperty(PropertyName = "Link Ban Settings")]
        public LinkBanSettings LinkBanSettings { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [DefaultValue(DiscordLogLevel.Info)]
        [JsonProperty(PropertyName = "Discord Extension Log Level (Verbose, Debug, Info, Warning, Error, Exception, Off)")]
        public DiscordLogLevel ExtensionDebugging { get; set; }
    }
}