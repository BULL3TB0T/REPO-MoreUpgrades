using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace MoreUpgrades.Patches
{
    [HarmonyPatch(typeof(ItemAttributes))]
    internal class ItemAttributesPatch
    {
        [HarmonyPatch("GetValue")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> GetValueTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);
            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldsfld, AccessTools.Field(typeof(ShopManager), "instance")),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ShopManager), "itemValueMultiplier"))
            );
            matcher.Advance(1);
            matcher.Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ItemAttributes), "itemAssetName")),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Plugin), "ItemValueMultiplier"))
            );
            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldsfld, AccessTools.Field(typeof(ShopManager), "instance")),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ShopManager), "upgradeValueIncrease"))
            );
            matcher.Advance(1);
            matcher.Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ItemAttributes), "itemAssetName")),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Plugin), "UpgradeValueIncrease"))
            );
            return matcher.InstructionEnumeration();
        }
    }
}
