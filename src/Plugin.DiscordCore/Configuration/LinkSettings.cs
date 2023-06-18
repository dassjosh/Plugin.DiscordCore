using Newtonsoft.Json;
using Oxide.Ext.Discord.Entities;

namespace DiscordCorePlugin.Configuration
{
    public class LinkSettings
    {
        [JsonProperty(PropertyName = "Announcement Channel Id")]
        public Snowflake AnnouncementChannel { get; set; }
        
        [JsonProperty(PropertyName = "Link Code Generator Characters")]
        public string LinkCodeCharacters { get; set; }

        [JsonProperty(PropertyName = "Link Code Generator Length")]
        public int LinkCodeLength { get; set; }

        [JsonProperty(PropertyName = "Automatically Relink A Player If They Leave And Rejoin The Discord Server")]
        public bool AutoRelinkPlayer { get; set; }
        
        [JsonProperty(PropertyName = "Inactive Settings")]
        public InactiveSettings InactiveSettings { get; set; }

        public LinkSettings(LinkSettings settings)
        {
            AnnouncementChannel = settings?.AnnouncementChannel ?? default(Snowflake);
            LinkCodeCharacters = settings?.LinkCodeCharacters ?? "123456789";
            LinkCodeLength = settings?.LinkCodeLength ?? 6;
            if (LinkCodeLength <= 0)
            {
                LinkCodeLength = 6;
            }
            AutoRelinkPlayer = settings?.AutoRelinkPlayer ?? true;
            InactiveSettings = new InactiveSettings(settings?.InactiveSettings);
        }
    }
}