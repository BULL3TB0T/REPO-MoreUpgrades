using HarmonyLib;
using MoreUpgrades.Classes;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace MoreUpgrades.Patches
{
    [HarmonyPatch(typeof(ValuableObject))]
    internal class ValuableObjectPatch
    {   
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void Start(ValuableObject __instance)
        {
            if (MoreUpgradesManager.instance == null)
                return;
            UpgradeItem upgradeItem = Plugin.instance.upgradeItems.FirstOrDefault(x => x.upgradeBase.name == "Valuable Count");
            if (upgradeItem == null || (__instance.GetComponent<SurplusValuable>() && upgradeItem.GetConfig<bool>("Ignore Money Bags")))
                return;
            List<ValuableObject> currentValuables = upgradeItem.GetVariable<List<ValuableObject>>("Current Valuables");
            if (!currentValuables.Contains(__instance))
                currentValuables.Add(__instance);
        }

        [HarmonyPatch("DollarValueSetLogic")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> DollarValueSetLogicTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions);
            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ValuableObject), "dollarValueOriginal")),
                new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(ValuableObject), "dollarValueCurrent"))
            );
            matcher.Insert(
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ValuableObjectPatch), "DollarValueSetLogic"))
            );
            return matcher.InstructionEnumeration();
        }

        private static float DollarValueSetLogic(float dollarValueCurrent)
        {
            if (MoreUpgradesManager.instance == null)
                return dollarValueCurrent;
            UpgradeItem upgradeItem = Plugin.instance.upgradeItems.FirstOrDefault(x => x.upgradeBase.name == "Item Value");
            if (upgradeItem == null)
                return dollarValueCurrent;
            int level = 0;
            foreach (PlayerAvatar playerAvatar in GameDirector.instance.PlayerList)
                level = upgradeItem.playerUpgrade.GetLevel(playerAvatar);
            return dollarValueCurrent * (1f + level * upgradeItem.GetConfig<float>("Scaling Factor"));
        }
    }
}
