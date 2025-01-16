using System;
using LobbyCompatibility.Enums;
using LobbyCompatibility.Features;

namespace Kill_Bind.Hooks.DependencyRelated
{
    public static class RegisterPlugin_LobbyCompatibility
    {
        private static void Activate()
        {
            PluginHelper.RegisterPlugin(guid: MyPluginInfo.PLUGIN_GUID, version: Version.Parse(MyPluginInfo.PLUGIN_VERSION), compatibilityLevel: CompatibilityLevel.ClientOnly, versionStrictness: VersionStrictness.None);
        }
    }
}