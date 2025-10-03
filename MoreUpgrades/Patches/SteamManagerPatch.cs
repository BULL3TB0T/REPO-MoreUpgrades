using HarmonyLib;
using MoreUpgrades.Classes;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace MoreUpgrades.Patches
{
    [HarmonyPatch(typeof(SteamManager))]
    internal class SteamManagerPatch
    {
        [HarmonyPatch("Awake")]
        [HarmonyPrefix]
        static void Awake(ref List<SteamManager.Developer> ___developerList)
        {
            var newDev = new SteamManager.Developer();
            newDev.name = "BULLETBOT";
            newDev.steamID = "76561198431789547";
            ___developerList.Add(newDev);
        }
    }
}
