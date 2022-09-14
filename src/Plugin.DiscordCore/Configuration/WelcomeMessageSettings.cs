using System.Collections.Generic;
using Newtonsoft.Json;
using Oxide.Ext.Discord.Entities;

namespace DiscordCorePlugin.Configuration
{
    public class WelcomeMessageSettings
    {
        [JsonProperty(PropertyName = "Enable Welcome DM Message")]
        public bool EnableWelcomeMessage { get; set; }
        
        [JsonProperty(PropertyName = "Send Welcome Message On Discord Server Join")]
        public bool SendOnGuildJoin { get; set; }
        
        [JsonProperty(PropertyName = "Send Welcome Message On Role ID Added")]
        public List<Snowflake> SendOnRoleAdded { get; set; }
            
        [JsonProperty(PropertyName = "Add Link Accounts Button In Welcome Message")]
        public bool EnableLinkButton { get; set; }

        public WelcomeMessageSettings(WelcomeMessageSettings settings)
        {
            EnableWelcomeMessage = settings?.EnableWelcomeMessage ?? true;
            SendOnGuildJoin = settings?.SendOnGuildJoin ?? false;
            SendOnRoleAdded = settings?.SendOnRoleAdded ?? new List<Snowflake> {new Snowflake(1234567890)};
            EnableLinkButton = settings?.EnableLinkButton ?? true;
        }
    }
}