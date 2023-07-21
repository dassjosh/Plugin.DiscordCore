using System;
using DiscordCorePlugin.Configuration;
using Oxide.Core.Libraries.Covalence;
using Oxide.Ext.Discord.Entities;
using Oxide.Ext.Discord.Entities.Users;
using Oxide.Plugins;

namespace DiscordCorePlugin.Link
{
    public class JoinBanHandler
    {
        private readonly Hash<string, JoinBanData> _playerBans = new Hash<string, JoinBanData>();
        private readonly Hash<Snowflake, JoinBanData> _discordBans = new Hash<Snowflake, JoinBanData>();
        private readonly LinkBanSettings _settings;

        public JoinBanHandler(LinkBanSettings settings)
        {
            _settings = settings;
        }

        public void AddBan(IPlayer player)
        {
            if (!_settings.EnableLinkBanning)
            {
                return;
            }
            
            JoinBanData ban = GetBan(player);

            ban.AddDeclined();
            if (ban.Times >= _settings.BanDeclineAmount)
            {
                ban.SetBanDuration(_settings.BanDuration);
            }
        }
        
        public void AddBan(DiscordUser user)
        {
            if (!_settings.EnableLinkBanning)
            {
                return;
            }
            
            JoinBanData ban = GetBan(user);

            ban.AddDeclined();
            if (ban.Times >= _settings.BanDeclineAmount)
            {
                ban.SetBanDuration(_settings.BanDuration);
            }
        }

        public bool Unban(IPlayer player)
        {
            return _playerBans.Remove(player.Id);
        }
        
        public bool Unban(DiscordUser user)
        {
            return _discordBans.Remove(user.Id);
        }

        public bool IsBanned(IPlayer player)
        {
            if (!_settings.EnableLinkBanning)
            {
                return false;
            }
            
            JoinBanData ban = _playerBans[player.Id];
            return ban != null && ban.IsBanned();
        }
        
        public bool IsBanned(DiscordUser user)
        {
            if (!_settings.EnableLinkBanning)
            {
                return false;
            }
            
            JoinBanData ban = _discordBans[user.Id];
            return ban != null && ban.IsBanned();
        }

        public TimeSpan GetRemainingBan(IPlayer player)
        {
            if (!_settings.EnableLinkBanning)
            {
                return TimeSpan.Zero;
            }
            
            return _playerBans[player.Id]?.GetRemainingBan() ?? TimeSpan.Zero;
        }

        public TimeSpan GetRemainingDuration(DiscordUser user)
        {
            if (!_settings.EnableLinkBanning)
            {
                return TimeSpan.Zero;
            }
            
            return _discordBans[user.Id]?.GetRemainingBan() ?? TimeSpan.Zero;
        }
        
        public DateTimeOffset GetRemainingBan(DiscordUser user)
        {
            return DateTimeOffset.UtcNow + GetRemainingDuration(user);
        }

        private JoinBanData GetBan(IPlayer player)
        {
            JoinBanData ban = _playerBans[player.Id];
            if (ban == null)
            {
                ban = new JoinBanData();
                _playerBans[player.Id] = ban;
            }

            return ban;
        }
        
        private JoinBanData GetBan(DiscordUser user)
        {
            JoinBanData ban = _discordBans[user.Id];
            if (ban == null)
            {
                ban = new JoinBanData();
                _discordBans[user.Id] = ban;
            }

            return ban;
        }
    }
}