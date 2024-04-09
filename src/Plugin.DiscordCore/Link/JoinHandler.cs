using System;
using System.Collections.Generic;
using System.Text;
using DiscordCorePlugin.Configuration;
using DiscordCorePlugin.Enums;
using DiscordCorePlugin.Localization;
using DiscordCorePlugin.Plugins;
using DiscordCorePlugin.Templates;
using Oxide.Core.Libraries.Covalence;
using Oxide.Ext.Discord.Entities;
using Oxide.Ext.Discord.Libraries;

namespace DiscordCorePlugin.Link
{
    public class JoinHandler
    {
        private readonly List<JoinData> _activations = new();
        private readonly LinkSettings _settings;
        private readonly LinkHandler _linkHandler;
        private readonly JoinBanHandler _ban;
        private readonly DiscordCore _plugin = DiscordCore.Instance;
        private readonly StringBuilder _sb = new();

        public JoinHandler(LinkSettings settings, LinkHandler linkHandler, JoinBanHandler ban)
        {
            _settings = settings;
            _linkHandler = linkHandler;
            _ban = ban;
        }
        
        public JoinData FindByCode(string code)
        {
            for (int index = 0; index < _activations.Count; index++)
            {
                JoinData activation = _activations[index];
                if (activation.Code?.Equals(code, StringComparison.OrdinalIgnoreCase) ?? false)
                {
                    return activation;
                }
            }

            return null;
        }

        public JoinData FindCompletedByPlayer(IPlayer player)
        {
            for (int index = 0; index < _activations.Count; index++)
            {
                JoinData activation = _activations[index];
                if (activation.IsCompleted() && activation.Player.Id.Equals(player.Id, StringComparison.OrdinalIgnoreCase))
                {
                    return activation;
                }
            }

            return null;
        }
        
        public JoinData FindCompletedByUser(DiscordUser user)
        {
            for (int index = 0; index < _activations.Count; index++)
            {
                JoinData activation = _activations[index];
                if (activation.IsCompleted() && activation.Discord.Id == user.Id)
                {
                    return activation;
                }
            }

            return null;
        }

        public void RemoveByPlayer(IPlayer player)
        {
            for (int index = _activations.Count - 1; index >= 0; index--)
            {
                JoinData activation = _activations[index];
                if (activation.IsMatch(player))
                {
                    _activations.RemoveAt(index);
                }
            }
        }
        
        public void RemoveByUser(DiscordUser user)
        {
            for (int index = _activations.Count - 1; index >= 0; index--)
            {
                JoinData activation = _activations[index];
                if (activation.IsMatch(user))
                {
                    _activations.RemoveAt(index);
                }
            }
        }

        public JoinData CreateActivation(IPlayer player)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            
            RemoveByPlayer(player);
            JoinData activation = new(JoinSource.Server)
            {
                Code = GenerateCode(),
                Player = player
            };
            _activations.Add(activation);
            return activation;
        }
        
        public JoinData CreateActivation(DiscordUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            
            RemoveByUser(user);
            JoinData activation = new(JoinSource.Discord)
            {
                Code = GenerateCode(),
                Discord = user
            };
            _activations.Add(activation);
            return activation;
        }
        
        public JoinData CreateActivation(IPlayer player, DiscordUser user, JoinSource from)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            
            RemoveByPlayer(player);
            RemoveByUser(user);
            JoinData activation = new(from)
            {
                Discord = user,
                Player = player
            };
            _activations.Add(activation);
            return activation;
        }
        
        private string GenerateCode()
        {
            _sb.Clear();
            for (int i = 0; i < _settings.LinkCodeLength; i++)
            {
                _sb.Append(_settings.LinkCodeCharacters[Oxide.Core.Random.Range(0, _settings.LinkCodeCharacters.Length)]);
            }

            return _sb.ToString();
        }

        public void CompleteLink(JoinData data, DiscordInteraction interaction)
        {
            IPlayer player = data.Player;
            DiscordUser user = data.Discord;

            _activations.Remove(data);
            RemoveByPlayer(data.Player);
            RemoveByUser(data.Discord);
            
            _linkHandler.HandleLink(player, user, LinkReason.Command, interaction);
        }

        public void DeclineLink(JoinData data, DiscordInteraction interaction)
        {
            _activations.Remove(data);

            if (data.From == JoinSource.Server)
            {
                _ban.AddBan(data.Player);
                RemoveByPlayer(data.Player);
                using (PlaceholderData placeholders = _plugin.GetDefault(data.Player, data.Discord))
                {
                    placeholders.ManualPool();
                    _plugin.Chat(data.Player, ServerLang.Link.Declined.JoinWithUser, placeholders);
                    _plugin.SendTemplateMessage(TemplateKeys.Link.Declined.JoinWithUser, interaction, placeholders);
                }
            }
            else if (data.From == JoinSource.Discord)
            {
                _ban.AddBan(data.Discord);
                RemoveByUser(data.Discord);
                using (PlaceholderData placeholders = _plugin.GetDefault(data.Player, data.Discord))
                {
                    placeholders.ManualPool();
                    _plugin.Chat(data.Player, ServerLang.Link.Declined.JoinWithPlayer, placeholders);
                    _plugin.SendTemplateMessage(TemplateKeys.Link.Declined.JoinWithPlayer, data.Discord, data.Player, placeholders);
                }
            }
        }
    }
}