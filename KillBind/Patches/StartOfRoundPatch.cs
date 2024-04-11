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
                RagdollTypeList.Clear(); //Remove preset values

                foreach (GameObject ragdoll in __instance.playerRagdolls)
                {
                    string ragdollName = CleanRagdollName(ragdoll.name);
                    RagdollTypeList.Add(ragdollName);
                }
                HeadCreatedList = true;
                return;
            }
            return;
        }

        private static string CleanRagdollName(string ragdollName)
        {
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
                ragdollName = ragdollName.Replace("Player", "");
                ragdollName = ragdollName.Replace("Ragdoll", "");
                ragdollName = ragdollName.Replace(" Variant", "");
                //mostly just for custom added ones
                ragdollName = ragdollName.Replace("Variant", "");
                ragdollName = ragdollName.Replace("Prefab", "");
            }
            return ragdollName;
        }
    }
}