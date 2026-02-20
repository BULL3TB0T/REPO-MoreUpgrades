using HarmonyLib;
using MoreUpgrades.Classes;
using System.Collections.Generic;
using System.Linq;

namespace MoreUpgrades.Patches
{
    [HarmonyPatch(typeof(PhysGrabObject))]
    internal class PhysGrabObjectPatch
    {
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void Update(PhysGrabObject __instance, bool ___isValuable, PlayerAvatar ___lastPlayerGrabbing)
        {
            if (MoreUpgradesManager.instance == null || !___isValuable)
                return;
            UpgradeItem upgradeItem = Plugin.instance.upgradeItems.FirstOrDefault(x => x.upgradeBase.name == "Item Resist");
            if (upgradeItem == null || upgradeItem.GetConfig<string>("Exclude Valuables").Split(',').Select(x => x.Trim())
                    .Where(x => !string.IsNullOrEmpty(x)).Contains(__instance.name.Replace("(Clone)", "")))
                return;
            if (___lastPlayerGrabbing != null)
            {
                Dictionary<PhysGrabObject, PlayerAvatar> lastPlayerGrabbed =
                    upgradeItem.GetVariable<Dictionary<PhysGrabObject, PlayerAvatar>>("Last Player Grabbed");
                if (lastPlayerGrabbed.TryGetValue(__instance, out PlayerAvatar playerAvatar) && playerAvatar == ___lastPlayerGrabbing)
                    return;
                else if (lastPlayerGrabbed.ContainsKey(__instance))
                    lastPlayerGrabbed.Remove(__instance);
                lastPlayerGrabbed.Add(__instance, ___lastPlayerGrabbing);
            }
        }

        [HarmonyPatch("OnDestroy")]
        [HarmonyPostfix]
        static void OnDestroy(PhysGrabObject __instance)
        {
            if (MoreUpgradesManager.instance == null)
                return;
            ValuableObject valuableObject = __instance.gameObject.GetComponent<ValuableObject>();
            if (valuableObject == null)
                return;
            UpgradeItem upgradeItem = Plugin.instance.upgradeItems.FirstOrDefault(x => x.upgradeBase.name == "Valuable Count");
            if (upgradeItem == null || (__instance.GetComponent<SurplusValuable>() && upgradeItem.GetConfig<bool>("Ignore Money Bags")))
                return;
            List<ValuableObject> currentValuables = upgradeItem.GetVariable<List<ValuableObject>>("Current Valuables");
            if (currentValuables.Contains(valuableObject))
                currentValuables.Remove(valuableObject);
        }

        [HarmonyPatch("GrabStarted")]
        [HarmonyPostfix]
        static void GrabStarted(PhysGrabObject __instance, bool ___grabbedLocal, bool ___isValuable)
        {
            if (MoreUpgradesManager.instance == null || !___grabbedLocal || !___isValuable)
                return;
            UpgradeItem upgradeItem = Plugin.instance.upgradeItems.FirstOrDefault(x => x.upgradeBase.name == "Item Resist");
            if (upgradeItem == null || !upgradeItem.GetConfig<bool>("Print Valuables"))
                return;
            Plugin.instance.logger.LogMessage("Grabbed Valuable Name: " + __instance.name.Replace("(Clone)", ""));
        }
    }
}
