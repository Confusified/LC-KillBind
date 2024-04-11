using HarmonyLib;
using static KillBind.Patches.UIHandler;

namespace KillBind.Patches
{
    [HarmonyPatch(typeof(MenuManager))]
    public class MenuManagerPatch
    {
        public static MenuManager Instance;

        [HarmonyPatch("Awake")]
        [HarmonyPrefix]
        public static void OnAwake(MenuManager __instance)
        {
            if (__instance.isInitScene) { return; } //To avoid issues
            Instance = __instance;
            CreateInScene();
        }
    }
}