using DiscordCorePlugin.Enums;
using Oxide.Core.Libraries.Covalence;
using Oxide.Ext.Discord.Entities.Users;

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
        
        public bool IsCompleted()
        {
            return Player != null && Discord != null && Discord.Id.IsValid();
        }

        public bool IsMatch(IPlayer player)
        {
            return Player != null && player != null && Player.Id == player.Id;
        }
        
        public bool IsMatch(DiscordUser user)
        {
            return Discord != null && user != null && Discord.Id == user.Id;
        }
    }
}