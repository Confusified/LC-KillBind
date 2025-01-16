using System.Collections;
using System.Collections.Generic;
using DunGen;
using GameNetcodeStuff;
using Kill_Bind.Config;
using UnityEngine;
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
    public static void OnPressKillBind(CallbackContext callbackContext)
    {
        Main.Logger.LogDebug("Keybind for KillBind has been pressed.");
        networkManager = GameNetworkManager.Instance;
        player = networkManager.localPlayerController;
        terminal = UnityEngine.Object.FindObjectOfType<Terminal>(); // does not support multiple terminals most likely (uh oh)
        hudManagerInstance = HUDManager.Instance;

        // We only want the kill bind to actually do something when the situation is valid
        if (!callbackContext.performed) return;
        if (player != networkManager.localPlayerController) return;
        if (player.isPlayerDead) return;
        if (hudManagerInstance.typingIndicator.enabled || player.isTypingChat) return;
        if (terminal.terminalInUse && player.inTerminalMenu) return;

        Main.Logger.LogDebug("Passed KillBind's checks, attempting to kill after yielding until end of frame");
        CoroutineHelper.Start(KillAfterYield(player));
    }

    public static IEnumerator KillAfterYield(PlayerControllerB localPlayer)
    {
        List<GameObject> ragdollList = localPlayer.playersManager.playerRagdolls;
        yield return waitForFrameEnd;
        // 
        GameObject ragdoll = ragdollList.Find((GameObject x) => x.name.Contains(ConfigSettings.RagdollType.Value));
        int num = (!(ConfigSettings.RagdollType.Value == "Normal")) ? 1 : 0;
        num = ragdoll != null ? ragdollList.IndexOf(ragdoll) : num;

        localPlayer.KillPlayer(localPlayer.thisController.velocity, true, ConfigSettings.DeathCause.Value, num);
        Main.Logger.LogDebug("Player should have died now by use of KillBind.");

        if (ToilHead.ToilHeadMod_Present && ConfigSettings.RagdollType.Value == "Spring")
        {
            Main.Logger.LogDebug("Attempting to replace the ragdoll with a ToilHead variant");
            ToilHead.CreateToilheadRagdoll(localPlayer);
        }
    }
}