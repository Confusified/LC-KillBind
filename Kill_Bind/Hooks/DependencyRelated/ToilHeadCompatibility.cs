using System;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using com.github.zehsteam.ToilHead;
using GameNetcodeStuff;

namespace Kill_Bind.Hooks.DependencyRelated;

public class ToilHead
{
    public static bool ToilHeadMod_Present = false;
    
    // This value is responsible for spawning a Toil-Head OR a Slayer-Player (?), depending on RNG.
    public static float ToilSlayerRagdollChance = 10f; // Default value of ToilHead is 10, so I replicate the same default
    internal static ConfigEntry<float> ToilPlayerSlayerChance = null!;

    internal static void CreateToilheadRagdoll(PlayerControllerB self)
    {
        bool editRagdoll = Utils.RandomPercent(ToilSlayerRagdollChance);
        TurretHeadManager.SetDeadBodyTurretHead(self, editRagdoll);
    }
    private static void Activate()
    {
        ToilHeadMod_Present = true;
        ConfigFile config = Chainloader.PluginInfos["com.github.zehsteam.ToilHead"].Instance.Config;
        foreach (ConfigDefinition key in config.Keys)
        {
            if (key.Section == "Toil-Player Settings" && key.Key == "ToilPlayerSlayerChance")
            {
                config.TryGetEntry<float>(key, out ToilPlayerSlayerChance);
                ToilSlayerRagdollChance = ToilPlayerSlayerChance.Value;
                Main.Logger.LogDebug($"Updated ToilSlayerRagdollChance to {ToilSlayerRagdollChance}");
                ToilPlayerSlayerChance.SettingChanged += UpdateChanceValue;
                return;
            }
        }
        Main.Logger.LogDebug($"Could not find ToilPlayerSlayerChance config setting. ToilSlayerRagdollChance will use the default value set by KillBind ({ToilSlayerRagdollChance})");
    }

    private static void UpdateChanceValue(object sender = null!, EventArgs args = null!)
    {
        ToilSlayerRagdollChance = ToilPlayerSlayerChance.Value;
        Main.Logger.LogDebug($"Updated ToilSlayerRagdollChance to {ToilSlayerRagdollChance}");
    }
}