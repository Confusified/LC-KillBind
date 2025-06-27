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
        if (!performedCallback) return;
        
        bool modEnabled = ConfigSettings.ModEnabled.Value;
        if (!modEnabled) return;

        GameNetworkManager networkManager = GameNetworkManager.Instance;
        PlayerControllerB? player = networkManager.localPlayerController;
        if (player == null) return;
        bool isDead = player.isPlayerDead;
        bool isTyping = player.isTypingChat;
        bool inTerminalMenu = player.inTerminalMenu;

        Terminal terminal = UnityEngine.Object.FindObjectOfType<Terminal>(); // does not support multiple terminals
        bool terminalInUse = terminal.terminalInUse;

        HUDManager hudManagerInstance = HUDManager.Instance;
        bool hasTypingIndicator = hudManagerInstance.typingIndicator.enabled;

        QuickMenuManager quickmenuInstance = player.quickMenuManager;
        bool isMenuOpen = quickmenuInstance.isMenuOpen;

        bool inShipPhase = StartOfRoundHooks.StartOfRoundInstance.inShipPhase;

        Main.Logger.LogDebug("Keybind for KillBind has been pressed.");

        // We only want the kill bind to actually do something when the situation is valid


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

        string ragdollTypeValue = ConfigSettings.RagdollType.Value;
        // This fetches the int of the ragdoll as the game uses the index rather than the name for the ragdoll
        GameObject ragdoll = StartOfRoundHooks.PlayerRagdollsList.Find((GameObject x) => x.name.Contains(Regex.Replace(ragdollTypeValue, " ", "", RegexOptions.None)));

        // Due to changing the name of the normal regular to "Normal", it cannot be found by this system. So it's done like this
        // 1 == Head Burst, which is the failsafe in case something goes wrong

        int ragdollInt = ragdollTypeValue == "Normal" ? 0 : 1;
        ragdollInt = ragdoll != null ? StartOfRoundHooks.PlayerRagdollsList.IndexOf(ragdoll) : ragdollInt;

        CauseOfDeath deathCause = ConfigSettings.DeathCause.Value;
        bool matchRagdoll = ConfigSettings.DeathCauseMatchesRagdollType.Value;
        if (matchRagdoll)
        {
            switch (ragdollInt)
            {
                case 0: // Taken from MouthDogAI & CaveDwellerAI & CrawlerAI & HoarderBugAI & JesterAI & PufferAI & SandSpiderAI, (Some use this with a different CoD)
                    deathCause = CauseOfDeath.Mauling;
                    break;
                case 1: //
                    deathCause = CauseOfDeath.Unknown;
                    break;
                case 2: // Taken from SpringManAI
                    deathCause = CauseOfDeath.Mauling;
                    break;
                case 3: // Taken from RedLocustBees
                    deathCause = CauseOfDeath.Electrocution;
                    break;
                case 4: // Taken from MaskedPlayerEnemy - Comedy variant
                    deathCause = CauseOfDeath.Strangulation;
                    break;
                case 5: // Tragedy variant
                    deathCause = CauseOfDeath.Strangulation;
                    break;
                case 6: // Taken from RadMechAI
                    deathCause = CauseOfDeath.Burning;
                    break;
                case 7: // Taken from ClaySurgeonAI
                    deathCause = CauseOfDeath.Snipped;
                    break;
                case 8: // Taken from BushWolfEnemy
                    deathCause = CauseOfDeath.Mauling;
                    break;
                case 9: // Taken from GiantKiwiAI
                    deathCause = CauseOfDeath.Stabbing;
                    break;
                default: // If not added yet
                    deathCause = CauseOfDeath.Unknown;
                    break;

            }
        }

        Main.Logger.LogDebug("Player should have died now");
        Main.Logger.LogDebug($"Ragdoll: {ragdollTypeValue}, Ragdoll Int: {ragdollInt}, CoD: {deathCause}");
        localPlayer.KillPlayer(localPlayer.thisController.velocity, spawnBody: true, causeOfDeath: deathCause, deathAnimation: ragdollInt, positionOffset: default);
        if (ToilHead.ToilHeadMod_Present && ragdollTypeValue == "Spring")
        {
            Main.Logger.LogDebug("Attempting to replace the ragdoll with a ToilHead variant");
            ToilHead.CreateToilheadRagdoll(localPlayer);
        }
    }
}