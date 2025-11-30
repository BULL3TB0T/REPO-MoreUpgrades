using HarmonyLib;
using System.Linq;

namespace MoreUpgrades.Classes
{
    public static class MoreUpgradesAPI
    {
        public static float ItemValueMultiplier(Item item)
        {
            float defaultValue = 
                (float)AccessTools.Field(typeof(ShopManager), "itemValueMultiplier").GetValue(ShopManager.instance);
            if (MoreUpgradesManager.instance == null)
                return defaultValue;
            UpgradeItem upgradeItem = Plugin.instance.upgradeItems.FirstOrDefault(x => x.playerUpgrade.Item == item);
            if (upgradeItem == null)
                return defaultValue;
            float value = upgradeItem.GetConfig<float>("Price Multiplier");
            if (value < 0)
                value = defaultValue;
            return value;
        }

        public static float UpgradeValueIncrease(Item item)
        {
            float defaultValue = 
                (float)AccessTools.Field(typeof(ShopManager), "upgradeValueIncrease").GetValue(ShopManager.instance);
            if (MoreUpgradesManager.instance == null)
                return defaultValue;
            UpgradeItem upgradeItem = Plugin.instance.upgradeItems.FirstOrDefault(x => x.playerUpgrade.Item == item);
            if (upgradeItem == null)
                return defaultValue;
            float value = upgradeItem.GetConfig<float>("Price Increase Scaling");
            if (value < 0)
                value = defaultValue;
            return value;
        }
    }
}
