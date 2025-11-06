using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using REPOLib.Modules;
using System.Collections.Generic;
using UnityEngine;

namespace MoreUpgrades.Classes
{
    public class UpgradeItem
    {
        public bool isRepoLibImported;
        private string sectionName;
        public UpgradeItemBase upgradeItemBase;
        public PlayerUpgrade playerUpgrade;
        private Dictionary<string, ConfigEntryBase> configEntries;
        internal Dictionary<string, int> appliedPlayerDictionary;
        internal Dictionary<string, object> variables;

        public bool HasConfig(string key) => configEntries.TryGetValue(key, out ConfigEntryBase _);

        public bool AddConfig<T>(string key, T defaultValue, string description = "")
        {
            if (configEntries.ContainsKey(key))
            {
                Plugin.instance.logger.LogWarning($"A config entry with the key '{key}' already exists. Duplicates are not allowed.");
                return false;
            }
            if (upgradeItemBase.excludeConfigs.Contains(key))
                return false;
            ConfigEntryBase configEntryBase = null;
            if (defaultValue is int)
            {
                configEntryBase = Plugin.instance.Config.Bind(sectionName, key, defaultValue, 
                    new ConfigDescription(description, new AcceptableValueRange<int>(0, 1000)));
            }
            else if (defaultValue is float)
            {
                configEntryBase = Plugin.instance.Config.Bind(sectionName, key, defaultValue,
                    new ConfigDescription(description, new AcceptableValueRange<float>(-1, 100000)));
            }
            else
                configEntryBase = Plugin.instance.Config.Bind(sectionName, key, defaultValue, description);
            configEntries.Add(key, configEntryBase);
            return true;
        }

        public bool AddConfig<T>(string key, T defaultValue, ConfigDescription description = null)
        {
            if (configEntries.ContainsKey(key))
            {
                Plugin.instance.logger.LogWarning($"A config entry with the key '{key}' already exists. Duplicates are not allowed.");
                return false;
            }
            configEntries.Add(key, Plugin.instance.Config.Bind(sectionName, key, defaultValue, description));
            return true;
        }

        public bool AddConfig(string key, ConfigEntryBase value)
        {
            if (configEntries.ContainsKey(key))
            {
                Plugin.instance.logger.LogWarning($"A config entry with the key '{key}' already exists. Duplicates are not allowed.");
                return false;
            }
            configEntries.Add(key, value);
            return true;
        }

        public bool SetConfig<T>(string key, T value)
        {
            if (!configEntries.TryGetValue(key, out ConfigEntryBase entry))
            {
                Plugin.instance.logger.LogWarning($"A config entry with the key '{key}' does not exist. Returning default value.");
                return false;
            }
            if (entry is ConfigEntry<T> convertedEntry)
            {
                convertedEntry.Value = value;
                return true;
            }
            Plugin.instance.logger.LogWarning($"Type mismatch for config entry '{key}'." +
                $" Expected: {entry.SettingType.FullName}, but got: {typeof(T).FullName}. Returning default value.");
            return false;
        }

        public T GetConfig<T>(string key)
        {
            if (!configEntries.TryGetValue(key, out ConfigEntryBase value))
            {
                Plugin.instance.logger.LogWarning($"A config entry with the key '{key}' does not exist. Returning default value.");
                return default;
            }
            if (value is ConfigEntry<T> convertedValue)
                return convertedValue.Value;
            Plugin.instance.logger.LogWarning($"Type mismatch for config entry '{key}'." +
                $" Expected: {value.SettingType.FullName}, but got: {typeof(T).FullName}. Returning default value.");
            return default;
        }

        public bool HasVariable(string key) => variables.TryGetValue(key, out object _);

        public bool AddVariable<T>(string key, T value)
        {
            if (HasVariable(key))
            {
                Plugin.instance.logger.LogWarning($"A variable with the key '{key}' already exists. Duplicates are not allowed.");
                return false;
            }
            variables.Add(key, value);
            return true;
        }

        public bool SetVariable<T>(string key, T value)
        {
            if (!variables.TryGetValue(key, out object obj))
            {
                Plugin.instance.logger.LogWarning($"A variable with the key '{key}' does not exist.");
                return false;
            }
            if (obj is T)
            {
                variables[key] = value;
                return true;
            }
            Plugin.instance.logger.LogWarning($"Type mismatch for variable '{key}'." +
                $" Expected: {obj.GetType().FullName}, but got: {typeof(T).FullName}.");
            return false;
        }

        public T GetVariable<T>(string key)
        {
            if (!variables.TryGetValue(key, out object value))
            {
                Plugin.instance.logger.LogWarning($"A variable with the key '{key}' does not exist. Returning default value.");
                return default;
            }
            if (value is T convertedValue)
                return convertedValue;
            Plugin.instance.logger.LogWarning($"Type mismatch for variable '{key}'." +
                $" Expected: {value.GetType().FullName}, but got: {typeof(T).FullName}. Returning default value.");
            return default;
        }

        private void SetupConfig()
        {
            configEntries = new Dictionary<string, ConfigEntryBase>();
            AddConfig("Enabled", true, "Whether the upgrade item can be spawned to the shop.");
            AddConfig("Max Amount", upgradeItemBase.maxAmount, 
                "The maximum number of times the upgrade item can appear in the truck.");
            AddConfig("Max Amount In Shop", upgradeItemBase.maxAmountInShop,
                "The maximum number of times the upgrade item can appear in the shop.");
            AddConfig("Minimum Price", upgradeItemBase.minPrice, "The minimum cost to purchase the upgrade item.");
            AddConfig("Maximum Price", upgradeItemBase.maxPrice, "The maximum cost to purchase the upgrade item.");
            AddConfig("Price Increase Scaling", upgradeItemBase.priceIncreaseScaling,
                "The scale of the price increase based on the total number of upgrade item purchased." +
                "\nSet this value under 0 to use the default scaling.");
            AddConfig("Price Multiplier", isRepoLibImported ? -1f : 1f,
               "The multiplier of the price." +
               "\nSet this value under 0 to use the default multiplier.");
            AddConfig("Max Purchase Amount", upgradeItemBase.maxPurchaseAmount,
                "The maximum number of times the upgrade item can be purchased before it is no longer available in the shop." +
                "\nSet to 0 to disable the limit.");
            AddConfig("Allow Team Upgrades", false,
                "Whether the upgrade item applies to the entire team instead of just one player.");
            AddConfig("Sync Host Upgrades", false, "Whether the host should sync the item upgrade for the entire team.");
            AddConfig("Starting Amount", 0, "The number of times the upgrade item is applied at the start of the game.");
        }

        internal UpgradeItem(UpgradeItemBase upgradeItemBase)
        {
            sectionName = upgradeItemBase.name + (isRepoLibImported ? $" ({Compatibility.REPOLib.modGUID})" : "");
            this.upgradeItemBase = upgradeItemBase;
            appliedPlayerDictionary = new Dictionary<string, int>();
            variables = new Dictionary<string, object>();
            SetupConfig();
            Item item = Plugin.instance.assetBundle.LoadAsset<Item>(upgradeItemBase.name);
            string assetName = $"Modded Item Upgrade Player {upgradeItemBase.name}";
            item.name = assetName;
            item.itemName = $"{upgradeItemBase.name} Upgrade";
            item.maxAmount = HasConfig("Max Amount") ? GetConfig<int>("Max Amount") : upgradeItemBase.maxAmount;
            item.maxAmountInShop = HasConfig("Max Amount In Shop") ? GetConfig<int>("Max Amount In Shop") :
                upgradeItemBase.maxAmountInShop;
            item.maxPurchaseAmount = HasConfig("Max Purchase Amount") ? GetConfig<int>("Max Purchase Amount") :
                upgradeItemBase.maxPurchaseAmount;
            item.maxPurchase = item.maxPurchaseAmount > 0;
            Value value = ScriptableObject.CreateInstance<Value>();
            value.valueMin = HasConfig("Minimum Price") ? GetConfig<float>("Minimum Price") : upgradeItemBase.minPrice;
            value.valueMax = HasConfig("Maximum Price") ? GetConfig<float>("Maximum Price") : upgradeItemBase.maxPrice;
            item.value = value;
            GameObject prefab = Plugin.instance.assetBundle.LoadAsset<GameObject>($"{upgradeItemBase.name} Prefab");
            prefab.name = assetName;
            REPOLibItemUpgrade itemUpgrade = prefab.AddComponent<REPOLibItemUpgrade>();
            AccessTools.Field(typeof(REPOLibItemUpgrade), "_upgradeId").SetValue(itemUpgrade, upgradeItemBase.name);
            ItemAttributes itemAttributes = prefab.GetComponent<ItemAttributes>();
            itemAttributes.item = item;
            Items.RegisterItem(itemAttributes);
            playerUpgrade = Upgrades.RegisterUpgrade(upgradeItemBase.name, item, upgradeItemBase.onStart, upgradeItemBase.onUpgrade);
        }

        internal UpgradeItem(PlayerUpgrade playerUpgrade)
        {
            isRepoLibImported = true;
            Item item = playerUpgrade.Item;
            upgradeItemBase = new UpgradeItemBase
            {
                name = playerUpgrade.UpgradeId,
                maxAmount = item.maxAmount,
                maxAmountInShop = item.maxAmountInShop,
                maxPurchaseAmount = item.maxPurchaseAmount,
                minPrice = item.value.valueMin,
                maxPrice = item.value.valueMax
            };
            sectionName = $"{upgradeItemBase.name} ({Compatibility.REPOLib.modGUID})";
            appliedPlayerDictionary = new Dictionary<string, int>();
            SetupConfig();
            item.maxAmount = HasConfig("Max Amount") ? GetConfig<int>("Max Amount") : upgradeItemBase.maxAmount;
            item.maxAmountInShop = HasConfig("Max Amount In Shop") ? GetConfig<int>("Max Amount In Shop") : 
                upgradeItemBase.maxAmountInShop;
            item.maxPurchaseAmount = HasConfig("Max Purchase Amount") ? GetConfig<int>("Max Purchase Amount") : 
                upgradeItemBase.maxPurchaseAmount;
            item.maxPurchase = item.maxPurchaseAmount > 0;
            item.value.valueMin = HasConfig("Minimum Price") ? GetConfig<float>("Minimum Price") : upgradeItemBase.minPrice;
            item.value.valueMax = HasConfig("Maximum Price") ? GetConfig<float>("Maximum Price") : upgradeItemBase.maxPrice;
            this.playerUpgrade = playerUpgrade;
        }
    }
}
