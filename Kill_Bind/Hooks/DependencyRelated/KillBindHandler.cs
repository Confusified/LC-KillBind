using System.Collections;
using System.Collections.Generic;
using GameNetcodeStuff;
using Kill_Bind.Config;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

namespace Kill_Bind.Hooks.DependencyRelated;
public class KillBindHandler : MonoBehaviour
{
    internal static WaitForEndOfFrame waitForFrameEnd = new();
    private static IEnumerator? killCoroutine;
    private GameNetworkManager? networkManager;
    private PlayerControllerB? player;
    private Terminal terminal;
    private HUDManager hudManagerInstance;
    public void OnPressKillBind(CallbackContext callbackContext)
    {
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
        killCoroutine = KillAfterYield(player);
        StartCoroutine(killCoroutine);
    }

    public static IEnumerator KillAfterYield(PlayerControllerB localPlayer)
    {
        List<GameObject> ragdollList = localPlayer.playersManager.playerRagdolls;
        yield return waitForFrameEnd;
        GameObject ragdoll = ragdollList.Find((GameObject x) => ((Object)x).name.Contains(ConfigSettings.DeathAnimation.Value));
        int num = (!(ConfigSettings.DeathAnimation.Value == "Normal")) ? 1 : 0;
        num = ragdoll != null ? ragdollList.IndexOf(ragdoll) : num;
        localPlayer.KillPlayer(localPlayer.thisController.velocity, true, (CauseOfDeath)ConfigSettings.DeathCause.Value, num, default(Vector3));
        Main.Logger.LogDebug("Player should have died now by use of KillBind.");
        if (ToilHead.ToilHeadMod_Present && ConfigSettings.DeathAnimation.Value == "Spring")
        {
            Main.Logger.LogDebug("Attempting to replace the ragdoll with a ToilHead variant");
            ToilHead.CreateToilheadRagdoll(localPlayer);
        }
    }
}