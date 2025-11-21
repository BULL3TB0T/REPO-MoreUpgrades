using HarmonyLib;
using MoreUpgrades.Classes;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace MoreUpgrades.Patches
{
    [HarmonyPatch(typeof(MissionUI))]
    internal class MissionUIPatch
    {
        [HarmonyPatch("MissionText")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> MissionTextTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions);
            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(MissionUI), "messagePrev"))
            );
            matcher.Advance(1);
            matcher.Insert(
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MissionUIPatch), "MissionText"))
            );
            return matcher.InstructionEnumeration();
        }

        static void MissionText()
        {
            if (MoreUpgradesManager.instance == null)
                return;
            UpgradeItem upgradeItem = Plugin.instance.upgradeItems.FirstOrDefault(x => x.upgradeBase.name == "Valuable Count");
            if (upgradeItem == null)
                return;
            upgradeItem.SetVariable("Changed", true);
        }
    }
}
