﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using static KillBind.Patches.UIHandler;

namespace KillBind.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    public class StartOfRoundPatch
    {
        public static bool HeadCreatedList = false;

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void UpdateHeadDropdownList(StartOfRound __instance)
        {
            if (!HeadCreatedList) //Create list with real values
            {
                HeadTypeDropdownList.Clear(); //Remove preset values

                foreach (GameObject ragdoll in __instance.playerRagdolls)
                {
                    string ragdollName = CleanRagdollName(ragdoll.name);
                    HeadTypeDropdownList.Add(ragdollName);
                }
                HeadCreatedList = true;
                return;
            }
            return;
        }

        public static string CleanRagdollName(string ragdollName)
        {
            Initialise.modLogger.LogInfo(ragdollName);
            if (ragdollName == "PlayerRagdoll") //Normal ragdoll
            {
                ragdollName = "Normal";
            }
            else if (ragdollName == "PlayerRagdollWithComedyMask Variant")
            {
                ragdollName = "Comedy Mask";
            }
            else if (ragdollName == "PlayerRagdollWithTragedyMask Variant")
            {
                ragdollName = "Tragedy Mask";
            }
            else
            {
                ragdollName = ragdollName.Replace("PlayerRagdoll", "");
                ragdollName = ragdollName.Replace(" Variant", "");
            }
            return ragdollName;
        }
    }
}