using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Kill_Bind.Config;
using Kill_Bind.Hooks;
using Kill_Bind.Hooks.DependencyRelated;
using LethalCompanyInputUtils.Api;
using LethalCompanyInputUtils.BindingPathEnums;
using Mono.Cecil.Cil;
using MonoMod.RuntimeDetour;
using UnityEngine.InputSystem;

namespace Kill_Bind;

public class KillBind_Inputs : LcInputActions
{
    [InputAction(KeyboardControl.Backspace, Name = "Suicide", ActionType = InputActionType.Button)]
    public InputAction ActionKillBind { get; set; }
}

// Soft Dependencies
[SoftDependency("BMX.LobbyCompatibility", typeof(RegisterPlugin_LobbyCompatibility))]
[SoftDependency("com.github.zehsteam.ToilHead", typeof(ToilHead))]
// While LethalConfig is not necessary, it is highly recommended.
[SoftDependency("ainavt.lc.lethalconfig", typeof(SetupConfig_LethalConfig))]

// Hard Dependencies
[BepInDependency("com.rune580.LethalCompanyInputUtils", BepInDependency.DependencyFlags.HardDependency)]

// Plugin
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Main : BaseUnityPlugin
{
    public static Main Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger { get; private set; } = null!;
    private static string[] modGUIDSegments = MyPluginInfo.PLUGIN_GUID.Split(".");
    private static readonly string configLocation = Utility.CombinePaths(Paths.ConfigPath + "\\" + modGUIDSegments[1].Replace(".", "\\")) + modGUIDSegments[2];
    internal static ConfigFile killbindConfig = new(configLocation + ".cfg", false);
    internal static List<IDetour> Hooks { get; set; } = new List<IDetour>();
    public static readonly KillBind_Inputs InputActionInstance = new();

    public void Awake()
    {
        Logger = base.Logger;
        Instance = this;

        ConfigHandler.InitialiseConfig();
        // Running code from the soft dependencies in the same file will cause an error if you do not have the dependency.
        InitialiseSoftDependencies();
        HookMethods();

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
    }

    private static void InitialiseSoftDependencies()
    {
        Logger.LogDebug("Activating Soft Dependencies...");

        SoftDependencyAttribute.Init(Instance);
    }

    private static void HookMethods()
    {
        Logger.LogDebug("Hooking...");

        Hooks.Add(new Hook(typeof(StartOfRound).GetMethod("Start", AccessTools.allDeclared), StartOfRoundHooks.UpdateRagdollTypeList));
        // On.StartOfRound.Start += StartOfRoundHooks.UpdateRagdollTypeList;
        Logger.LogDebug("Hooked: StartOfRound, Start");

        Logger.LogDebug("Finished Hooking.");

        InputActionInstance.ActionKillBind.performed += KillBindHandler.OnPressKillBind;
        Logger.LogDebug("Bound KillBind's Keybind.");
    }
}

// Source: https://discord.com/channels/1168655651455639582/1216761387343151134/1216761387343151134, Lethal Company Modding, Discord Server
internal class SoftDependencyAttribute : BepInDependency
{
    public System.Type Handler;

    /// <summary>
    /// Marks this BepInEx.BaseUnityPlugin as soft depenant on another plugin.
    /// The handler type must have an Initialize() method that will automatically be invoked if the compatible dependency is present.
    /// </summary>
    /// <param name="guid">The GUID of the referenced plugin.</param>
    /// <param name="handlerType">The class type that will handle this compatibility. Must contain a private method called Initialize()</param>
    public SoftDependencyAttribute(string guid, System.Type handlerType) : base(guid, DependencyFlags.SoftDependency)
    {
        Handler = handlerType;
    }

    /// <summary>
    /// Global initializer for this class.
    /// You must call this method from your base plugin Awake method and pass the plugin instance to the source parameter.
    /// </summary>
    /// <param name="source">The source plugin instance with the BepInPlugin attribute.</param>
    internal static void Init(BaseUnityPlugin source)
    {
        const BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Static;

        IEnumerable<SoftDependencyAttribute> attributes = source.GetType().GetCustomAttributes<SoftDependencyAttribute>();
        foreach (SoftDependencyAttribute attr in attributes)
        {
            if (attr == null) continue;
            if (Chainloader.PluginInfos.ContainsKey(attr.DependencyGUID))
            {
                Main.Logger.LogDebug("Found compatible mod: " + attr.DependencyGUID);
                attr.Handler.GetMethod("Activate", bindingFlags)?.Invoke(null, null);
                attr.Handler = null!;
            }
        }
    }
}