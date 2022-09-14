using DiscordCorePlugin.Link;
using DiscordCorePlugin.Templates;
using Oxide.Ext.Discord.Attributes.ApplicationCommands;
using Oxide.Ext.Discord.Entities.Interactions;
using Oxide.Ext.Discord.Entities.Users;
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
            SendTemplateMessage(TemplateKeys.Link.WelcomeMessage.DmLinkAccounts, interaction, GetDefault(user).Add(CodeKey, join.Code));
        }
        
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
            SendTemplateMessage(TemplateKeys.Link.WelcomeMessage.GuildLinkAccounts, interaction, GetDefault(user).Add(CodeKey, join.Code));
        }

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