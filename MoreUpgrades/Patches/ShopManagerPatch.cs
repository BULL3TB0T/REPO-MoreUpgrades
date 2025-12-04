using HarmonyLib;
using MoreUpgrades.Classes;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace MoreUpgrades.Patches
{
    [HarmonyPatch(typeof(ShopManager))]
    internal class ShopManagerPatch
    {
        [HarmonyPatch("GetAllItemsFromStatsManager")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> GetAllItemsFromStatsManagerTranspiler(
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
                new CodeMatch(OpCodes.Stloc_1)
            );
            matcher.Advance(1);
            matcher.Insert(
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ShopManagerPatch), 
                    "GetAllItemsFromStatsManager")),
                new CodeInstruction(OpCodes.Brtrue, brLabel)
            );
            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ShopManager), "itemValueMultiplier"))
            );
            matcher.RemoveInstructions(2);
            matcher.Insert(
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MoreUpgradesAPI), "ItemValueMultiplier"))
            );
            return matcher.InstructionEnumeration();
        }

        static bool GetAllItemsFromStatsManager(Item item)
        {
            if (MoreUpgradesManager.instance == null)
                return false;
            UpgradeItem upgradeItem = Plugin.instance.upgradeItems.FirstOrDefault(x => x.playerUpgrade.Item == item);
            return upgradeItem != null && !upgradeItem.GetConfig<bool>("Enabled");
        }

        [HarmonyPatch("UpgradeValueGet")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> UpgradeValueGetTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions);
            CodeMatch[] codeMatches = new CodeMatch[]
            {
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ShopManager), "upgradeValueIncrease"))
            };
            matcher.MatchForward(false, codeMatches);
            matcher.RemoveInstructions(codeMatches.Length);
            matcher.Insert(
                new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MoreUpgradesAPI), "UpgradeValueIncrease"))
            );
            return matcher.InstructionEnumeration();
        }
    }
}
