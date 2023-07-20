using System.Collections.Generic;
using DiscordCorePlugin.Data;
using Oxide.Ext.Discord.Entities;
using Oxide.Ext.Discord.Libraries.Linking;
using Oxide.Plugins;

namespace DiscordCorePlugin.Plugins
{
    //Define:FileOrder=60
    public partial class DiscordCore
    {
        public IDictionary<PlayerId, Snowflake> GetPlayerIdToDiscordIds()
        {
            Hash<PlayerId, Snowflake> data = new Hash<PlayerId, Snowflake>();
            foreach (DiscordInfo info in _pluginData.PlayerDiscordInfo.Values)
            {
                data[new PlayerId(info.PlayerId)] = info.DiscordId;
            }

            return data;
        }
    }
}