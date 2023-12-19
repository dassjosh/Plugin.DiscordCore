using DiscordCorePlugin.Placeholders;
using Oxide.Core.Libraries.Covalence;
using Oxide.Ext.Discord.Entities;
using Oxide.Ext.Discord.Libraries;

namespace DiscordCorePlugin.Plugins
{
    //Define:FileOrder=55
    public partial class DiscordCore
    {
        public void RegisterPlaceholders()
        {
            if (!string.IsNullOrEmpty(_pluginConfig.ServerNameOverride))
            {
                _placeholders.RegisterPlaceholder(this, new PlaceholderKey("guild.name"), _pluginConfig.ServerNameOverride);
            }

            _placeholders.RegisterPlaceholder(this, PlaceholderKeys.InviteUrl, _pluginConfig.InviteUrl);
            _placeholders.RegisterPlaceholder<string>(this, PlaceholderKeys.LinkCode, PlaceholderDataKeys.Code);
            _placeholders.RegisterPlaceholder<string>(this, PlaceholderKeys.NotFound, PlaceholderDataKeys.NotFound);
        }

        public string LangPlaceholder(string key, PlaceholderData data)
        {
            return _placeholders.ProcessPlaceholders(Lang(key), data);
        }

        public PlaceholderData GetDefault()
        {
            return _placeholders.CreateData(this).AddGuild(Guild);
        }
        
        public PlaceholderData GetDefault(IPlayer player)
        {
            return GetDefault().AddPlayer(player);
        }
        
        public PlaceholderData GetDefault(DiscordUser user)
        {
            return GetDefault().AddUser(user);
        }
        
        public PlaceholderData GetDefault(IPlayer player, DiscordUser user)
        {
            return GetDefault(player).AddUser(user);
        }
    }
}