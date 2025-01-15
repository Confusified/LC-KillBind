using BepInEx.Configuration;

namespace Kill_Bind.Config;

public class ConfigHandler
{

    public static void InitialiseConfig()
    {

    }
}

public class ConfigSettings
{
    public static ConfigEntry<string> DeathAnimation = Main.killbindConfig.Bind<string>("Mod Settings", "Ragdoll Type", "Unknown", "ragdoll type");
    public static ConfigEntry<int> DeathCause = Main.killbindConfig.Bind<int>("Mod Settings", "Death Cause", 0, "death cause");
}