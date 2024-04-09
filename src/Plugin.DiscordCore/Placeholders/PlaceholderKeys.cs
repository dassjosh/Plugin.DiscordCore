using DiscordCorePlugin.Plugins;
using Oxide.Ext.Discord.Libraries;

namespace DiscordCorePlugin.Placeholders
{
    public class PlaceholderKeys
    {
        public static readonly PlaceholderKey InviteUrl = new(nameof(DiscordCore), "invite.url");
        public static readonly PlaceholderKey LinkCode = new(nameof(DiscordCore), "link.code");
        public static readonly PlaceholderKey CommandChannels = new(nameof(DiscordCore), "command.channels");
        public static readonly PlaceholderKey NotFound = new(nameof(DiscordCore), "notfound");
    }
}