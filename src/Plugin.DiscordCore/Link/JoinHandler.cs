using System;
using System.Collections.Generic;
using System.Text;
using DiscordCorePlugin.Configuration;
using DiscordCorePlugin.Enums;
using DiscordCorePlugin.Localization;
using DiscordCorePlugin.Plugins;
using DiscordCorePlugin.Templates;
using Oxide.Core.Libraries.Covalence;
using Oxide.Ext.Discord.Entities.Interactions;
using Oxide.Ext.Discord.Entities.Users;
using Oxide.Ext.Discord.Pooling;

namespace DiscordCorePlugin.Link
{
    public class JoinHandler
    {
        private readonly List<JoinData> _activations = new List<JoinData>();
        private readonly LinkSettings _settings;
        private readonly LinkHandler _linkHandler;
        private readonly JoinBanHandler _ban;
        private readonly DiscordPluginPool _pool;
        private readonly DiscordCore _plugin = DiscordCore.Instance;

        public JoinHandler(LinkSettings settings, LinkHandler linkHandler, JoinBanHandler ban, DiscordPluginPool pool)
        {
            _settings = settings;
            _linkHandler = linkHandler;
            _ban = ban;
            _pool = pool;
        }
        
        public JoinData FindByCode(string code)
        {
            for (int index = 0; index < _activations.Count; index++)
            {
                JoinData activation = _activations[index];
                if (activation.Code.Equals(code, StringComparison.OrdinalIgnoreCase))
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
            JoinData activation = new JoinData(JoinedFrom.Server)
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
            JoinData activation = new JoinData(JoinedFrom.Discord)
            {
                Code = GenerateCode(),
                Discord = user
            };
            _activations.Add(activation);
            return activation;
        }
        
        public JoinData CreateActivation(IPlayer player, DiscordUser user, JoinedFrom from)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            
            RemoveByPlayer(player);
            RemoveByUser(user);
            JoinData activation = new JoinData(from)
            {
                Discord = user,
                Player = player
            };
            _activations.Add(activation);
            return activation;
        }
        
        private string GenerateCode()
        {
            StringBuilder sb = _pool.GetStringBuilder();
            for (int i = 0; i < _settings.LinkCodeLength; i++)
            {
                sb.Append(_settings.LinkCodeCharacters[Oxide.Core.Random.Range(0, _settings.LinkCodeCharacters.Length)]);
            }

            string code = sb.ToString();
            _pool.FreeStringBuilder(sb);
            return code;
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

            if (data.From == JoinedFrom.Server)
            {
                _ban.AddBan(data.Player);
                RemoveByPlayer(data.Player);
                _plugin.Chat(data.Player, ServerLang.Link.Declined.JoinWithUser, _plugin.GetDefault(data.Player, data.Discord));
                _plugin.SendTemplateMessage(TemplateKeys.Link.Declined.JoinWithUser, interaction);
            }
            else if (data.From == JoinedFrom.Discord)
            {
                _ban.AddBan(data.Discord);
                RemoveByUser(data.Discord);
                _plugin.Chat(data.Player, ServerLang.Link.Declined.JoinWithPlayer, _plugin.GetDefault(data.Player, data.Discord));
                _plugin.SendTemplateMessage(TemplateKeys.Link.Declined.JoinWithPlayer, interaction);
            }
        }
    }
}