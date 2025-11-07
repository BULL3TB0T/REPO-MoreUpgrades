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
        static IEnumerable<CodeInstruction> GetAllItemsFromStatsManagerTranspiler(IEnumerable<CodeInstruction> instructions)
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
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ShopManagerPatch), "GetAllItemsFromStatsManager")),
                new CodeInstruction(OpCodes.Brtrue, brLabel)
            );
            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ShopManager), "itemValueMultiplier"))
            );
            matcher.Advance(1);
            matcher.Insert(
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Plugin), "ItemValueMultiplier"))
            );
            return matcher.InstructionEnumeration();
        }

        static bool GetAllItemsFromStatsManager(Item item)
        {
            if (MoreUpgradesManager.instance == null)
                return false;
            UpgradeItem upgradeItem = Plugin.instance.upgradeItems.FirstOrDefault(x => x.playerUpgrade.Item == item);
            return upgradeItem != null && upgradeItem.HasConfig("Enabled") && !upgradeItem.GetConfig<bool>("Enabled");
        }

        [HarmonyPatch("UpgradeValueGet")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> UpgradeValueGetTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions);
            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ShopManager), "upgradeValueIncrease"))
            );
            matcher.Advance(1);
            matcher.Insert(
                new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ShopManagerPatch), "UpgradeValueIncrease"))
            );
            return matcher.InstructionEnumeration();
        }

        internal static float UpgradeValueIncrease(float upgradeValueIncrease, Item item)
        {
            if (MoreUpgradesManager.instance == null)
                return upgradeValueIncrease;
            UpgradeItem upgradeItem = Plugin.instance.upgradeItems.FirstOrDefault(x => x.playerUpgrade.Item == item);
            if (upgradeItem == null)
                return upgradeValueIncrease;
            float value = upgradeItem.HasConfig("Price Increase Scaling") ? 
                upgradeItem.GetConfig<float>("Price Increase Scaling") : upgradeItem.upgradeBase.priceIncreaseScaling;
            if (value < 0)
                value = upgradeValueIncrease;
            return value;
        }
    }
}
