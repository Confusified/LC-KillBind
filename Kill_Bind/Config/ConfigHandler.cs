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

        RagdollTypeList = cfg.Bind("z Do Not Touch z", "Ragdoll List String", DEFAULT_RAGDOLL_TYPE, "This is used to retain the config setting for the type of ragdoll. Editing this may cause issues");
        ListRagdollType = new(Regex.Split(RagdollTypeList.Value, ";"));
        RagdollTypeDescription = new(description: "Determines what ragdoll will be used.", acceptableValues: new AcceptableValueList<string>(ListRagdollType.ToArray()));

        ModEnabled = cfg.Bind(CONFIG_SECTION, "Mod Enabled", true, "Determines whether the mod is enabled.");

        DeathCause = cfg.Bind(CONFIG_SECTION, "Cause of Death", CauseOfDeath.Unknown, "Determines what the cause of death will be for your ragdoll.");
        DeathCauseMatchesRagdollType = cfg.Bind(CONFIG_SECTION, "Accurate ragdolls", false, "When enabled causes your Cause of Death to match your selected ragdoll type.");
        RagdollType = cfg.Bind(CONFIG_SECTION, "Type of Ragdoll", DEFAULT_RAGDOLL_TYPE, RagdollTypeDescription);

        ClearOrphanedEntries(cfg);
        cfg.Save();

        // Re-enable auto-saving the config
        cfg.SaveOnConfigSet = true;
    }

    // This removes all unused config entries, this does not bring over any old config values (i'm just too lazy to implement that)
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

public struct ConfigSettings
{   
    internal const string CONFIG_SECTION = "Mod Settings";
    public const string DEFAULT_RAGDOLL_TYPE = "Head Burst";
    public static ConfigEntry<bool> ModEnabled;
    public static ConfigEntry<string> RagdollType;
    public static ConfigEntry<CauseOfDeath> DeathCause;
    public static ConfigEntry<bool> DeathCauseMatchesRagdollType;
    // Using this makes it possible to 'remember' what ragdoll the user used before exiting the game
    public static ConfigEntry<string> RagdollTypeList;
    public static List<string> ListRagdollType;
}