using HarmonyLib;
using MoreUpgrades.Classes;
using System.Collections.Generic;
using System.Linq;

namespace MoreUpgrades.Patches
{
    [HarmonyPatch(typeof(ValuableObject))]
    internal class ValuableObjectPatch
    {   
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void Start(ValuableObject __instance)
        {
            if (MoreUpgradesManager.instance == null)
                return;
            UpgradeItem upgradeItem = Plugin.instance.upgradeItems.FirstOrDefault(x => x.name == "Valuable Count");
            if (upgradeItem == null)
                return;
            List<ValuableObject> currentValuables = upgradeItem.GetVariable<List<ValuableObject>>("CurrentValuables");
            if (!currentValuables.Contains(__instance))
                currentValuables.Add(__instance);
        }
    }
}
