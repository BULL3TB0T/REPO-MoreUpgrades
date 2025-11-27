using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using KeybindLib.Classes;
using MoreUpgrades.Classes;
using MoreUpgrades.Patches;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoreUpgrades
{
    [BepInDependency(Compatibility.REPOLib.modGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(Compatibility.KeybindLib.modGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(Compatibility.CustomColors.modGUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(modGUID, modName, modVer)]
    internal class Plugin : BaseUnityPlugin
    {
        private const string modGUID = "bulletbot.moreupgrades";
        private const string modName = "MoreUpgrades";
        private const string modVer = "1.5.8";

        internal static Plugin instance;
        internal ManualLogSource logger;
        private readonly Harmony harmony = new Harmony(modGUID);

        internal ConfigEntry<bool> importUpgrades;
        internal ConfigEntry<string> excludeUpgradeIds;

        internal AssetBundle assetBundle;
        internal List<UpgradeItem> upgradeItems;

        private void PatchAll(string name)
        {
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Namespace == $"MoreUpgrades.{name}"))
                harmony.PatchAll(type);
        }

        private GameObject GetVisualsFromComponent(Component component)
        {
            GameObject visuals = null;
            if (component.GetType() == typeof(EnemyParent))
            {
                EnemyParent enemyParent = component as EnemyParent;
                Enemy enemy = (Enemy)AccessTools.Field(typeof(EnemyParent), "Enemy").GetValue(component);
                if (enemyParent.enemyName != "Bella")
                {
                    try
                    {
                        visuals = enemyParent.GetComponentInChildren<EnemyVision>().VisionTransform.gameObject;
                    }
                    catch { }
                    if (visuals == null)
                    {
                        try
                        {
                            visuals = enemyParent.EnableObject.GetComponentInChildren<Animator>().gameObject;
                        }
                        catch { }
                    }
                }
                else
                    visuals = enemy.GetComponentInChildren<EnemyTricycle>().followTargetTransform.gameObject;
                if (visuals == null)
                    visuals = enemy.gameObject;
            }
            else if (component.GetType() == typeof(PlayerAvatar))
            {
                PlayerAvatar playerAvatar = component as PlayerAvatar;
                visuals = playerAvatar.playerAvatarVisuals.gameObject;
            }
            return visuals;
        }

        internal void AddEnemyToMap(Component component, string enemyName = null)
        {
            UpgradeItem upgradeItem = upgradeItems.FirstOrDefault(x => x.upgradeBase.name == "Map Enemy Tracker");
            if (upgradeItem == null)
                return;
            if (component is EnemyParent enemyParent && enemyName == null)
                enemyName = enemyParent.enemyName;
            if (upgradeItem.GetConfig<string>("Exclude Enemies").Split(',').Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x)).Contains(enemyName))
                return;
            GameObject visuals = GetVisualsFromComponent(component);
            List<(GameObject, Color)> addToMap = upgradeItem.GetVariable<List<(GameObject, Color)>>("Add To Map");
            List<GameObject> removeFromMap = upgradeItem.GetVariable<List<GameObject>>("Remove From Map");
            if (visuals == null || addToMap.Any(x => x.Item1 == visuals))
                return;
            if (removeFromMap.Contains(visuals))
                removeFromMap.Remove(visuals);
            addToMap.Add((visuals, upgradeItem.GetConfig<Color>("Color")));
        }

        internal void RemoveEnemyFromMap(Component component, string enemyName = null)
        {
            UpgradeItem upgradeItem = upgradeItems.FirstOrDefault(x => x.upgradeBase.name == "Map Enemy Tracker");
            if (upgradeItem == null)
                return;
            if (component is EnemyParent enemyParent && enemyName == null)
                enemyName = enemyParent.enemyName;
            if (upgradeItem.GetConfig<string>("Exclude Enemies").Split(',').Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x)).Contains(enemyName))
                return;
            GameObject visuals = GetVisualsFromComponent(component);
            List<(GameObject, Color)> addToMap = upgradeItem.GetVariable<List<(GameObject, Color)>>("Add To Map");
            List<GameObject> removeFromMap = upgradeItem.GetVariable<List<GameObject>>("Remove From Map");
            if (visuals == null || removeFromMap.Contains(visuals))
                return;
            if (addToMap.Any(x => x.Item1 == visuals))
                addToMap.RemoveAll(x => x.Item1 == visuals);
            removeFromMap.Add(visuals);
        }

        internal void AddPlayerToMap(PlayerAvatar playerAvatar)
        {
            UpgradeItem upgradeItem = upgradeItems.FirstOrDefault(x => x.upgradeBase.name == "Map Player Tracker");
            if (upgradeItem == null)
                return;
            GameObject visuals = GetVisualsFromComponent(playerAvatar);
            List<(GameObject, Color)> addToMap = upgradeItem.GetVariable<List<(GameObject, Color)>>("Add To Map");
            List<GameObject> removeFromMap = upgradeItem.GetVariable<List<GameObject>>("Remove From Map");
            if (visuals == null || addToMap.Any(x => x.Item1 == visuals))
                return;
            if (removeFromMap.Contains(visuals))
                removeFromMap.Remove(visuals);
            Color color = upgradeItem.GetConfig<Color>("Color");
            if (upgradeItem.GetConfig<bool>("Player Color"))
                color = (Color)AccessTools.Field(typeof(PlayerAvatarVisuals), "color").GetValue(playerAvatar.playerAvatarVisuals);
            addToMap.Add((visuals, color));
        }

        internal void RemovePlayerFromMap(PlayerAvatar playerAvatar)
        {
            UpgradeItem upgradeItem = upgradeItems.FirstOrDefault(x => x.upgradeBase.name == "Map Player Tracker");
            if (upgradeItem == null)
                return;
            GameObject visuals = GetVisualsFromComponent(playerAvatar);
            List<(GameObject, Color)> addToMap = upgradeItem.GetVariable<List<(GameObject, Color)>>("Add To Map");
            List<GameObject> removeFromMap = upgradeItem.GetVariable<List<GameObject>>("Remove From Map");
            if (visuals == null || removeFromMap.Contains(visuals))
                return;
            if (addToMap.Any(x => x.Item1 == visuals))
                addToMap.RemoveAll(x => x.Item1 == visuals);
            removeFromMap.Add(visuals);
        }

        private static float ItemValueMultiplier(float itemValueMultiplier, Item item)
        {
            if (MoreUpgradesManager.instance == null)
                return itemValueMultiplier;
            UpgradeItem upgradeItem = instance.upgradeItems.FirstOrDefault(x => x.playerUpgrade.Item == item);
            if (upgradeItem == null)
                return itemValueMultiplier;
            float value = upgradeItem.HasConfig("Price Multiplier") ? upgradeItem.GetConfig<float>("Price Multiplier") :
                itemValueMultiplier;
            if (value < 0)
                value = itemValueMultiplier;
            return value;
        }
        
        void Awake()
        {
            instance = this;
            logger = BepInEx.Logging.Logger.CreateLogSource(modName);
            assetBundle = AssetBundle.LoadFromMemory(Properties.Resources.moreupgrades);
            if (assetBundle == null)
            {
                logger.LogError("Something went wrong when loading the asset bundle.");
                return;
            }
            importUpgrades = Config.Bind("! REPOLib Configuration !", "Import Upgrades", false, 
                "Whether to import the upgrades from REPOLib.");
            excludeUpgradeIds = Config.Bind("! REPOLib Configuration !", "Exclude Upgrade IDs", "",
                "Exclude specific REPOLib upgrades by listing their IDs, seperated by commas." +
                "\nThis setting only has an effect if 'Import Upgrades' is enabled.");
            upgradeItems = new List<UpgradeItem>();
            UpgradeItem.Base sprintUsageBase = new UpgradeItem.Base
            {
                name = "Sprint Usage",
                maxAmount = 10,
                maxAmountInShop = 2,
                minPrice = 9000,
                maxPrice = 14000
            };
            UpgradeItem sprintUsage = null;
            void UpdateSprintUsage(PlayerAvatar playerAvatar, int level)
            {
                if (PlayerController.instance.playerAvatarScript != playerAvatar)
                    return;
                string key = "Energy Sprint Drain";
                if (!sprintUsage.HasVariable(key))
                    sprintUsage.AddVariable(key, PlayerController.instance.EnergySprintDrain);
                PlayerController.instance.EnergySprintDrain =
                    sprintUsage.GetVariable<float>(key) * Mathf.Pow(sprintUsage.GetConfig<float>("Scaling Factor"), level);
            }
            sprintUsageBase.onStart += UpdateSprintUsage;
            sprintUsageBase.onUpgrade += UpdateSprintUsage;
            sprintUsage = new UpgradeItem(sprintUsageBase);
            sprintUsage.AddConfig("Scaling Factor", 0.9f,
                "Formula: energySprintDrain * (scalingFactor ^ upgradeLevel))");
            upgradeItems.Add(sprintUsage);
            UpgradeItem.Base valuableCountBase = new UpgradeItem.Base
            {
                name = "Valuable Count",
                minPrice = 30000,
                maxPrice = 40000,
                maxPurchaseAmount = 1,
                priceIncreaseScaling = 0
            };
            UpgradeItem valuableCount = null;
            valuableCountBase.onVariablesStart += delegate
            {
                valuableCount.AddVariable("Current Valuables", new List<ValuableObject>());
                valuableCount.AddVariable("Changed", false);
                valuableCount.AddVariable("Previous Count", 0);
                valuableCount.AddVariable("Previous Value", 0);
                valuableCount.AddVariable("Text Length", 0);
            };
            valuableCountBase.onUpdate += delegate
            {
                if (SemiFunc.RunIsLobby() || SemiFunc.RunIsShop())
                    return;
                PlayerAvatar playerAvatar = PlayerController.instance.playerAvatarScript;
                if (MissionUI.instance != null && playerAvatar != null && valuableCount.playerUpgrade.GetLevel(playerAvatar) > 0)
                {
                    TextMeshProUGUI missionText = 
                        (TextMeshProUGUI)AccessTools.Field(typeof(MissionUI), "Text").GetValue(MissionUI.instance);
                    string messagePrev = 
                        (string)AccessTools.Field(typeof(MissionUI), "messagePrev").GetValue(MissionUI.instance);
                    List<ValuableObject> currentValuables = 
                        valuableCount.GetVariable<List<ValuableObject>>("Current Valuables");
                    bool changed = valuableCount.GetVariable<bool>("Changed");
                    int previousCount = valuableCount.GetVariable<int>("Previous Count");
                    int previousValue = valuableCount.GetVariable<int>("Previous Value");
                    int textLength = valuableCount.GetVariable<int>("Text Length");
                    int count = currentValuables.Count;
                    bool displayTotalValue = valuableCount.GetConfig<bool>("Display Total Value");
                    int value = displayTotalValue ? currentValuables.Select(x =>
                    {
                        return (int)((float)AccessTools.Field(typeof(ValuableObject), "dollarValueCurrent").GetValue(x));
                    }).Sum() : 0;
                    if (!missionText.text.IsNullOrWhiteSpace() && (changed || previousCount != count || previousValue != value))
                    {
                        string text = missionText.text;
                        if (!changed && (previousCount != count || previousValue != value))
                            text = text.Substring(0, text.Length - textLength);
                        string valuableText = $"\nValuables: <b>{count}</b>" +
                            (displayTotalValue ? 
                                $" (<color=#558B2F>$</color><b>{SemiFunc.DollarGetString(value)}</b>)" : "");
                        text += valuableText;
                        valuableCount.SetVariable("Previous Count", count);
                        valuableCount.SetVariable("Previous Value", value);
                        valuableCount.SetVariable("Text Length", valuableText.Length);
                        missionText.text = text;
                        AccessTools.Field(typeof(MissionUI), "messagePrev").SetValue(MissionUI.instance, text);
                        if (changed)
                            valuableCount.SetVariable("Changed", false);
                    }
                }
            };
            valuableCount = new UpgradeItem(valuableCountBase);
            valuableCount.AddConfig("Display Total Value", true, 
                "Whether to display the total value next to the valuable counter.");
            valuableCount.AddConfig("Ignore Money Bags", false,
                "Whether to ignore the money bags from the extraction points.");
            upgradeItems.Add(valuableCount);
            void UpdateTracker(UpgradeItem upgradeItem)
            {
                PlayerAvatar playerAvatar = PlayerController.instance.playerAvatarScript;
                if (playerAvatar != null && upgradeItem.playerUpgrade.GetLevel(playerAvatar) > 0)
                {
                    List<(GameObject, Color)> addToMap = upgradeItem.GetVariable<List<(GameObject, Color)>>("Add To Map");
                    for (int i = addToMap.Count - 1; i >= 0; i--)
                    {
                        (GameObject gameObject, Color color) = addToMap[i];
                        addToMap.RemoveAt(i);
                        MapCustom mapCustom = gameObject.GetComponent<MapCustom>();
                        if (mapCustom != null)
                            continue;
                        mapCustom = gameObject.AddComponent<MapCustom>();
                        mapCustom.color = color;
                        mapCustom.sprite = upgradeItem.GetConfig<bool>("Arrow Icon") ? 
                            assetBundle.LoadAsset<Sprite>("Map Tracker") :
                            playerAvatar.playerDeathHead.mapCustom.sprite;
                    }
                    List<GameObject> removeFromMap = upgradeItem.GetVariable<List<GameObject>>("Remove From Map");
                    for (int i = removeFromMap.Count - 1; i >= 0; i--)
                    {
                        GameObject gameObject = removeFromMap[i];
                        removeFromMap.RemoveAt(i);
                        MapCustom mapCustom = gameObject.GetComponent<MapCustom>();
                        if (mapCustom == null)
                            continue;
                        Destroy(mapCustom.mapCustomEntity.gameObject);
                        Destroy(mapCustom);
                    }
                }
            };
            UpgradeItem.Base mapEnemyTrackerBase = new UpgradeItem.Base
            {
                name = "Map Enemy Tracker",
                minPrice = 50000,
                maxPrice = 60000,
                maxPurchaseAmount = 1,
                priceIncreaseScaling = 0
            };
            UpgradeItem mapEnemyTracker = null;
            mapEnemyTrackerBase.onVariablesStart += delegate
            {
                mapEnemyTracker.AddVariable("Add To Map", new List<(GameObject, Color)>());
                mapEnemyTracker.AddVariable("Remove From Map", new List<GameObject>());
            };
            mapEnemyTrackerBase.onUpdate += delegate
            {
                if (SemiFunc.RunIsLobby() || SemiFunc.RunIsShop())
                    return;
                UpdateTracker(mapEnemyTracker);
            };
            mapEnemyTracker = new UpgradeItem(mapEnemyTrackerBase);
            mapEnemyTracker.AddConfig("Arrow Icon", true, 
                "Whether the icon should appear as an arrow showing direction instead of a dot.");
            mapEnemyTracker.AddConfig("Color", Color.red, "The color of the icon.");
            mapEnemyTracker.AddConfig("Exclude Enemies", "", 
                "Exclude specific enemies from displaying their icon by listing their names." +
                "\nExample: 'Gnome, Clown', seperated by commas.");
            upgradeItems.Add(mapEnemyTracker);
            UpgradeItem.Base mapPlayerTrackerBase = new UpgradeItem.Base
            {
                name = "Map Player Tracker",
                minPrice = 30000,
                maxPrice = 40000,
                maxPurchaseAmount = 1,
                priceIncreaseScaling = 0
            };
            UpgradeItem mapPlayerTracker = null;
            mapPlayerTrackerBase.onVariablesStart += delegate
            {
                mapPlayerTracker.AddVariable("Add To Map", new List<(GameObject, Color)>());
                mapPlayerTracker.AddVariable("Remove From Map", new List<GameObject>());
            };
            mapPlayerTrackerBase.onUpdate += delegate
            {
                UpdateTracker(mapPlayerTracker);
            };
            mapPlayerTracker = new UpgradeItem(mapPlayerTrackerBase);
            mapPlayerTracker.AddConfig("Arrow Icon", true, 
                "Whether the icon should appear as an arrow showing direction instead of a dot.");
            mapPlayerTracker.AddConfig("Player Color", false, "Whether the icon should be colored as the player.");
            mapPlayerTracker.AddConfig("Color", Color.blue, "The color of the icon.");
            upgradeItems.Add(mapPlayerTracker);
            UpgradeItem.Base itemResistBase = new UpgradeItem.Base
            {
                name = "Item Resist",
                maxAmount = 10,
                maxAmountInShop = 2,
                minPrice = 4000,
                maxPrice = 6000
            };
            UpgradeItem itemResist = null;
            itemResistBase.onVariablesStart += delegate
            {
                itemResist.AddVariable("Last Player Grabbed", new Dictionary<PhysGrabObject, PlayerAvatar>());
            };
            itemResist = new UpgradeItem(itemResistBase);
            itemResist.AddConfig("Scaling Factor", 0.9f,
                "Formula: valueLost * (scalingFactor ^ upgradeLevel)");
            upgradeItems.Add(itemResist);
            UpgradeItem.Base mapZoomBase = new UpgradeItem.Base
            {
                name = "Map Zoom",
                maxAmount = 2,
                maxAmountInShop = 1,
                minPrice = 20000,
                maxPrice = 35000,
                maxPurchaseAmount = 2
            };
            UpgradeItem mapZoom = null;
            void UpdateMapZoom(PlayerAvatar playerAvatar, int level)
            {
                if (PlayerController.instance.playerAvatarScript != playerAvatar)
                    return;
                MapPatch.mapCamera.orthographicSize = MapPatch.defaultMapZoom + level * mapZoom.GetConfig<float>("Scaling Factor");
            }
            mapZoomBase.onStart += UpdateMapZoom;
            mapZoomBase.onUpgrade += UpdateMapZoom;
            mapZoom = new UpgradeItem(mapZoomBase);
            mapZoom.AddConfig("Scaling Factor", 0.75f,
                "Formula: defaultMapZoom + upgradeLevel * scalingFactor");
            upgradeItems.Add(mapZoom);
            UpgradeItem autoScan = new UpgradeItem(new UpgradeItem.Base
            {
                name = "Autoscan",
                maxAmount = 3,
                maxAmountInShop = 1,
                minPrice = 45000,
                maxPrice = 50000,
                maxPurchaseAmount = 3
            });
            autoScan.AddConfig("Silent Scanning", false,
                "Whether the scanned items should be silent or not.");
            autoScan.AddConfig("Scaling Factor", 5f,
                "Formula: upgradeLevel * scalingFactor");
            upgradeItems.Add(autoScan);
            UpgradeItem itemValue = new UpgradeItem(new UpgradeItem.Base
            {
                name = "Item Value",
                maxAmount = 10,
                maxAmountInShop = 2,
                minPrice = 75000,
                maxPrice = 82500
            });
            itemValue.AddConfig("Scaling Factor", 0.05f,
                "This variable is based on the host!\nFormula: itemValue * (1 + upgradeLevel * scalingFactor)");
            upgradeItems.Add(itemValue);
            UpgradeItem.Base extraLifeBase = new UpgradeItem.Base
            {
                name = "Extra Life",
                maxAmount = 10,
                maxAmountInShop = 2,
                minPrice = 150000,
                maxPrice = 225000
            };
            Keybind reviveKeybind = Keybinds.Bind("Revive", "<Keyboard>/r");
            extraLifeBase.onUpdate += delegate
            {
                if (MoreUpgradesManager.instance == null)
                    return;
                PlayerAvatar playerAvatar = PlayerController.instance.playerAvatarScript;
                if (playerAvatar == null)
                    return;
                if (InputManager.instance.KeyUp(reviveKeybind.inputKey))
                    MoreUpgradesManager.instance.Revive(playerAvatar);
            };
            UpgradeItem extraLife = new UpgradeItem(extraLifeBase);
            extraLife.AddConfig("Singleplayer Invincibility Timer", 3f,
                "This variable is based on the host! After reviving, you will be given a short invincibility period.");
            extraLife.AddConfig("Multiplayer Invincibility Timer", 0f,
                "This variable is based on the host! After reviving, you will be given a short invincibility period.");
            upgradeItems.Add(extraLife);
            SceneManager.activeSceneChanged += delegate
            {
                if (RunManager.instance == null || RunManager.instance.levelCurrent == RunManager.instance.levelMainMenu 
                    || RunManager.instance.levelCurrent == RunManager.instance.levelLobbyMenu
                    || RunManager.instance.levelCurrent == RunManager.instance.levelSplashScreen)
                    return;
                GameObject manager = new GameObject("More Upgrades Manager");
                manager.AddComponent<MoreUpgradesManager>();
            };
            logger.LogMessage($"{modName} has started.");
            PatchAll("Patches");
            PatchAll("REPOLibPatches");
            if (Compatibility.CustomColors.IsLoaded())
                PatchAll("CustomColorsPatches");
        }
    }
}