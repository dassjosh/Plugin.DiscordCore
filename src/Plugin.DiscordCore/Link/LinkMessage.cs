using DiscordCorePlugin.Configuration;
using DiscordCorePlugin.Plugins;
using Oxide.Core.Libraries.Covalence;
using Oxide.Ext.Discord.Entities.Interactions;
using Oxide.Ext.Discord.Entities.Users;
using Oxide.Ext.Discord.Libraries.Placeholders;

namespace DiscordCorePlugin.Link
{
    public class LinkMessage
    {
        private readonly string _chatLang;
        private readonly string _chatAnnouncement;
        private readonly string _discordTemplate;
        private readonly string _announcementTemplate;
        private readonly DiscordCore _plugin;
        private readonly LinkSettings _link;

        public LinkMessage(string chatLang, string chatAnnouncement, string discordTemplate, string announcementTemplate, DiscordCore plugin, LinkSettings link)
        {
            _chatLang = chatLang;
            _chatAnnouncement = chatAnnouncement;
            _discordTemplate = discordTemplate;
            _announcementTemplate = announcementTemplate;
            _plugin = plugin;
            _link = link;
        }

        public void SendMessages(IPlayer player, DiscordUser user, DiscordInteraction interaction, PlaceholderData data)
        {
            using (data)
            {
                data.ManualPool();
                _plugin.BroadcastMessage(_chatAnnouncement, data);
                _plugin.Chat(player, _chatLang, data);
                if (!string.IsNullOrEmpty(_discordTemplate))
                {
                    if (interaction != null)
                    {
                        _plugin.SendTemplateMessage(_discordTemplate, interaction, data);
                    }
                    else
                    {
                        _plugin.SendTemplateMessage(_discordTemplate, user, player, data);
                    }
                }
            
                _plugin.SendGlobalTemplateMessage(_announcementTemplate, _link.AnnouncementChannel, user, player, data);
            }
        }
    }
}