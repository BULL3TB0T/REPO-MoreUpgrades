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
            var matcher = new CodeMatcher(instructions);
            matcher.MatchForward(false, 
                new CodeMatch(OpCodes.Br),
                new CodeMatch(OpCodes.Ldloca_S),
                new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Dictionary<string, Item>.ValueCollection.Enumerator), "get_Current"))
            );
            var brLabel = matcher.Operand;
            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Dictionary<string, Item>.ValueCollection.Enumerator), "get_Current")),
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
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Item), "itemAssetName")),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Plugin), "ItemValueMultiplier"))
            );
            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ShopManager), "upgradeValueIncrease"))
            );
            matcher.Advance(1);
            matcher.Insert(
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Item), "itemAssetName")),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Plugin), "UpgradeValueIncrease"))
            );
            return matcher.InstructionEnumeration();
        }

        static bool GetAllItemsFromStatsManager(Item item)
        {
            if (MoreUpgradesManager.instance == null)
                return false;
            UpgradeItem upgradeItem = Plugin.instance.upgradeItems.FirstOrDefault(x => x.fullName == item.itemAssetName);
            return upgradeItem != null && upgradeItem.HasConfig("Enabled") && !upgradeItem.GetConfig<bool>("Enabled");
        }
    }
}
