﻿using DiscordCorePlugin.Enums;
using Oxide.Core.Libraries.Covalence;
using Oxide.Ext.Discord.Entities;

namespace DiscordCorePlugin.Link
{
    public class JoinData
    {
        public IPlayer Player { get; set; }
        public DiscordUser Discord { get; set; }
        public string Code { get; set; }
        public JoinSource From { get; }

        public JoinData(JoinSource from)
        {
            From = from;
        }
        
        public bool IsCompleted() => Player != null && Discord != null && Discord.Id.IsValid();

        public bool IsMatch(IPlayer player) => Player != null && player != null && Player.Id == player.Id;

        public bool IsMatch(DiscordUser user) => Discord != null && user != null && Discord.Id == user.Id;
    }
}