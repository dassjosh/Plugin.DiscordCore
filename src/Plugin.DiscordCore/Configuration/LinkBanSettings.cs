using Newtonsoft.Json;
using Oxide.Ext.Discord.Entities;

namespace DiscordCorePlugin.Configuration
{
    public class LinkBanSettings
    {
        [JsonProperty(PropertyName = "Enable Link Ban")]
        public bool EnableLinkBanning { get; set; }
        
        [JsonProperty(PropertyName = "Ban Announcement Channel ID")]
        public Snowflake BanAnnouncementChannel { get; set; }
            
        [JsonProperty(PropertyName = "Ban Link After X Join Declines")]
        public int BanDeclineAmount { get; set; }
            
        [JsonProperty(PropertyName = "Ban Duration (Hours)")]
        public int BanDuration { get; set; }

        public LinkBanSettings(LinkBanSettings settings)
        {
            EnableLinkBanning = settings?.EnableLinkBanning ?? true;
            BanAnnouncementChannel = settings?.BanAnnouncementChannel ?? default(Snowflake);
            BanDeclineAmount = settings?.BanDeclineAmount ?? 3;
            BanDuration = settings?.BanDuration ?? 24;
        }
    }
}