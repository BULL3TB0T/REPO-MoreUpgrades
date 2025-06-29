using HarmonyLib;
using MoreUpgrades.Classes;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace MoreUpgrades.Patches
{
    [HarmonyPatch(typeof(ItemUpgrade))]
    internal class ItemUpgradePatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void Start(ItemUpgrade __instance)
        {
            if (MoreUpgradesManager.instance == null)
                return;
            GameObject gameObject = __instance.gameObject;
            UpgradeItem upgradeItem = Plugin.instance.upgradeItems.FirstOrDefault(x => 
                x.fullName == gameObject.GetComponent<ItemAttributes>().item.itemAssetName);
            if (upgradeItem == null)
                return;
            __instance.upgradeEvent = new UnityEvent();
            __instance.upgradeEvent.AddListener(() =>
            {
                if (upgradeItem.HasConfig("Allow Team Upgrades") && upgradeItem.GetConfig<bool>("Allow Team Upgrades"))
                {
                    foreach (PlayerAvatar playerAvatar in SemiFunc.PlayerGetAll())
                        MoreUpgradesManager.instance.Upgrade(upgradeItem.name, SemiFunc.PlayerGetSteamID(playerAvatar));
                }
                else
                {
                    MoreUpgradesManager.instance.Upgrade(upgradeItem.name,
                        SemiFunc.PlayerGetSteamID(
                            SemiFunc.PlayerAvatarGetFromPhotonID(
                                (int)AccessTools.Field(typeof(ItemToggle), "playerTogglePhotonID")
                                .GetValue(gameObject.GetComponent<ItemToggle>()))));
                }
            });
        }
    }
}
