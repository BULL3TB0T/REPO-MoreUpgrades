using HarmonyLib;
using MoreUpgrades.Classes;

namespace MoreUpgrades.Patches
{
    [HarmonyPatch(typeof(EnemyParent))]
    internal class EnemyParentPatch
    {
        [HarmonyPatch("SpawnRPC")]
        [HarmonyPostfix]
        static void SpawnRPC(EnemyParent __instance)
        {
            if (MoreUpgradesManager.instance == null)
                return;
            Plugin.instance.AddEnemyToMap(__instance);
        }

        [HarmonyPatch("DespawnRPC")]
        [HarmonyPostfix]
        static void DespawnRPC(EnemyParent __instance)
        {
            if (MoreUpgradesManager.instance == null)
                return;
            Plugin.instance.RemoveEnemyFromMap(__instance);
        }
    }
}
