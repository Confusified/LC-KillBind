﻿using GameNetcodeStuff;
using HarmonyLib;
using KillBindNS;
using System;
using UnityEngine;

namespace KillBind.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    public class KillBindPatch
    {
        [HarmonyPatch("LateUpdate")]
        [HarmonyPrefix]
        public static void OnPressKill(PlayerControllerB __instance)
        {
            if (BasePlugin.ModEnabled.Value && BasePlugin.InputActionInstance.ExplodeKey.triggered && __instance == GameNetworkManager.Instance.localPlayerController && !__instance.isPlayerDead && !__instance.isTypingChat && !HUDManager.Instance.typingIndicator.enabled && (!UnityEngine.Object.FindObjectOfType<Terminal>().terminalInUse && !__instance.inTerminalMenu))
            {
                if (BasePlugin.DeathCause.Value > Enum.GetValues(typeof(CauseOfDeath)).Length || BasePlugin.DeathCause.Value < 0) { BasePlugin.DeathCause.Value = (int)BasePlugin.DeathCause.DefaultValue; BasePlugin.mls.LogInfo("Your config for HeadType is invalid, reverting to default"); } //If your choice is invalid, set to default (unknown death cause)
                if (BasePlugin.HeadType.Value > 3 || BasePlugin.HeadType.Value < 0) { BasePlugin.HeadType.Value = (int)BasePlugin.HeadType.DefaultValue; BasePlugin.mls.LogInfo("Your config for HeadType is invalid, reverting to default"); } //If your choice is invalid, set to default (explode head)

                __instance.KillPlayer(Vector3.zero, true, (CauseOfDeath)BasePlugin.DeathCause.Value, BasePlugin.HeadType.Value);
            }
        }
    }
}