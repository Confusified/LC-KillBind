using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LethalCompanyInputUtils.Api;
using UnityEngine.InputSystem;
using System.Reflection;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace KillBind
{
    public class KillBind_Inputs : LcInputActions
    {
        [InputAction("<Keyboard>/k", Name = "Suicide", ActionType = InputActionType.Button)]
        public InputAction ActionKillBind { get; set; }
    }

    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency("com.rune580.LethalCompanyInputUtils", BepInDependency.DependencyFlags.HardDependency)]
    public class Initialise : BaseUnityPlugin
    {
        private const string modGUID = "com.Confusified.KillBind";
        private const string modName = "Kill Bind";
        private const string modVersion = "2.1.3";

        public static readonly KillBind_Inputs InputActionInstance = new KillBind_Inputs();
        private readonly Harmony _harmony = new Harmony(modGUID);
        public static ManualLogSource modLogger;

        private static readonly string configLocation = Utility.CombinePaths(Paths.ConfigPath + "\\" + modGUID.Substring(4).Replace(".", "\\"));
        private static readonly string privateConfigLocation = configLocation + ".private";
        private static ConfigFile modConfig = new ConfigFile(configLocation + ".cfg", false);

        public static List<string> RagdollTypeList;

        public class DefaultModSettings
        {
            public static bool ModEnabled = true;
            public static int DeathCause = 0;
            public static int RagdollType = 1;
            public static int ConfigVersion = 2;
            public static bool FirstTime = true;
        }

        public class ModSettings
        {
            public static ConfigEntry<bool> ModEnabled;
            public static ConfigEntry<int> DeathCause;
            public static ConfigEntry<int> RagdollType;
            public static int ConfigVersion;
            public static bool FirstTime;
        }

        public class LegacySettings
        {
            public static ConfigEntry<int> HeadType; //from before v2.1.0
        }

        public void Awake()
        {
            modLogger = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            modLogger = Logger;

            InitialiseConfig();

            ES3.CacheFile(privateConfigLocation);

            if (ModSettings.FirstTime)
            {
                ModSettings.ConfigVersion = -1; //To make sure it'll update your config
                ModSettings.FirstTime = false;
                ES3.Save("FirstTime", ModSettings.FirstTime, privateConfigLocation);
            }

            UpdateConfig();
            ClearLegacySettings();

            if (ModSettings.ModEnabled.Value)
            {
                _harmony.PatchAll(Assembly.GetExecutingAssembly());
                modLogger.LogInfo($"{modName} {modVersion} has loaded");
                return;
            }
            modLogger.LogInfo($"{modName} {modVersion} did not load, it is disabled in the config");
            return;
        }

        private void InitialiseConfig()
        {
            LoadPrivateConfig();
            ModSettings.ModEnabled = modConfig.Bind<bool>("Mod Settings", "Enabled", true, "Toggle the mod");
            ModSettings.DeathCause = modConfig.Bind<int>("Mod Settings", "Death Cause", 0, "Cause of Death your ragdoll will have");
            ModSettings.RagdollType = modConfig.Bind<int>("Mod Settings", "Ragdoll Type", 1, "Type of ragdoll you will have");

            //Legacy settings
            LegacySettings.HeadType = modConfig.Bind<int>("Mod Settings", "HeadType", 1, "Type of head your ragdoll will have");
        }
        //hi if anyone is reading this please don't do config stuff like this, i need to change it but i don't want to
        private void UpdateConfig()
        {
            if (ModSettings.ConfigVersion == DefaultModSettings.ConfigVersion) { return; }
            int[] oldInts = { LegacySettings.HeadType.Value, ModSettings.DeathCause.Value, ModSettings.ConfigVersion };
            bool oldModEnabled = ModSettings.ModEnabled.Value;

            //Clear files and variables
            modConfig.Clear();
            modConfig = null;
            File.WriteAllText(configLocation + ".cfg", ""); //Clears config (private one is unnecessary because you can't edit it manually
            modConfig = new ConfigFile(configLocation + ".cfg", false);

            ModSettings.ModEnabled = null;
            ModSettings.DeathCause = null;
            ModSettings.RagdollType = null;
            ModSettings.ConfigVersion = -999;

            //Set default values
            InitialiseConfig();

            //Restore old values
            ModSettings.RagdollType.Value = oldInts[0];
            ModSettings.DeathCause.Value = oldInts[1];
            ModSettings.ModEnabled.Value = oldModEnabled;
            ModSettings.ConfigVersion = DefaultModSettings.ConfigVersion;
            ES3.Save("ConfigVersion", ModSettings.ConfigVersion, privateConfigLocation);

            StringBuilder updatedConfigOutput = new StringBuilder("Updated config file from version 0 to 1");
            updatedConfigOutput.Replace("0", oldInts[2].ToString());
            updatedConfigOutput.Replace("1", ModSettings.ConfigVersion.ToString());
            modLogger.LogInfo(updatedConfigOutput);
            return;
        }

        private static void LoadPrivateConfig()
        {
            ModSettings.ConfigVersion = ES3.Load("ConfigVersion", privateConfigLocation, DefaultModSettings.ConfigVersion);
            ModSettings.FirstTime = ES3.Load("FirstTime", privateConfigLocation, DefaultModSettings.FirstTime);
            return;
        }

        private static void ClearLegacySettings()
        {
            modConfig.Remove(LegacySettings.HeadType.Definition);
        }
    }
}