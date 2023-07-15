using DiscordCorePlugin.Placeholders;
using Oxide.Core.Libraries.Covalence;
using Oxide.Ext.Discord.Entities.Users;
using Oxide.Ext.Discord.Libraries.Placeholders;

namespace DiscordCorePlugin.Plugins
{
    //Define:FileOrder=55
    public partial class DiscordCore
    {
        public void RegisterPlaceholders()
        {
            if (!string.IsNullOrEmpty(_pluginConfig.ServerNameOverride))
            {
                _placeholders.RegisterPlaceholder(this, "guild.name", _pluginConfig.ServerNameOverride);
            }

            _placeholders.RegisterPlaceholder(this, PlaceholderKeys.InviteCode, _pluginConfig.InviteCode);
            _placeholders.RegisterPlaceholder(this, PlaceholderKeys.ServerLinkArg, ServerLinkArgument);
            _placeholders.RegisterPlaceholder<string>(this, PlaceholderKeys.LinkCode, PlaceholderKeys.Data.CodeKey);
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