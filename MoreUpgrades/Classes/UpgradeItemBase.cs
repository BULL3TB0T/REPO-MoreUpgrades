using System.Collections.Generic;

namespace MoreUpgrades.Classes
{
    public class UpgradeItemBase
    {
        public string name = null;
        public int maxAmount = 1;
        public int maxAmountInShop = 1;
        public float minPrice = 1000;
        public float maxPrice = 1000;
        public int maxPurchaseAmount = 0;
        public float priceIncreaseScaling = -1f;
        public List<string> excludeConfigs = new List<string>();
    }
}
