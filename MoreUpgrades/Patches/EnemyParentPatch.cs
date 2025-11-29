using HarmonyLib;
using MoreUpgrades.Classes;

namespace MoreUpgrades.Patches
{
    [HarmonyPatch(typeof(EnemyParent))]
    internal class EnemyParentPatch
    {
        [HarmonyPatch("Setup")]
        [HarmonyPostfix]
        static void Setup(EnemyParent __instance)
        {
            if (MoreUpgradesManager.instance == null)
                return;
            Plugin.instance.RegisterToMap(__instance);
        }

        [HarmonyPatch("SpawnRPC")]
        [HarmonyPostfix]
        static void SpawnRPC(EnemyParent __instance)
        {
            if (MoreUpgradesManager.instance == null)
                return;
            Plugin.instance.ShowToMap(__instance);
        }

        [HarmonyPatch("DespawnRPC")]
        [HarmonyPostfix]
        static void DespawnRPC(EnemyParent __instance)
        {
            if (MoreUpgradesManager.instance == null)
                return;
            Plugin.instance.HideFromMap(__instance);
        }
    }
}
