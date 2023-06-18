using System.Collections.Generic;
using Newtonsoft.Json;
using Oxide.Ext.Discord.Entities;

namespace DiscordCorePlugin.Configuration
{
    public class LinkPermissionSettings
    {
        [JsonProperty(PropertyName = "On Link Server Permissions To Add")]
        public List<string> LinkPermissions { get; set; }
        
        [JsonProperty(PropertyName = "On Unlink Server Permissions To Remove")]
        public List<string> UnlinkPermissions { get; set; }
        
        [JsonProperty(PropertyName = "On Link Server Groups To Add")]
        public List<string> LinkGroups { get; set; }
        
        [JsonProperty(PropertyName = "On Unlink Server Groups To Remove")]
        public List<string> UnlinkGroups { get; set; }
        
        [JsonProperty(PropertyName = "On Link Discord Roles To Add")]
        public List<Snowflake> LinkRoles { get; set; }
        
        [JsonProperty(PropertyName = "On Unlink Discord Roles To Remove")]
        public List<Snowflake> UnlinkRoles { get; set; }

        public LinkPermissionSettings(LinkPermissionSettings settings)
        {
            LinkPermissions = settings?.LinkPermissions ?? new List<string>();
            LinkGroups = settings?.LinkGroups ?? new List<string>();
            LinkRoles = settings?.LinkRoles ?? new List<Snowflake>();
            UnlinkPermissions = settings?.UnlinkPermissions ?? new List<string>();
            UnlinkGroups = settings?.UnlinkGroups ?? new List<string>();
            UnlinkRoles = settings?.UnlinkRoles ?? new List<Snowflake>();
        }
    }
}