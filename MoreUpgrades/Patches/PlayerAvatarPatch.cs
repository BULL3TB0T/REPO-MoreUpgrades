using HarmonyLib;
using MoreUpgrades.Classes;
using System.Linq;

namespace MoreUpgrades.Patches
{
    [HarmonyPatch(typeof(PlayerAvatar))]
    internal class PlayerAvatarPatch
    {
        [HarmonyPatch("LateStart")]
        [HarmonyPostfix]
        static void LateStart(PlayerAvatar __instance)
        {
            if (MoreUpgradesManager.instance == null || __instance == PlayerController.instance.playerAvatarScript)
                return;
            Plugin.instance.AddPlayerToMap(__instance);
        }

        [HarmonyPatch("ReviveRPC")]
        [HarmonyPostfix]
        static void ReviveRPC(PlayerAvatar __instance)
        {
            if (MoreUpgradesManager.instance == null || __instance == PlayerController.instance.playerAvatarScript)
                return;
            Plugin.instance.AddPlayerToMap(__instance);
        }

        [HarmonyPatch("PlayerDeathRPC")]
        [HarmonyPostfix]
        static void PlayerDeathRPC(PlayerAvatar __instance)
        {
            if (MoreUpgradesManager.instance == null || __instance == PlayerController.instance.playerAvatarScript)
                return;
            Plugin.instance.RemovePlayerFromMap(__instance);
            Plugin.instance.RemoveEnemyFromMap(__instance);
        }

        [HarmonyPatch("SetColorRPC")]
        [HarmonyPostfix]
        static void SetColorRPC(PlayerAvatar __instance)
        {
            if (MoreUpgradesManager.instance == null || __instance == PlayerController.instance.playerAvatarScript)
                return;
            UpgradeItem upgradeItem = Plugin.instance.upgradeItems.FirstOrDefault(x => x.upgradeBase.name == "Map Player Tracker");
            if (upgradeItem != null && upgradeItem.GetConfig<bool>("Player Color"))
            {
                Plugin.instance.RemovePlayerFromMap(__instance);
                Plugin.instance.AddPlayerToMap(__instance);
            }
        }
    }
}
