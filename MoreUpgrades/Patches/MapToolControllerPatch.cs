using HarmonyLib;
using MoreUpgrades.Classes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MoreUpgrades.Patches
{
    [HarmonyPatch(typeof(MapToolController))]
    internal class MapToolControllerPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void Start(Transform ___displaySpringTransform)
        {
            if (MoreUpgradesManager.instance == null)
                return;
            UpgradeItem upgradeItem = Plugin.instance.upgradeItems.FirstOrDefault(x => x.upgradeBase.name == "Map Cosmetics Tracker");
            if (upgradeItem == null)
                return;
            List<CosmeticLEDInfo> cosmeticLEDInfos = upgradeItem.GetVariable<List<CosmeticLEDInfo>>("Cosmetic LED Infos");
            float spacing = 0.025f;
            float center = 0.1f;
            int LEDCount = ValuableDirector.instance.cosmeticWorldObjectsLevelLoopsMax + 1;
            float startZ = center + ((LEDCount - 1) * spacing * 0.5f);
            GameObject originalLED = ___displaySpringTransform.Find("LED - L").gameObject;
            for (int i = 0; i < LEDCount; i++)
            {
                GameObject LED = Object.Instantiate(originalLED, ___displaySpringTransform, false);
                LED.name = "Left Side LED - Cosmetics Indicator " + (i + 1);
                Material material = Object.Instantiate(originalLED.GetComponent<Renderer>().material);
                material.name = "Dirt Finder - LED Cosmetics Indicator " + (i + 1);
                material.SetColor("_Color", Color.black);
                LED.GetComponent<Renderer>().material = material;
                LED.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
                LED.transform.localPosition = new Vector3(-0.0575f, 0, startZ - (i * spacing));
                cosmeticLEDInfos.Add(new CosmeticLEDInfo()
                {
                    ledObject = LED,
                    ledMaterial = material
                });
            }
        }
    }
}
