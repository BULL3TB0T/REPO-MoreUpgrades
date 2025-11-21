using CustomColors;
using HarmonyLib;
using MoreUpgrades.Classes;
using System.Linq;

namespace MoreUpgrades.CustomColorsPatches
{
    [HarmonyPatch(typeof(CustomColorsMod.ModdedColorPlayerAvatar))]
    internal class ModdedColorPlayerAvatarPatch
    {
        [HarmonyPatch("ModdedSetColorRPC")]
        [HarmonyPostfix]
        static void ModdedSetColorRPC(CustomColorsMod.ModdedColorPlayerAvatar __instance)
        {
            PlayerAvatar playerAvatar = __instance.avatar;
            if (MoreUpgradesManager.instance == null || playerAvatar == SemiFunc.PlayerAvatarLocal())
                return;
            UpgradeItem upgradeItem = Plugin.instance.upgradeItems.FirstOrDefault(x => x.upgradeBase.name == "Map Player Tracker");
            if (upgradeItem != null && upgradeItem.GetConfig<bool>("Player Color"))
            {
                Plugin.instance.RemovePlayerFromMap(playerAvatar);
                Plugin.instance.AddPlayerToMap(playerAvatar);
            }
        }
    }
}
