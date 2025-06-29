using BepInEx;
using BepInEx.Configuration;
using REPOLib.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MoreUpgrades.Classes
{
    public class UpgradeItem
    {
        public string name { get; private set; }
        public string fullName { get; private set; }
        public string modGUID { get; private set; }
        internal string saveName;
        internal string sectionName;
        internal UpgradeItemBase upgradeItemBase;
        private Dictionary<string, ConfigEntryBase> configEntries;
        internal Dictionary<string, int> playerUpgrades;
        internal Dictionary<string, int> appliedPlayerUpgrades;
        internal Dictionary<string, object> variables;
        public Action onInit;
        public Action onLateUpdate;
        public Action onFixedUpdate;
        public Action onUpdate;
        public Action onChanged;

        public bool HasConfig(string key) => configEntries.TryGetValue(key, out ConfigEntryBase _);

        public bool AddConfig<T>(string key, T defaultValue, string description = "")
        {
            if (configEntries.ContainsKey(key))
            {
                Plugin.instance.logger.LogWarning($"A config entry with the key '{key}' already exists. Duplicates are not allowed.");
                return false;
            }
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

        public int GetAmount(string steamId = null)
        {
            if (steamId != null)
                return playerUpgrades.ContainsKey(steamId) ? playerUpgrades[steamId] : 0;
            PlayerAvatar playerAvatar = SemiFunc.PlayerAvatarLocal();
            if (playerAvatar != null)
                steamId = SemiFunc.PlayerGetSteamID(playerAvatar);
            return steamId != null && playerUpgrades.ContainsKey(steamId) ? playerUpgrades[steamId] : 0;
        }

        internal UpgradeItem(UpgradeItemBase upgradeItemBase, string modGUID = null, Item modItem = null, GameObject modPrefab = null)
        {
            name = upgradeItemBase.name;
            fullName = $"Modded Item Upgrade Player {name}";
            this.modGUID = modGUID;
            saveName = new string(name.Where(x => !char.IsWhiteSpace(x)).ToArray());
            sectionName = name + (!modGUID.IsNullOrWhiteSpace() ? $" ({modGUID})" : "");
            this.upgradeItemBase = upgradeItemBase;
            configEntries = new Dictionary<string, ConfigEntryBase>();
            playerUpgrades = new Dictionary<string, int>();
            appliedPlayerUpgrades = new Dictionary<string, int>();
            variables = new Dictionary<string, object>();
            void TryAddConfig<T>(string key, T defaultValue, string description = "")
            {
                if (!upgradeItemBase.excludeConfigs.Contains(key))
                    AddConfig(key, defaultValue, description);
            }
            TryAddConfig("Enabled", true, "Whether the upgrade item can be spawned to the shop.");
            TryAddConfig("Max Amount", upgradeItemBase.maxAmount, "The maximum number of times the upgrade item can appear in the truck.");
            TryAddConfig("Max Amount In Shop", upgradeItemBase.maxAmountInShop, 
                "The maximum number of times the upgrade item can appear in the shop.");
            TryAddConfig("Minimum Price", upgradeItemBase.minPrice, "The minimum cost to purchase the upgrade item.");
            TryAddConfig("Maximum Price", upgradeItemBase.maxPrice, "The maximum cost to purchase the upgrade item.");
            TryAddConfig("Price Increase Scaling", upgradeItemBase.priceIncreaseScaling,
                "The scale of the price increase based on the total number of upgrade item purchased." +
                "\nDefault scaling is set to 0.5. " +
                "(Note: Other mods may modify this value, affecting the game's default scaling.)" +
                "\nSet to -1 to use the default scaling.");
            TryAddConfig("Max Purchase Amount", upgradeItemBase.maxPurchaseAmount,
                "The maximum number of times the upgrade item can be purchased before it is no longer available in the shop." +
                "\nSet to 0 to disable the limit.");
            TryAddConfig("Allow Team Upgrades", false, 
                "Whether the upgrade item applies to the entire team instead of just one player.");
            TryAddConfig("Sync Host Upgrades", false, "Whether the host should sync the item upgrade for the entire team.");
            TryAddConfig("Starting Amount", 0, "The number of times the upgrade item is applied at the start of the game.");
            TryAddConfig("Display Name", name, "The display name of the upgrade item.");
            Item item = modItem ?? Plugin.instance.assetBundle.LoadAsset<Item>(name);
            item.name = fullName;
            item.itemAssetName = fullName;
            item.itemName = $"{(HasConfig("Display Name") ? GetConfig<string>("Display Name") : name)} Upgrade";
            item.maxAmount = HasConfig("Max Amount") ? GetConfig<int>("Max Amount") : upgradeItemBase.maxAmount;
            item.maxAmountInShop = HasConfig("Max Amount In Shop") ? GetConfig<int>("Max Amount In Shop") : upgradeItemBase.maxAmountInShop;
            item.maxPurchaseAmount = HasConfig("Max Purchase Amount") ? GetConfig<int>("Max Purchase Amount") : upgradeItemBase.maxPurchaseAmount;
            item.maxPurchase = item.maxPurchaseAmount > 0;
            item.value = ScriptableObject.CreateInstance<Value>();
            item.value.valueMin = HasConfig("Minimum Price") ? GetConfig<float>("Minimum Price") : upgradeItemBase.minPrice;
            item.value.valueMax = HasConfig("Maximum Price") ? GetConfig<float>("Maximum Price") : upgradeItemBase.maxPrice;
            GameObject prefab = modPrefab ?? Plugin.instance.assetBundle.LoadAsset<GameObject>($"{name} Prefab");
            prefab.name = fullName;
            prefab.GetComponent<ItemAttributes>().item = item;
            item.prefab = prefab;
            Items.RegisterItem(item);
        }
    }
}
