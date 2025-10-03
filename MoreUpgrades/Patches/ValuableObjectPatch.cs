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
            UpgradeItem upgradeItem = Plugin.instance.upgradeItems.FirstOrDefault(x => x.upgradeItemBase.name == "Valuable Count");
            if (upgradeItem == null || (__instance.GetComponent<SurplusValuable>() && upgradeItem.GetConfig<bool>("Ignore Money Bags")))
                return;
            List<ValuableObject> currentValuables = upgradeItem.GetVariable<List<ValuableObject>>("Current Valuables");
            if (!currentValuables.Contains(__instance))
                currentValuables.Add(__instance);
        }
    }
}
