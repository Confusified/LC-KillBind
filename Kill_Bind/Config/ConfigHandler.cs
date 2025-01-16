using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using BepInEx.Configuration;
using HarmonyLib;
using static Kill_Bind.Config.ConfigSettings;

namespace Kill_Bind.Config;

public class ConfigHandler
{
    internal static ConfigDescription RagdollTypeDescription;
    public static void InitialiseConfig()
    {
        ConfigFile cfg = Main.killbindConfig;
        // Disable auto-saving the config, as it is still being initialised.
        cfg.SaveOnConfigSet = false;

        RagdollTypeList = cfg.Bind<string>("z Do Not Touch z", "Ragdoll Types List", "PLACEHOLDER", "List of all ragdolls (since last startup). This allows for seamless usage between games.");
        ListRagdollType = new(Regex.Split(RagdollTypeList.Value, ";"));
        RagdollTypeDescription = new(description: "Determines what ragdoll will be used.", acceptableValues: new AcceptableValueList<string>(ListRagdollType.ToArray()));

        ModEnabled = cfg.Bind("Mod Settings", "Mod Enabled", true, "Determines whether the mod is enabled.");
        RagdollType = cfg.Bind("Mod Settings", "Type of Ragdoll", "HeadBurst", RagdollTypeDescription);
        DeathCause = cfg.Bind("Mod Settings", "Cause of Death", CauseOfDeath.Unknown, "Determines what the cause of death will be for your ragdoll.");


        ClearOrphanedEntries(cfg);
        cfg.Save();

        // Re-enable auto-saving the config
        cfg.SaveOnConfigSet = true;
    }

    // This removes all unused config entries, this does not bring over any old config values (i'm just too lazy to implement that (again))
    private static void ClearOrphanedEntries(ConfigFile cfg)
    {
        // Find the private property `OrphanedEntries` from the type `ConfigFile`
        PropertyInfo orphanedEntriesProp = AccessTools.Property(typeof(ConfigFile), "OrphanedEntries");
        // And get the value of that property from our ConfigFile instance
        var orphanedEntries = (Dictionary<ConfigDefinition, string>)orphanedEntriesProp.GetValue(cfg);
        // And finally, clear the `OrphanedEntries` dictionary
        orphanedEntries.Clear();
    }
}

public class ConfigSettings
{
    public static ConfigEntry<bool> ModEnabled;
    public static ConfigEntry<string> RagdollType;
    public static ConfigEntry<CauseOfDeath> DeathCause;
    // Using this list will make it possible to customise your config before entering a lobby.
    public static ConfigEntry<string> RagdollTypeList;
    public static List<string> ListRagdollType;
}