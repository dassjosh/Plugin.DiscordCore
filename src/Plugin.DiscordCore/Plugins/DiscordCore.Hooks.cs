using Oxide.Core.Libraries.Covalence;

namespace DiscordCorePlugin.Plugins
{
    //Define:FileOrder=27
    public partial class DiscordCore
    {
        // ReSharper disable once UnusedMember.Local
        private void OnUserConnected(IPlayer player)
        {
            _linkHandler.OnUserConnected(player);
        }
    }
}