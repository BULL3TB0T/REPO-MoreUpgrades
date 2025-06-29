using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MoreUpgrades.Classes
{
    public static class MoreUpgradesLib
    {
        public static bool IsManagerActive() => MoreUpgradesManager.instance != null;

        public static IReadOnlyList<UpgradeItem> GetCoreUpgradeItems()
            => Plugin.instance.upgradeItems.Where(x => x.modGUID == null).ToList();

        public static IReadOnlyList<UpgradeItem> GetUpgradeItemsByMod(string modGUID) =>
            Plugin.instance.upgradeItems.Where(x => x.modGUID == modGUID).ToList();

        public static UpgradeItem Register(string modGUID, Item item, GameObject prefab, UpgradeItemBase upgradeItemBase)
        {
            if (string.IsNullOrEmpty(modGUID))
            {
                Plugin.instance.logger.LogWarning("Couldn't register the upgrade item because the modGUID is not valid.");
                return null;
            }
            if (Plugin.instance.assetBundle == null)
            {
                Plugin.instance.logger.LogWarning($"{modGUID}: Couldn't register the upgrade item because the core mod failed to start." +
                    " Contact the mod creator.");
                return null;
            }
            if (item == null || prefab == null)
            {
                Plugin.instance.logger.LogWarning($"{modGUID}: Couldn't register the upgrade item because the item or prefab are not valid.");
                return null;
            }
            string name = upgradeItemBase.name;
            if (string.IsNullOrEmpty(name))
            {
                Plugin.instance.logger.LogWarning($"{modGUID}: Couldn't register the upgrade item because the base name is not valid.");
                return null;
            }
            if (Plugin.instance.upgradeItems.Any(x => x.name.ToLower().Trim() == name.ToLower().Trim()))
            {
                Plugin.instance.logger.LogWarning($"{modGUID}: An upgrade item with the name '{name.Trim()}' already exists." +
                    " Duplicate upgrade items are not allowed.");
                return null;
            }
            UpgradeItem upgradeItem = new UpgradeItem(upgradeItemBase, modGUID, item, prefab);
            Plugin.instance.upgradeItems.Add(upgradeItem);
            return upgradeItem;
        }
    }
}
