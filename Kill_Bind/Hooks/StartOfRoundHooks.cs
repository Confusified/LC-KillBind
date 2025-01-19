using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using BepInEx.Configuration;
using Kill_Bind.Config;
using Kill_Bind.Hooks.DependencyRelated;
using UnityEngine;

namespace Kill_Bind.Hooks;

public class StartOfRoundHooks
{
    public static StartOfRound StartOfRoundInstance;
    private static bool ragdollListCreated = false;
    public static void UpdateRagdollTypeConfig()
    {
        List<string> ListRagdollType = new(Regex.Split(ConfigSettings.RagdollTypeList, ";"));
        ConfigHandler.RagdollTypeDescription = new(description: "Determines what ragdoll will be used.", acceptableValues: new AcceptableValueList<string>(ListRagdollType.ToArray()));
        string oldVal = ConfigSettings.RagdollType.Value;

        // By temporarily disabling the auto-saving, it'll minimize the amount of unnecessary saving.
        Main.killbindConfig.SaveOnConfigSet = false;
        Main.killbindConfig.Remove(ConfigSettings.RagdollType.Definition);

        ConfigSettings.RagdollType = Main.killbindConfig.Bind("Mod Settings", "Type of Ragdoll", "Head Burst", ConfigHandler.RagdollTypeDescription);
        ConfigSettings.RagdollType.Value = oldVal;
        Main.killbindConfig.Save();

        if (SetupConfig_LethalConfig.LethalConfigFound) SetupConfig_LethalConfig.UpdateRagdollTypeDropdown();
        Main.killbindConfig.SaveOnConfigSet = true;
    }

    public static void UpdateRagdollTypeList(On.StartOfRound.orig_Start orig, StartOfRound self)
    {
        orig(self);
        if (ragdollListCreated) return;
        Main.Logger.LogDebug("Creating ragdoll list...");
        ConfigSettings.RagdollTypeList = "";
        foreach (GameObject ragdoll in self.playerRagdolls)
        {
            string ragdollName = CleanRagdollName(ragdoll.name);
            ConfigSettings.RagdollTypeList = (self.playerRagdolls.IndexOf(ragdoll) == 0) ? ragdollName : (ConfigSettings.RagdollTypeList + ";" + ragdollName);
        }

        UpdateRagdollTypeConfig();
        Main.Logger.LogDebug("Finished creating ragdoll list");
        StartOfRoundInstance = self;
        ragdollListCreated = true;
    }

    internal static string CleanRagdollName(string ragdollName)
    {
        if (ragdollName == "PlayerRagdoll")
        {
            return "Normal";
        }
        string pattern = "Player|Ragdoll|With|Variant|Prefab| ";
        ragdollName = Regex.Replace(ragdollName, pattern, "", RegexOptions.IgnoreCase);

        foreach (char letter in ragdollName.ToCharArray())
        {
            string stringLetter = letter.ToString();
            int index = ragdollName.IndexOf(stringLetter);
            if (index == 0 || index == -1) continue;

            string originalLetter = ragdollName.ToUpper().ToCharArray().GetValue(index).ToString();
            if (originalLetter == stringLetter)
            {
                ragdollName = ragdollName[..index] + " " + ragdollName[index..];
            }
        }
        return ragdollName;
    }
}