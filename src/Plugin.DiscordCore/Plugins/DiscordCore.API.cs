using DiscordCorePlugin.Api;
using DiscordCorePlugin.Enums;
using Oxide.Core.Libraries.Covalence;
using Oxide.Ext.Discord.Entities.Users;
using Oxide.Ext.Discord.Extensions;

namespace DiscordCorePlugin.Plugins
{
    public partial class DiscordCore
    {
        //Define:FileOrder=65
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