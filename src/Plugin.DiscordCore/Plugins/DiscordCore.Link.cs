using System.Collections.Generic;
using DiscordCorePlugin.Data;
using Oxide.Ext.Discord.Entities;
using Oxide.Plugins;

namespace DiscordCorePlugin.Plugins
{
    //Define:FileOrder=60
    public partial class DiscordCore
    {
        public IDictionary<string, Snowflake> GetSteamToDiscordIds()
        {
            Hash<string, Snowflake> data = new Hash<string, Snowflake>();
            foreach (DiscordInfo info in _pluginData.PlayerDiscordInfo.Values)
            {
                data[info.PlayerId] = info.DiscordId;
            }

            return data;
        }
    }
}