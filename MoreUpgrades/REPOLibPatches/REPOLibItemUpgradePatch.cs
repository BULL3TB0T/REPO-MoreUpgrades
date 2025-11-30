using HarmonyLib;
using MoreUpgrades.Classes;
using REPOLib.Modules;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace MoreUpgrades.REPOLibPatches
{
    [HarmonyPatch(typeof(REPOLibItemUpgrade))]
    internal class REPOLibItemUpgradePatch
    {
        [HarmonyPatch("Upgrade")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> UpgradeTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions);
            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(PlayerUpgrade), "AddLevel", 
                    new[] { typeof(PlayerAvatar), typeof(int) }))
            );
            matcher.RemoveInstructions(2);
            matcher.Insert(
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(REPOLibItemUpgradePatch), "Upgrade"))
            );
            return matcher.InstructionEnumeration();
        }

        static void Upgrade(PlayerUpgrade playerUpgrade, PlayerAvatar playerAvatar, int amount)
        {
            if (MoreUpgradesManager.instance != null)
            {
                UpgradeItem upgradeItem = Plugin.instance.upgradeItems.FirstOrDefault(x =>
                    x.playerUpgrade.Item == playerUpgrade.Item);
                if (upgradeItem != null && upgradeItem.GetConfig<bool>("Allow Team Upgrades"))
                {
                    foreach (PlayerAvatar currentPlayerAvatar in SemiFunc.PlayerGetAll())
                        playerUpgrade.AddLevel(currentPlayerAvatar, amount);
                    return;
                }
            }
            playerUpgrade.AddLevel(playerAvatar, amount);
        }
    }
}
