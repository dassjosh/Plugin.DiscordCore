using DiscordCorePlugin.Plugins;
using Oxide.Ext.Discord.Libraries.Placeholders;

namespace DiscordCorePlugin.Placeholders
{
    public class PlaceholderKeys
    {
        public static readonly PlaceholderKey InviteCode = new PlaceholderKey(nameof(DiscordCore), "invite.code");
        public static readonly PlaceholderKey ServerLinkArg = new PlaceholderKey(nameof(DiscordCore), "server.link.arg");
        public static readonly PlaceholderKey LinkCode = new PlaceholderKey(nameof(DiscordCore), "link.code");
        public static readonly PlaceholderKey CommandChannels = new PlaceholderKey(nameof(DiscordCore), "command.channels");
        public static readonly PlaceholderKey NotFound = new PlaceholderKey(nameof(DiscordCore), "notfound");
    }
}