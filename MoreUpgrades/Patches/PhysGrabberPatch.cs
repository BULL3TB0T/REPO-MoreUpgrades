using HarmonyLib;
using MoreUpgrades.Classes;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MoreUpgrades.Patches
{
    [HarmonyPatch(typeof(PhysGrabber))]
    internal class PhysGrabberPatch
    {
        [HarmonyPatch("RayCheck")]
        [HarmonyPostfix]
        static void RayCheck(PlayerAvatar ___playerAvatar, LayerMask ___mask)
        {
            if (MoreUpgradesManager.instance == null || 
                PlayerController.instance.playerAvatarScript != ___playerAvatar ||
                (bool)AccessTools.Field(typeof(PlayerAvatar), "isDisabled").GetValue(___playerAvatar) ||
                (bool)AccessTools.Field(typeof(PlayerAvatar), "deadSet").GetValue(___playerAvatar) )
                return;
            UpgradeItem upgradeItem = Plugin.instance.upgradeItems.FirstOrDefault(x => x.upgradeBase.name == "Autoscan");
            if (upgradeItem == null)
                return;
            int level = upgradeItem.playerUpgrade.GetLevel(___playerAvatar);
            if (level <= 0) 
                return;
            Vector3 origin = ___playerAvatar.playerTransform.position;
            Collider[] colliders = Physics.OverlapSphere(origin, level * upgradeItem.GetConfig<float>("Scaling Factor"), 
                ___mask, QueryTriggerInteraction.Collide);
            foreach (Collider collider in colliders)
            {
                ValuableObject valuableObject = collider.transform.GetComponentInParent<ValuableObject>();
                if (valuableObject == null || (bool)AccessTools.Field(typeof(ValuableObject), "discovered").GetValue(valuableObject)) 
                    continue;
                if (upgradeItem.GetConfig<bool>("Silent Scanning"))
                {
                    if (!GameManager.Multiplayer())
                        AccessTools.Method(typeof(ValuableObject), "DiscoverRPC").Invoke(valuableObject, null);
                    else
                        ((PhotonView)AccessTools.Field(typeof(ValuableObject), "photonView").GetValue(valuableObject))
                            .RPC("DiscoverRPC", RpcTarget.All);
                }
                else 
                    valuableObject.Discover(ValuableDiscoverGraphic.State.Discover);
            }
        }
    }
}
