using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BepInEx.Configuration;
using Kill_Bind.Config;
using Kill_Bind.Hooks.DependencyRelated;
using UnityEngine;

namespace Kill_Bind.Hooks;

public class StartOfRoundHooks
{
    public static StartOfRound StartOfRoundInstance;
    public static List<GameObject> PlayerRagdollsList {get; private set;} = null!;
    private static bool ragdollListCreated = false;
    public static void UpdateRagdollTypeConfig()
    {
        ConfigSettings.ListRagdollType = new(Regex.Split(ConfigSettings.RagdollTypeList.Value, ";"));
        ConfigHandler.RagdollTypeDescription = new(description: "Determines what ragdoll will be used.", acceptableValues: new AcceptableValueList<string>(ConfigSettings.ListRagdollType.ToArray()));
        string oldVal = ConfigSettings.RagdollType.Value;

        // By temporarily disabling the auto-saving, it'll minimize the amount of unnecessary saving.
        Main.killbindConfig.SaveOnConfigSet = false;
        Main.killbindConfig.Remove(ConfigSettings.RagdollType.Definition);

        ConfigSettings.RagdollType = Main.killbindConfig.Bind("Mod Settings", "Type of Ragdoll", "HeadBurst", ConfigHandler.RagdollTypeDescription);
        ConfigSettings.RagdollType.Value = oldVal;
        Main.killbindConfig.Save();

        if (SetupConfig_LethalConfig.LethalConfigFound) SetupConfig_LethalConfig.UpdateRagdollTypeDropdown();
        Main.killbindConfig.SaveOnConfigSet = true;
    }

    public static void UpdateRagdollTypeList(Action<StartOfRound> orig, StartOfRound self)
    {
        orig(self);
        StartOfRoundInstance = self;
        if (ragdollListCreated) return;
        Main.Logger.LogDebug("Creating ragdoll list...");
        ConfigSettings.RagdollTypeList.Value = "";
        PlayerRagdollsList = self.playerRagdolls;
        foreach (GameObject ragdoll in PlayerRagdollsList)
        {
            string ragdollName = CleanRagdollName(ragdoll.name);
            ConfigSettings.RagdollTypeList.Value = (PlayerRagdollsList.IndexOf(ragdoll) == 0) ? ragdollName : (ConfigSettings.RagdollTypeList.Value + ";" + ragdollName);
            Main.Logger.LogDebug($"{PlayerRagdollsList.IndexOf(ragdoll)}: {ragdollName}");
        }

        UpdateRagdollTypeConfig();
        Main.Logger.LogDebug("Finished creating ragdoll list");
        
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

            string upperLetter = ragdollName.ToUpper().ToCharArray().GetValue(index).ToString();
            if (upperLetter == stringLetter)
            {
                ragdollName = ragdollName[..index] + " " + ragdollName[index..];
            }
        }
        return ragdollName;
    }
}