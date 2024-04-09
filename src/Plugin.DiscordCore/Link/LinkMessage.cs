using DiscordCorePlugin.Configuration;
using DiscordCorePlugin.Plugins;
using Oxide.Core.Libraries.Covalence;
using Oxide.Ext.Discord.Entities;
using Oxide.Ext.Discord.Libraries;

namespace DiscordCorePlugin.Link
{
    public class LinkMessage
    {
        private readonly string _chatLang;
        private readonly string _chatAnnouncement;
        private readonly TemplateKey _discordTemplate;
        private readonly TemplateKey _announcementTemplate;
        private readonly DiscordCore _plugin;
        private readonly LinkSettings _link;

        public LinkMessage(string chatLang, string chatAnnouncement, TemplateKey discordTemplate, TemplateKey announcementTemplate, DiscordCore plugin, LinkSettings link)
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
                if (_discordTemplate.IsValid)
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