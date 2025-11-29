using HarmonyLib;
using MoreUpgrades.Classes;

namespace MoreUpgrades.Patches
{
    [HarmonyPatch(typeof(EnemyHealth))]
    internal class EnemyHealthPatch
    {
        [HarmonyPatch("DeathRPC")]
        [HarmonyPostfix]
        static void DeathRPC(EnemyHealth __instance, Enemy ___enemy)
        {
            if (MoreUpgradesManager.instance == null)
                return;
            Plugin.instance.HideFromMap((EnemyParent)AccessTools.Field(typeof(Enemy), "EnemyParent").GetValue(___enemy));
        }
    }
}
