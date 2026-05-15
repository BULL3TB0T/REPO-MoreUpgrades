using HarmonyLib;
using MoreUpgrades.Classes;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace MoreUpgrades.Patches
{
    [HarmonyPatch(typeof(UpgradeStand))]
    internal class UpgradeStandPatch
    {
        [HarmonyPatch("GetWeightedUpgradeExcluding")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> GetWeightedUpgradeExcludingTranspiler(
            IEnumerable<CodeInstruction> instructions)  
        {
            CodeMatcher matcher = new CodeMatcher(instructions);
            matcher.MatchForward(false, 
                new CodeMatch(OpCodes.Br),
                new CodeMatch(OpCodes.Ldloca_S),
                new CodeMatch(OpCodes.Call, 
                    AccessTools.Method(typeof(Dictionary<string, Item>.ValueCollection.Enumerator), "get_Current"))
            );
            var brLabel = matcher.Operand;
            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Call, 
                    AccessTools.Method(typeof(Dictionary<string, Item>.ValueCollection.Enumerator), "get_Current")),
                new CodeMatch(OpCodes.Stloc_3)
            );
            matcher.Advance(1);
            matcher.Insert(
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(UpgradeStandPatch),
                    "GetWeightedUpgradeExcluding")),
                new CodeInstruction(OpCodes.Brtrue, brLabel)
            );
            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldsfld, AccessTools.Field(typeof(ShopManager), "instance")),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ShopManager), "itemValueMultiplier"))
            );
            matcher.RemoveInstructions(2);
            matcher.Insert(
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MoreUpgradesAPI), "ItemValueMultiplier"))
            );
            return matcher.InstructionEnumeration();
        }

        static bool GetWeightedUpgradeExcluding(Item item)
        {
            if (MoreUpgradesManager.instance == null)
                return false;
            UpgradeItem upgradeItem = Plugin.instance.upgradeItems.FirstOrDefault(x => x.playerUpgrade.Item == item);
            return upgradeItem != null && !upgradeItem.GetConfig<bool>("Enabled");
        }
    }
}
