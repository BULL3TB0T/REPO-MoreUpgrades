using HarmonyLib;
using MoreUpgrades.Classes;

namespace MoreUpgrades.Patches
{
    [HarmonyPatch(typeof(EnemySlowMouth))]
    internal class EnemySlowMouthPatch
    {
        [HarmonyPatch("UpdateStateRPC")]
        [HarmonyPostfix]
        static void UpdateStateRPC(EnemySlowMouth __instance, Enemy ___enemy)
        {
            if (MoreUpgradesManager.instance == null || ___enemy == null)
                return;
            PlayerAvatar playerTarget = (PlayerAvatar)AccessTools.Field(typeof(EnemySlowMouth), "playerTarget").GetValue(__instance);
            EnemyParent enemyParent = (EnemyParent)AccessTools.Field(typeof(Enemy), "EnemyParent").GetValue(___enemy);
            EnemySlowMouth.State state = __instance.currentState;
            if (state == EnemySlowMouth.State.Attached)
            {
                Plugin.instance.RemoveEnemyFromMap(enemyParent);
                if (playerTarget == PlayerController.instance.playerAvatarScript)
                    return;
                Plugin.instance.RemovePlayerFromMap(playerTarget);
                Plugin.instance.AddEnemyToMap(playerTarget, enemyParent.enemyName);
            }
            else if (state == EnemySlowMouth.State.Detach)
            {
                Plugin.instance.AddEnemyToMap(enemyParent);
                if (playerTarget == PlayerController.instance.playerAvatarScript)
                    return;
                Plugin.instance.AddPlayerToMap(playerTarget);
                Plugin.instance.RemoveEnemyFromMap(playerTarget, enemyParent.enemyName);
            }
        }
    }
}
