﻿using DiscordCorePlugin.Configuration;
using DiscordCorePlugin.Data;
using DiscordCorePlugin.Templates;
using Oxide.Ext.Discord.Entities.Channels;

namespace DiscordCorePlugin.Plugins
{
    public partial class DiscordCore
    {
        //Define:FileOrder=25
        public void SetupGuildWelcomeMessage()
        {
            GuildLinkMessageSettings settings = _pluginConfig.LinkMessageSettings;
            if (!settings.Enabled)
            {
                return;
            }
            
            if (!settings.ChannelId.IsValid())
            {
                PrintWarning("Link message is enabled but link message channel ID is not valid");
                return;
            }

            DiscordChannel channel = Guild.Channels[settings.ChannelId];
            if (channel == null)
            {
                PrintWarning($"Link message failed to find channel with ID {settings.ChannelId}");
                return;
            }

            if (_pluginData.MessageData == null)
            {
                CreateGuildWelcomeMessage(settings);
                return;
            }

            channel.GetChannelMessage(Client, _pluginData.MessageData.MessageId, message =>
                {
                    UpdateGuildTemplateMessage(TemplateKeys.WelcomeMessage.GuildWelcomeMessage, message);
                },
                error =>
                {
                    if (error.HttpStatusCode == 404)
                    {
                        error.SuppressErrorMessage();
                        PrintWarning("The previous link message has been removed. Recreating the message.");
                        CreateGuildWelcomeMessage(settings);
                    }
                });
        }

        private void CreateGuildWelcomeMessage(GuildLinkMessageSettings settings)
        {
            SendGlobalTemplateMessage(TemplateKeys.WelcomeMessage.GuildWelcomeMessage, settings.ChannelId, null, null, null, message =>
            {
                _pluginData.MessageData = new LinkMessageData(message.ChannelId, message.Id);
            });
        }
    }
}