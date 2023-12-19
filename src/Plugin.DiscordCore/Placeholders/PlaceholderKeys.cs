using DiscordCorePlugin.Plugins;
using Oxide.Ext.Discord.Libraries;

namespace DiscordCorePlugin.Placeholders
{
    public class PlaceholderKeys
    {
        public static readonly PlaceholderKey InviteUrl = new PlaceholderKey(nameof(DiscordCore), "invite.url");
        public static readonly PlaceholderKey LinkCode = new PlaceholderKey(nameof(DiscordCore), "link.code");
        public static readonly PlaceholderKey CommandChannels = new PlaceholderKey(nameof(DiscordCore), "command.channels");
        public static readonly PlaceholderKey NotFound = new PlaceholderKey(nameof(DiscordCore), "notfound");
    }
}