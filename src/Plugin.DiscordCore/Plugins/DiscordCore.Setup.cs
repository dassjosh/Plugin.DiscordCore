using System;
using DiscordCorePlugin.Configuration;
using DiscordCorePlugin.Data;
using DiscordCorePlugin.Link;
using DiscordCorePlugin.Localization;
using Newtonsoft.Json;
using Oxide.Core;

namespace DiscordCorePlugin.Plugins
{
    //Define:FileOrder=10
    public partial class DiscordCore
    {
        // ReSharper disable once UnusedMember.Local
        private void Init()
        {
            Instance = this;
            _pluginData = Interface.Oxide.DataFileSystem.ReadObject<PluginData>(Name);

            permission.RegisterPermission(UsePermission, this);

            _banHandler = new JoinBanHandler(_pluginConfig.LinkBanSettings);
            _linkHandler = new LinkHandler(_pluginData, _pluginConfig);
            _joinHandler = new JoinHandler(_pluginConfig.LinkSettings, _linkHandler, _banHandler);

            _discordSettings.ApiToken = _pluginConfig.ApiKey;
            _discordSettings.LogLevel = _pluginConfig.ExtensionDebugging;
        }

        protected override void LoadDefaultConfig()
        {
            PrintWarning("Loading Default Config");
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            Config.Settings.DefaultValueHandling = DefaultValueHandling.Populate;
            _pluginConfig = AdditionalConfig(Config.ReadObject<PluginConfig>());
            Config.WriteObject(_pluginConfig);
        }

        public PluginConfig AdditionalConfig(PluginConfig config)
        {
            config.LinkSettings = new LinkSettings(config.LinkSettings);
            config.WelcomeMessageSettings = new WelcomeMessageSettings(config.WelcomeMessageSettings);
            config.LinkMessageSettings = new GuildMessageSettings(config.LinkMessageSettings);
            config.PermissionSettings = new LinkPermissionSettings(config.PermissionSettings);
            config.LinkBanSettings = new LinkBanSettings(config.LinkBanSettings);
            return config;
        }

        // ReSharper disable once UnusedMember.Local
        private void OnServerInitialized()
        {
            RegisterChatLangCommand(nameof(DiscordCoreChatCommand), ServerLang.Commands.DcCommand);
            
            if (string.IsNullOrEmpty(_pluginConfig.ApiKey))
            {
                PrintWarning("Please set the Discord Bot Token in the config and reload the plugin");
                return;
            }

            foreach (DiscordInfo info in _pluginData.PlayerDiscordInfo.Values)
            {
                if (info.LastOnline == DateTime.MinValue)
                {
                    info.LastOnline = DateTime.UtcNow;
                }
            }

            _link.AddLinkPlugin(this);
            RegisterPlaceholders();
            ValidateGroups();
            
            Client.Connect(_discordSettings);
        }

        public void ValidateGroups()
        {
            foreach (string group in _pluginConfig.PermissionSettings.LinkGroups)
            {
                if (!permission.GroupExists(group))
                {
                    PrintWarning($"`{group}` is set as the link group but group does not exist");
                }
            }
            
            foreach (string group in _pluginConfig.PermissionSettings.UnlinkGroups)
            {
                if (!permission.GroupExists(group))
                {
                    PrintWarning($"`{group}` is set as the unlink group but group does not exist");
                }
            }
            
            foreach (string perm in _pluginConfig.PermissionSettings.LinkPermissions)
            {
                if (!permission.PermissionExists(perm))
                {
                    PrintWarning($"`{perm}` is set as the link permission but group does not exist");
                }
            }
            
            foreach (string perm in _pluginConfig.PermissionSettings.UnlinkPermissions)
            {
                if (!permission.PermissionExists(perm))
                {
                    PrintWarning($"`{perm}` is set as the unlink permission but group does not exist");
                }
            }
        }

        // ReSharper disable once UnusedMember.Local
        private void Unload()
        {
            SaveData();
            Instance = null;
        }
    }
}