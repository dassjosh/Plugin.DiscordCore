using Oxide.Ext.Discord.Entities;
using Oxide.Plugins;

namespace DiscordCorePlugin.Data
{
    public class PluginData
    {
        public Hash<string, DiscordInfo> PlayerDiscordInfo = new Hash<string, DiscordInfo>();
        public Hash<Snowflake, DiscordInfo> LeftPlayerInfo = new Hash<Snowflake, DiscordInfo>();
        public Hash<string, DiscordInfo> InactivePlayerInfo = new Hash<string, DiscordInfo>();
        public LinkMessageData MessageData;
    }
}