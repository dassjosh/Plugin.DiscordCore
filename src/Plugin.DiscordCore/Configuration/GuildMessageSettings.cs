using Newtonsoft.Json;
using Oxide.Ext.Discord.Entities;

namespace DiscordCorePlugin.Configuration
{
    public class GuildMessageSettings
    {
        [JsonProperty(PropertyName = "Enable Guild Link Message")]
        public bool Enabled { get; set; }
            
        [JsonProperty(PropertyName = "Message Channel ID")]
        public Snowflake ChannelId { get; set; }

        public GuildMessageSettings(GuildMessageSettings settings)
        {
            Enabled = settings?.Enabled ?? false;
            ChannelId = settings?.ChannelId ?? default(Snowflake);
        }
    }
}