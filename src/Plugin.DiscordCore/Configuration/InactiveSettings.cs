using Newtonsoft.Json;

namespace DiscordCorePlugin.Configuration
{
    public class InactiveSettings
    {
        [JsonProperty(PropertyName = "Automatically Unlink Inactive Players")]
        public bool UnlinkInactive { get; set; }
        
        [JsonProperty(PropertyName = "Player Considered Inactive After X (Days)")]
        public float UnlinkInactiveDays { get; set; }
        
        [JsonProperty(PropertyName = "Automatically Relink Inactive Players On Game Server Join")]
        public bool AutoRelinkInactive { get; set; }

        public InactiveSettings(InactiveSettings settings)
        {
            UnlinkInactive = settings?.UnlinkInactive ?? false;
            UnlinkInactiveDays = settings?.UnlinkInactiveDays ?? 90;
            AutoRelinkInactive = settings?.AutoRelinkInactive ?? true;
        }
    }
}