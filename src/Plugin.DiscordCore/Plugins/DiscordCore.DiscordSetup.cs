using DiscordCorePlugin.Configuration;
using DiscordCorePlugin.Data;
using DiscordCorePlugin.Templates;
using Oxide.Ext.Discord.Entities.Api;
using Oxide.Ext.Discord.Entities.Channels;

namespace DiscordCorePlugin.Plugins
{
    public partial class DiscordCore
    {
        //Define:FileOrder=25
        public void SetupGuildWelcomeMessage()
        {
            GuildMessageSettings settings = _pluginConfig.LinkMessageSettings;
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

            channel.GetMessage(Client, _pluginData.MessageData.MessageId).Then(message =>
                {
                    UpdateGuildTemplateMessage(TemplateKeys.WelcomeMessage.GuildWelcomeMessage, message);
                }).Catch<ResponseError>(error =>
                {
                    if (error.HttpStatusCode == DiscordHttpStatusCode.NotFound)
                    {
                        error.SuppressErrorMessage();
                        PrintWarning("The previous link message has been removed. Recreating the message.");
                        CreateGuildWelcomeMessage(settings);
                    }
                });
        }

        private void CreateGuildWelcomeMessage(GuildMessageSettings settings)
        {
            SendGlobalTemplateMessage(TemplateKeys.WelcomeMessage.GuildWelcomeMessage, settings.ChannelId).Then(message =>
            {
                _pluginData.MessageData = new LinkMessageData(message.ChannelId, message.Id);
            });
        }
    }
}