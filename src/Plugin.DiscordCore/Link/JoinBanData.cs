using System;

namespace DiscordCorePlugin.Link
{
    public class JoinBanData
    {
        public int Times { get; private set; }
        private DateTime _bannedUntil;

        public void AddDeclined()
        {
            Times++;
        }

        public bool IsBanned()
        {
            return _bannedUntil > DateTime.UtcNow;
        }

        public TimeSpan GetRemainingBan()
        {
            return _bannedUntil - DateTime.UtcNow;
        }

        public void SetBanDuration(float hours)
        {
            _bannedUntil = DateTime.UtcNow.AddHours(hours);
        }
    }
}