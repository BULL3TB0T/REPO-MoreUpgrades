using HarmonyLib;
using MoreUpgrades.Classes;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace MoreUpgrades.Patches
{
    [HarmonyPatch(typeof(StatsManager))]
    internal class StatsManagerPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        static void Start(StatsManager __instance)
        {
            foreach (UpgradeItem upgradeItem in Plugin.instance.upgradeItems)
            {
                __instance.dictionaryOfDictionaries.Add($"playerUpgradeModded{upgradeItem.saveName}", upgradeItem.playerUpgrades);
                __instance.dictionaryOfDictionaries.Add($"appliedPlayerUpgradeModded{upgradeItem.saveName}", upgradeItem.appliedPlayerUpgrades);
            }
        }

        [HarmonyPatch("FetchPlayerUpgrades")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> FetchPlayerUpgradesTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);
            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Ldloca_S),
                new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(KeyValuePair<string, Dictionary<string, int>>), "get_Key")),
                new CodeMatch(OpCodes.Ldstr, "playerUpgrade")
            );
            var kvpLocal = matcher.Operand;
            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(string), "Trim")),
                new CodeMatch(OpCodes.Stloc_S)
            );
            var textLocal = matcher.Operand;
            matcher.Advance(1);
            matcher.Insert(
                new CodeInstruction(OpCodes.Ldloca_S, kvpLocal),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(KeyValuePair<string, Dictionary<string, int>>), "get_Key")),
                new CodeInstruction(OpCodes.Ldloc_S, textLocal),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(StatsManagerPatch), "FetchPlayerUpgrades")),
                new CodeInstruction(OpCodes.Stloc_S, textLocal)
            );
            return matcher.InstructionEnumeration();
        }

        static string FetchPlayerUpgrades(string key, string text)
        {
            string prefix = "playerUpgradeModded";
            if (MoreUpgradesManager.instance == null || !key.StartsWith(prefix))
                return text;
            UpgradeItem upgradeItem = Plugin.instance.upgradeItems.FirstOrDefault(x => x.saveName == key.Substring(prefix.Length));
            if (upgradeItem == null) 
                return text;
            return upgradeItem.HasConfig("Display Name") ? upgradeItem.GetConfig<string>("Display Name") : upgradeItem.name;
        }
    }
}
