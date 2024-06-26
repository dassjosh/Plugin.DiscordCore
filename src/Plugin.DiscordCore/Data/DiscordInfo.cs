﻿using System;
using Newtonsoft.Json;
using Oxide.Core.Libraries.Covalence;
using Oxide.Ext.Discord.Entities;

namespace DiscordCorePlugin.Data
{
    public class DiscordInfo
    {
        public Snowflake DiscordId { get; set; }
        public string PlayerId { get; set; }
        public DateTime LastOnline { get; set; } = DateTime.UtcNow;

        [JsonConstructor]
        public DiscordInfo() { }

        public DiscordInfo(IPlayer player, DiscordUser user)
        {
            PlayerId = player.Id;
            DiscordId = user.Id;
        }
    }
}