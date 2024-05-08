using DiscordCorePlugin.Enums;
using Oxide.Core.Libraries.Covalence;
using Oxide.Ext.Discord.Entities;

namespace DiscordCorePlugin.Link
{
    public class JoinData
    {
        public IPlayer Player { get; set; }
        public DiscordUser Discord { get; set; }
        public string Code { get; private set; }
        public JoinSource From { get; private set; }

        private JoinData() { }

        public static JoinData CreateServerActivation(IPlayer player, string code)
        {
            return new JoinData
            {
                From = JoinSource.Server,
                Code = code,
                Player = player
            };
        }
        
        public static JoinData CreateDiscordActivation(DiscordUser user, string code)
        {
            return new JoinData
            {
                From = JoinSource.Discord,
                Code = code,
                Discord = user
            };
        }

        public static JoinData CreateLinkedActivation(JoinSource source, IPlayer player, DiscordUser user)
        {
            return new JoinData
            {
                From = source,
                Player = player,
                Discord = user
            };
        }
        
        public bool IsCompleted() => Player != null && Discord != null && Discord.Id.IsValid();

        public bool IsMatch(IPlayer player) => Player != null && player != null && Player.Id == player.Id;

        public bool IsMatch(DiscordUser user) => Discord != null && user != null && Discord.Id == user.Id;
    }
}