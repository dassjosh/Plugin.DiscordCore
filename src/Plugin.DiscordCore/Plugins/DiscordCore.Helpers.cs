using Oxide.Core;

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
    }
}