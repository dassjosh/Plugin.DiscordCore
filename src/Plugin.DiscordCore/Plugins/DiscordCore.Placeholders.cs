using System;
using System.Text;
using Oxide.Core.Libraries.Covalence;
using Oxide.Ext.Discord.Entities.Users;
using Oxide.Ext.Discord.Libraries.Placeholders;
using Oxide.Ext.Discord.Libraries.Placeholders.Default;

namespace DiscordCorePlugin.Plugins
{
    //Define:FileOrder=55
    public partial class DiscordCore
    {
        private const string BanDurationKey = "ban.duration";
        private const string CodeKey = "dc.code";
        private const string OtherPlayerDataKey = "discordcore.other.player";
        private const string OtherUserDataKey = "discordcore.other.user";

        public void RegisterPlaceholders()
        {
            if (!string.IsNullOrEmpty(_pluginConfig.ServerNameOverride))
            {
                _placeholders.RegisterPlaceholder(this, "guild.name", _pluginConfig.ServerNameOverride);
            }

            _placeholders.RegisterPlaceholder(this, "discordcore.invite.code", _pluginConfig.InviteCode);
            _placeholders.RegisterPlaceholder(this, "discordcore.server.link.arg", ServerLinkArgument);
            _placeholders.RegisterPlaceholder(this, "discordcore.inactive.duration", InactiveDays);
            _placeholders.RegisterPlaceholder<TimeSpan>(this, "discordcore.join.banned.duration", BanDurationKey, BanDuration);
            _placeholders.RegisterPlaceholder<string>(this, "discordcore.link.code", CodeKey, Code);
            _placeholders.RegisterPlaceholder<string>(this, "discordcore.error.invalid.code", CodeKey, Code);
            PlayerPlaceholders.RegisterPlaceholders(this, "discordcore.other.player", OtherPlayerDataKey);
            UserPlaceholders.RegisterPlaceholders(this, "discordcore.other.user", OtherUserDataKey);
        }
        
        private void InactiveDays(StringBuilder builder, PlaceholderState state) => PlaceholderFormatting.Replace(builder, state, _pluginConfig.LinkSettings.UnlinkInactiveDays);
        private static void BanDuration(StringBuilder builder, PlaceholderState state, TimeSpan duration) => PlaceholderFormatting.Replace(builder, state, duration.TotalHours);
        private static void Code(StringBuilder builder, PlaceholderState state, string code) => PlaceholderFormatting.Replace(builder, state, code);

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