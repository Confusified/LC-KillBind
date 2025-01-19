using Kill_Bind.Config;
using LethalConfig;
using LethalConfig.ConfigItems;
using LethalConfig.ConfigItems.Options;

namespace Kill_Bind.Hooks.DependencyRelated;

public class SetupConfig_LethalConfig
{
    public static bool LethalConfigFound = false;
    private static void Activate()
    {
        LethalConfigFound = true;
        LethalConfigManager.SkipAutoGen();

        var checkBoxItem = new BoolCheckBoxConfigItem(ConfigSettings.ModEnabled, new BoolCheckBoxOptions
        {
            Section = "Mod Settings",
            Name = "Mod Enabled",
            Description = "Determines whether the mod is enabled.",
            RequiresRestart = false
        });
        LethalConfigManager.AddConfigItem(checkBoxItem);

        var deathcauseDropdown = new EnumDropDownConfigItem<CauseOfDeath>(ConfigSettings.DeathCause, new EnumDropDownOptions
        {
            Section = "Mod Settings",
            Name = "Cause of Death",
            Description = "Determines what the cause of death will be for your ragdoll.",
            RequiresRestart = false
        });
        LethalConfigManager.AddConfigItem(deathcauseDropdown);

        LethalConfigManager.SetModDescription("Become a ragdoll with just one button press");
    }


    public static void UpdateRagdollTypeDropdown()
    {
        var ragdollTypeDropdown = new TextDropDownConfigItem(ConfigSettings.RagdollType, new TextDropDownOptions
        {
            Section = "Mod Settings",
            Name = "Type of Ragdoll",
            Description = "Determines what ragdoll will be used.",
            RequiresRestart = false
        });
        LethalConfigManager.AddConfigItem(ragdollTypeDropdown);
    }
}
