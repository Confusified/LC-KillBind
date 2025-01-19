using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DunGen;
using GameNetcodeStuff;
using Kill_Bind.Config;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.InputSystem.InputAction;

namespace Kill_Bind.Hooks.DependencyRelated;
public class KillBindHandler : MonoBehaviour
{
    // By defining the WaitForEndOfFrame here, performance while using the killbind will be slightly better
    internal static WaitForEndOfFrame waitForFrameEnd = new();
    private static GameNetworkManager? networkManager;
    private static PlayerControllerB? player;
    private static Terminal? terminal;
    private static HUDManager? hudManagerInstance;
    private static QuickMenuManager? quickmenuInstance;
    public static void OnPressKillBind(CallbackContext callbackContext)
    {
        Main.Logger.LogDebug("Keybind for KillBind has been pressed.");
        networkManager = GameNetworkManager.Instance;
        player = networkManager.localPlayerController;
        terminal = UnityEngine.Object.FindObjectOfType<Terminal>(); // does not support multiple terminals most likely (uh oh)
        hudManagerInstance = HUDManager.Instance;
        quickmenuInstance = player?.quickMenuManager;

        // We only want the kill bind to actually do something when the situation is valid
        if (!callbackContext.performed) return;
        if (!ConfigSettings.ModEnabled.Value) return;
        if (player == null || player != networkManager.localPlayerController) return;
        if (player.isPlayerDead) return;
        if (StartOfRoundHooks.StartOfRoundInstance.inShipPhase) return;
        if (hudManagerInstance == null || hudManagerInstance.typingIndicator.enabled || player.isTypingChat) return;
        if (terminal == null || terminal.terminalInUse && player.inTerminalMenu) return;
        if (quickmenuInstance == null || quickmenuInstance.isMenuOpen) return;

        Main.Logger.LogDebug("Passed KillBind's checks, attempting to kill after yielding until end of frame");
        CoroutineHelper.Start(KillAfterYield(player));
    }

    public static IEnumerator KillAfterYield(PlayerControllerB localPlayer)
    {
        List<GameObject> ragdollList = localPlayer.playersManager.playerRagdolls;
        yield return waitForFrameEnd;

        // This fetches the int of the ragdoll as the game uses the index rather than the name for the ragdoll
        GameObject ragdoll = ragdollList.Find((GameObject x) => x.name.Contains(Regex.Replace(ConfigSettings.RagdollType.Value, " ", "", RegexOptions.None)));
        // Due to changing the name of the normal regular to "Normal", it cannot be found by this system. So it's done like this
        // 1 == Head Burst, which is the failsafe in case something goes wrong
        int ragdollInt = ConfigSettings.RagdollType.Value == "Normal" ? 0 : 1;
        ragdollInt = ragdoll != null ? ragdollList.IndexOf(ragdoll) : ragdollInt;
        
        CauseOfDeath deathCause = ConfigSettings.DeathCause.Value;

        localPlayer.KillPlayer(localPlayer.thisController.velocity, spawnBody: true, causeOfDeath: deathCause, deathAnimation: ragdollInt, positionOffset: default);
        Main.Logger.LogDebug("Player should have died now");
        Main.Logger.LogDebug($"Ragdoll: {ConfigSettings.RagdollType.Value}, Ragdoll Int: {ragdollInt}, CoD: {deathCause}");

        if (ToilHead.ToilHeadMod_Present && ConfigSettings.RagdollType.Value == "Spring")
        {
            Main.Logger.LogDebug("Attempting to replace the ragdoll with a ToilHead variant");
            ToilHead.CreateToilheadRagdoll(localPlayer);
        }
    }
}