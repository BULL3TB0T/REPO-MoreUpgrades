using HarmonyLib;
using MoreUpgrades.Classes;

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
            Plugin.instance.RegisterToMap(__instance);
            Plugin.instance.ShowToMap(__instance);
        }

        [HarmonyPatch("ReviveRPC")]
        [HarmonyPostfix]
        static void ReviveRPC(PlayerAvatar __instance)
        {
            if (MoreUpgradesManager.instance == null)
                return;
            Plugin.instance.ShowToMap(__instance);
        }

        [HarmonyPatch("PlayerDeathRPC")]
        [HarmonyPostfix]
        static void PlayerDeathRPC(PlayerAvatar __instance)
        {
            if (MoreUpgradesManager.instance == null)
                return;
            Plugin.instance.HideFromMap(__instance);
        }
    }
}
