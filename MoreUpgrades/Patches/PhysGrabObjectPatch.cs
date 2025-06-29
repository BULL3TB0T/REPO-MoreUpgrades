using HarmonyLib;
using MoreUpgrades.Classes;
using System.Collections.Generic;
using System.Linq;

namespace MoreUpgrades.Patches
{
    [HarmonyPatch(typeof(PhysGrabObject))]
    internal class PhysGrabObjectPatch
    {   
        [HarmonyPatch("OnDestroy")]
        [HarmonyPostfix]
        static void OnDestroy(PhysGrabObject __instance)
        {
            if (MoreUpgradesManager.instance == null)
                return;
            ValuableObject valuableObject = __instance.gameObject.GetComponent<ValuableObject>();
            if (valuableObject == null)
                return;
            UpgradeItem upgradeItem = Plugin.instance.upgradeItems.FirstOrDefault(x => x.name == "Valuable Count");
            if (upgradeItem == null)
                return;
            List<ValuableObject> currentValuables = upgradeItem.GetVariable<List<ValuableObject>>("CurrentValuables");
            if (currentValuables.Contains(valuableObject))
                currentValuables.Remove(valuableObject);
        }
    }
}
