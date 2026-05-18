using HarmonyLib;
using MoreUpgrades.Classes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MoreUpgrades.Patches
{
    [HarmonyPatch(typeof(CosmeticWorldObject))]
    internal class CosmeticWorldObjectPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void Start(CosmeticWorldObject __instance, Color ___blinkMaterialDefault)
        {
            if (MoreUpgradesManager.instance == null)
                return;
            UpgradeItem upgradeItem = Plugin.instance.upgradeItems.FirstOrDefault(x => x.upgradeBase.name == "Map Cosmetics Tracker");
            if (upgradeItem == null)
                return;
            List<CosmeticLEDInfo> cosmeticLEDInfos = upgradeItem.GetVariable<List<CosmeticLEDInfo>>("Cosmetic LED Infos");
            foreach (CosmeticLEDInfo cosmeticLEDInfo in cosmeticLEDInfos)
            {
                if (cosmeticLEDInfo.isExtracted || cosmeticLEDInfo.isDestroyed || cosmeticLEDInfo.cosmeticWorldObject == null)
                {
                    cosmeticLEDInfo.isExtracted = false;
                    cosmeticLEDInfo.isDestroyed = false;
                    cosmeticLEDInfo.color = ___blinkMaterialDefault;
                    cosmeticLEDInfo.cosmeticWorldObject = __instance;
                    break;
                }
            }
        }

        [HarmonyPatch("ExtractRPC")]
        [HarmonyPostfix]
        static void ExtractRPC(CosmeticWorldObject __instance)
        {
            if (MoreUpgradesManager.instance == null)
                return;
            UpgradeItem upgradeItem = Plugin.instance.upgradeItems.FirstOrDefault(x => x.upgradeBase.name == "Map Cosmetics Tracker");
            if (upgradeItem == null)
                return;
            List<CosmeticLEDInfo> cosmeticLEDInfos = upgradeItem.GetVariable<List<CosmeticLEDInfo>>("Cosmetic LED Infos");
            foreach (CosmeticLEDInfo cosmeticLEDInfo in cosmeticLEDInfos)
            {
                if (cosmeticLEDInfo.cosmeticWorldObject == __instance)
                {
                    cosmeticLEDInfo.isExtracted = true;
                    break;
                }
            }
        }
    }
}
