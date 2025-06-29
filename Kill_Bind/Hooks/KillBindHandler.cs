using System.Collections;
using System.Text.RegularExpressions;
using GameNetcodeStuff;
using Kill_Bind.Config;
using Kill_Bind.Hooks.DependencyRelated;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

namespace Kill_Bind.Hooks;

public class KillBindHandler : MonoBehaviour
{
    // By defining the WaitForEndOfFrame here, there won't be GC allocation when using the kill bind
    internal static WaitForEndOfFrame waitForFrameEnd = new();
    public static void OnPressKillBind(CallbackContext callbackContext)
    {
        // Defining variables
        bool performedCallback = callbackContext.performed;
        bool modEnabled = ConfigSettings.ModEnabled.Value;
        GameNetworkManager networkManager = GameNetworkManager.Instance;
        HUDManager hudManagerInstance = HUDManager.Instance;
        Terminal terminal = UnityEngine.Object.FindObjectOfType<Terminal>(); // does not support multiple terminals
        bool terminalInUse = terminal.terminalInUse;
        PlayerControllerB? player = networkManager.localPlayerController;
        bool isDead = player.isPlayerDead;
        bool isTyping = player.isTypingChat;
        bool hasTypingIndicator = hudManagerInstance.typingIndicator.enabled;
        bool inTerminalMenu = player.inTerminalMenu;
        QuickMenuManager quickmenuInstance = player.quickMenuManager;
        bool isMenuOpen = quickmenuInstance.isMenuOpen;
        bool inShipPhase = StartOfRoundHooks.StartOfRoundInstance.inShipPhase;

        Main.Logger.LogDebug("Keybind for KillBind has been pressed.");

        // We only want the kill bind to actually do something when the situation is valid
        if (!performedCallback) return;
        if (!modEnabled) return;
        if (player == null) return;
        if (isDead) return;
        if (inShipPhase) return;
        if (hudManagerInstance == null || hasTypingIndicator || isTyping) return;
        if (terminal == null || terminalInUse && inTerminalMenu) return;
        if (quickmenuInstance == null || isMenuOpen) return;

        player.StartCoroutine(KillAfterYield(player));
        // CoroutineHelper.Start(KillAfterYield(player));
    }

    public static IEnumerator KillAfterYield(PlayerControllerB localPlayer)
    {

        yield return waitForFrameEnd;

        // This fetches the int of the ragdoll as the game uses the index rather than the name for the ragdoll
        GameObject ragdoll = StartOfRoundHooks.PlayerRagdollsList.Find((GameObject x) => x.name.Contains(Regex.Replace(ConfigSettings.RagdollType.Value, " ", "", RegexOptions.None)));
        // Due to changing the name of the normal regular to "Normal", it cannot be found by this system. So it's done like this
        // 1 == Head Burst, which is the failsafe in case something goes wrong
        int ragdollInt = ConfigSettings.RagdollType.Value == "Normal" ? 0 : 1;
        ragdollInt = ragdoll != null ? StartOfRoundHooks.PlayerRagdollsList.IndexOf(ragdoll) : ragdollInt;

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