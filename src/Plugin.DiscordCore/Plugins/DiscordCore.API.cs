using DiscordCorePlugin.Api;
using DiscordCorePlugin.Enums;
using Oxide.Core.Libraries.Covalence;
using Oxide.Ext.Discord.Entities.Users;
using Oxide.Ext.Discord.Extensions;

namespace DiscordCorePlugin.Plugins
{
    //Define:FileOrder=65
    public partial class DiscordCore
    {
        // ReSharper disable once UnusedMember.Local
        private string API_Link(IPlayer player, DiscordUser user)
        {
            if (player.IsLinked())
            {
                return ApiErrorCodes.PlayerIsLinked;
            }

            if (user.IsLinked())
            {
                return  ApiErrorCodes.UserIsLinked;
            }
            
            _linkHandler.HandleLink(player, user, LinkReason.Api, null);
            return null;
        }
        
        // ReSharper disable once UnusedMember.Local
        private string API_Unlink(IPlayer player, DiscordUser user)
        {
            if (!player.IsLinked())
            {
                return  ApiErrorCodes.PlayerIsNotLinked;
            }

            if (!user.IsLinked())
            {
                return ApiErrorCodes.UserIsNotLinked;
            }
            
            _linkHandler.HandleUnlink(player, user, UnlinkedReason.Api, null);
            return null;
        }
    }
}