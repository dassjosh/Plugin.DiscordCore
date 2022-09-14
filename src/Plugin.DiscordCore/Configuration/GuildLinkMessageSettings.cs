using Newtonsoft.Json;
using Oxide.Ext.Discord.Entities;

namespace DiscordCorePlugin.Configuration
{
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
}