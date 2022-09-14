using Newtonsoft.Json;
using Oxide.Ext.Discord.Entities;

namespace DiscordCorePlugin.Data
{
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
}