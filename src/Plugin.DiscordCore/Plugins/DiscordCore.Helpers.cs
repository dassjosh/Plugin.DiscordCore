using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Ext.Discord.Entities.Users;

namespace DiscordCorePlugin.Plugins
{
    //Define:FileOrder=70
    public partial class DiscordCore
    {
        public void SaveData()
        {
            if (_pluginData != null)
            {
                Interface.Oxide.DataFileSystem.WriteObject(Name, _pluginData);
            }
        }

        public void LogGuildLeaveUnlink(IPlayer player, DiscordUser user)
        {
            Puts($"Player {player.Name}({player.Id}) Discord {user.FullUserName}({user.Id}) is no longer in the guild and has been unlinked.");
        }
        
        public void LogServerInactiveUnlink(IPlayer player, DiscordUser user)
        {
            Puts($"Player {player.Name}({player.Id}) Discord {user.FullUserName}({user.Id}) has been inactive for {{0}} days and has been unlinked.");
        }
        
        public void LogApiUnlink(IPlayer player, DiscordUser user)
        {
            Puts($"Player {player.Name}({player.Id}) Discord {user.FullUserName}({user.Id}) has been unlinked using through the API.");
        }
    }
}