using BepInEx;
using BepInEx.Logging;
using Kill_Bind.Hooks;
using HarmonyLib;
using LobbyCompatibility.Attributes;
using LobbyCompatibility.Enums;

namespace Kill_Bind;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("BMX.LobbyCompatibility", BepInDependency.DependencyFlags.HardDependency)]
[LobbyCompatibility(CompatibilityLevel.ClientOnly, VersionStrictness.None)]
public class Kill_Bind : BaseUnityPlugin
{
    public static Kill_Bind Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger { get; private set; } = null!;

    private void Awake()
    {
        Logger = base.Logger;
        Instance = this;

        Hook();

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
    }

    internal static void Hook()
    {
        Logger.LogDebug("Hooking...");

        /*
         *  Subscribe with 'On.Class.Method += CustomClass.CustomMethod;' for each method you're patching.
         */

        On.TVScript.SwitchTVLocalClient += ExampleTVPatch.SwitchTVPatch;

        Logger.LogDebug("Finished Hooking!");
    }

    internal static void Unhook()
    {
        Logger.LogDebug("Unhooking...");

        /*
         *  Unsubscribe with 'On.Class.Method -= CustomClass.CustomMethod;' for each method you're patching.
         */

        On.TVScript.SwitchTVLocalClient -= ExampleTVPatch.SwitchTVPatch;

        Logger.LogDebug("Finished Unhooking!");
    }
}
