using HarmonyLib;
using UnityEngine;

namespace MoreUpgrades.Patches
{
    [HarmonyPatch(typeof(Map))]
    internal class MapPatch
    {
        public static Camera mapCamera;
        public static float defaultMapZoom;

        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        static void Awake(ref Transform ___playerTransformTarget)
        {
            mapCamera = ___playerTransformTarget.GetComponentInChildren<Camera>();
            defaultMapZoom = mapCamera.orthographicSize;
        }
    }
}
