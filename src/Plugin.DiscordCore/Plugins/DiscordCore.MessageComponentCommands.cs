using DiscordCorePlugin.Link;
using DiscordCorePlugin.Placeholders;
using DiscordCorePlugin.Templates;
using Oxide.Ext.Discord.Attributes;
using Oxide.Ext.Discord.Entities;
using Oxide.Ext.Discord.Extensions;

namespace DiscordCorePlugin.Plugins
{
    //Define:FileOrder=40
    public partial class DiscordCore
    {
        private const string WelcomeMessageLinkAccountsButtonId = nameof(DiscordCore) + "_PmLinkAccounts";
        private const string GuildWelcomeMessageLinkAccountsButtonId = nameof(DiscordCore) + "_GuildLinkAccounts";
        private const string AcceptLinkButtonId = nameof(DiscordCore) + "_AcceptLink";
        private const string DeclineLinkButtonId = nameof(DiscordCore) + "_DeclineLink";

        // ReSharper disable once UnusedMember.Local
        [DiscordMessageComponentCommand(WelcomeMessageLinkAccountsButtonId)]
        private void HandleWelcomeMessageLinkAccounts(DiscordInteraction interaction)
        {
            DiscordUser user = interaction.User;
            if (user.IsLinked())
            {
                SendTemplateMessage(TemplateKeys.Errors.UserAlreadyLinked,interaction, GetDefault(user.Player, user));
                return;
            }

            JoinData join = _joinHandler.CreateActivation(user);
            SendTemplateMessage(TemplateKeys.Link.WelcomeMessage.DmLinkAccounts, interaction, GetDefault(user).Add(PlaceholderDataKeys.Code, join.Code));
        }
        
        // ReSharper disable once UnusedMember.Local
        [DiscordMessageComponentCommand(GuildWelcomeMessageLinkAccountsButtonId)]
        private void HandleGuildWelcomeMessageLinkAccounts(DiscordInteraction interaction)
        {
            DiscordUser user = interaction.User;
            if (user.IsLinked())
            {
                SendTemplateMessage(TemplateKeys.Errors.UserAlreadyLinked, interaction, GetDefault(user.Player, user));
                return;
            }
            
            JoinData join = _joinHandler.CreateActivation(user);
            SendTemplateMessage(TemplateKeys.Link.WelcomeMessage.GuildLinkAccounts, interaction, GetDefault(user).Add(PlaceholderDataKeys.Code, join.Code));
        }

        // ReSharper disable once UnusedMember.Local
        [DiscordMessageComponentCommand(AcceptLinkButtonId)]
        private void HandleAcceptLinkButton(DiscordInteraction interaction)
        {
            DiscordUser user = interaction.User;
            if (user.IsLinked())
            {
                SendTemplateMessage(TemplateKeys.Errors.UserAlreadyLinked, interaction, GetDefault(user.Player, user));
                return;
            }

            JoinData join = _joinHandler.FindCompletedByUser(user);
            if (join == null)
            {
                SendTemplateMessage(TemplateKeys.Errors.LookupActivationNotFound, interaction, GetDefault(user));
                return;
            }
            
            _joinHandler.CompleteLink(join, interaction);
        }
        
        // ReSharper disable once UnusedMember.Local
        [DiscordMessageComponentCommand(DeclineLinkButtonId)]
        private void HandleDeclineLinkButton(DiscordInteraction interaction)
        {
            DiscordUser user = interaction.User;
            if (user.IsLinked())
            {
                SendTemplateMessage(TemplateKeys.Errors.UserAlreadyLinked, interaction, GetDefault(user.Player, user));
                return;
            }
            
            JoinData join = _joinHandler.FindCompletedByUser(user);
            if (join == null)
            {
                SendTemplateMessage(TemplateKeys.Errors.LookupActivationNotFound, interaction, GetDefault(user));
                return;
            }
            
            _joinHandler.DeclineLink(join, interaction);
        }
    }
}