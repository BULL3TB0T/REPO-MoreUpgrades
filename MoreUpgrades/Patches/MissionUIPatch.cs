using HarmonyLib;
using MoreUpgrades.Classes;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace MoreUpgrades.Patches
{
    [HarmonyPatch(typeof(MissionUIPatch))]
    internal class MissionUIPatch
    {
        [HarmonyPatch(typeof(MissionUI), "MissionText")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                    .MatchForward(false,
                        new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(MissionUI), "messagePrev")))
                    .Advance(1)
                    .Insert(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MissionUIPatch), "MissionText")))
                    .InstructionEnumeration();
        }

        static void MissionText()
        {
            if (MoreUpgradesManager.instance == null)
                return;
            UpgradeItem upgradeItem = Plugin.instance.upgradeItems.FirstOrDefault(x => x.name == "Valuable Count");
            if (upgradeItem == null)
                return;
            upgradeItem.SetVariable("Changed", true);
        }
    }
}
