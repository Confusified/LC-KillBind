using System.Text.RegularExpressions;
using Kill_Bind.Config;
using LethalConfig;
using LethalConfig.ConfigItems;
using LethalConfig.ConfigItems.Options;

namespace Kill_Bind.Hooks.DependencyRelated;

public class SetupConfig_LethalConfig
{
    public static bool LethalConfigFound = false;
    private static bool ragdollTypeItemAdded = false;
    private static void Activate()
    {
        LethalConfigFound = true;
        LethalConfigManager.SkipAutoGen();

        var modEnabledBox = new BoolCheckBoxConfigItem(ConfigSettings.ModEnabled, new BoolCheckBoxOptions
        {
            Section = "Mod Settings",
            Name = "Mod Enabled",
            Description = "Determines whether the mod is enabled.",
            RequiresRestart = false
        });

        var deathcauseDropdown = new EnumDropDownConfigItem<CauseOfDeath>(ConfigSettings.DeathCause, new EnumDropDownOptions
        {
            Section = "Mod Settings",
            Name = "Cause of Death",
            Description = "Determines what the cause of death will be for your ragdoll.",
            RequiresRestart = false
        });

        LethalConfigManager.AddConfigItem(modEnabledBox);
        LethalConfigManager.AddConfigItem(deathcauseDropdown);
        
        // Commented out due to a bug, e.g. when adding ragdoll mods you'll have to boot into a lobby and then restart to be able to use your new ragdolls
        /*
        if (!ConfigSettings.ListRagdollType.Contains("PLACEHOLDER"))
        {
            ragdollTypeDropdown = new TextDropDownConfigItem(ConfigSettings.RagdollType, new TextDropDownOptions
            {
                Section = "Mod Settings",
                Name = "Type of Ragdoll",
                Description = "Determines what ragdoll will be used.",
                RequiresRestart = false
            });
            
            LethalConfigManager.AddConfigItem(ragdollTypeDropdown);
            ragdollTypeItemAdded = true;
        }
        */

        LethalConfigManager.SetModDescription("Become a ragdoll with just one button press");
    }


    public static void UpdateRagdollTypeDropdown()
    {
        if (!ragdollTypeItemAdded)
        {
            var ragdollTypeDropdown = new TextDropDownConfigItem(ConfigSettings.RagdollType, new TextDropDownOptions
            {
                Section = "Mod Settings",
                Name = "Type of Ragdoll",
                Description = "Determines what ragdoll will be used.",
                RequiresRestart = false
            });

            LethalConfigManager.AddConfigItem(ragdollTypeDropdown);
            ragdollTypeItemAdded = true;
        }
    }
}