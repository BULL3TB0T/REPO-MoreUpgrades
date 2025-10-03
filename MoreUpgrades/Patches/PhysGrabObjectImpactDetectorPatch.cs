using HarmonyLib;
using MoreUpgrades.Classes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MoreUpgrades.Patches
{
    [HarmonyPatch(typeof(PhysGrabObjectImpactDetector))]
    internal class PhysGrabObjectImpactDetectorPatch
    {
        [HarmonyPatch("Break")]
        [HarmonyPrefix]
        static void Break(PhysGrabObjectImpactDetector __instance, PhysGrabObject ___physGrabObject, 
            bool ___isValuable, ref float valueLost)
        {
            if (MoreUpgradesManager.instance == null || !___isValuable)
                return;
            UpgradeItem upgradeItem = Plugin.instance.upgradeItems.FirstOrDefault(x => x.upgradeItemBase.name == "Item Resist");
            if (upgradeItem == null)
                return;
            List<PhysGrabber> playerGrabbing = ___physGrabObject.playerGrabbing;
            if (playerGrabbing != null && playerGrabbing.Count > 0)
            {
                List<PlayerAvatar> playerAvatars = 
                    playerGrabbing.Select((PhysGrabber player) => player.playerAvatar).ToList();
                int level = 0;
                foreach (PlayerAvatar playerAvatar in playerAvatars)
                    level += upgradeItem.playerUpgrade.GetLevel(playerAvatar);
                valueLost = ReduceValueLost(upgradeItem, valueLost, level);
            }
            else
            {
                Dictionary<PhysGrabObject, PlayerAvatar> lastPlayerGrabbed = 
                    upgradeItem.GetVariable<Dictionary<PhysGrabObject, PlayerAvatar>>("Last Player Grabbed");
                if (lastPlayerGrabbed.TryGetValue(___physGrabObject, out PlayerAvatar playerAvatar))
                    valueLost = ReduceValueLost(upgradeItem, valueLost, upgradeItem.playerUpgrade.GetLevel(playerAvatar));
            }
        }

        private static float ReduceValueLost(UpgradeItem upgradeItem, float valueLost, int level) =>
            valueLost * Mathf.Pow(upgradeItem.GetConfig<float>("Scaling Factor"), level);
    }
}
