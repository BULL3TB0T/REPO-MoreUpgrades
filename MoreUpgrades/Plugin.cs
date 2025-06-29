using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using MoreUpgrades.Classes;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoreUpgrades
{
    [BepInDependency("REPOLib", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(CustomColors.modGUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(modGUID, modName, modVer)]
    internal class Plugin : BaseUnityPlugin
    {
        private const string modGUID = "bulletbot.moreupgrades";
        private const string modName = "MoreUpgrades";
        private const string modVer = "1.4.8";

        internal static Plugin instance;
        internal ManualLogSource logger;
        private readonly Harmony harmony = new Harmony(modGUID);

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
                try
                {
                    visuals = enemyParent.EnableObject.gameObject.GetComponentInChildren<Animator>().gameObject;
                }
                catch {}
                if (visuals == null)
                {
                    try
                    {
                        visuals = enemy.GetComponent<EnemyVision>().VisionTransform.gameObject;
                    }
                    catch { }
                }
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
            UpgradeItem upgradeItem = upgradeItems.FirstOrDefault(x => x.name == "Map Enemy Tracker");
            if (upgradeItem == null)
                return;
            if (component is EnemyParent enemyParent && enemyName == null)
                enemyName = enemyParent.enemyName;
            if (upgradeItem.GetConfig<string>("Exclude Enemies").Split(',').Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x)).Contains(enemyName))
                return;
            GameObject visuals = GetVisualsFromComponent(component);
            List<(GameObject, Color)> addToMap = upgradeItem.GetVariable<List<(GameObject, Color)>>("AddToMap");
            List<GameObject> removeFromMap = upgradeItem.GetVariable<List<GameObject>>("RemoveFromMap");
            if (visuals == null || addToMap.Any(x => x.Item1 == visuals))
                return;
            if (removeFromMap.Contains(visuals))
                removeFromMap.Remove(visuals);
            addToMap.Add((visuals, upgradeItem.GetConfig<Color>("Color")));
        }

        internal void RemoveEnemyFromMap(Component component, string enemyName = null)
        {
            UpgradeItem upgradeItem = upgradeItems.FirstOrDefault(x => x.name == "Map Enemy Tracker");
            if (upgradeItem == null)
                return;
            if (component is EnemyParent enemyParent && enemyName == null)
                enemyName = enemyParent.enemyName;
            if (upgradeItem.GetConfig<string>("Exclude Enemies").Split(',').Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x)).Contains(enemyName))
                return;
            GameObject visuals = GetVisualsFromComponent(component);
            List<(GameObject, Color)> addToMap = upgradeItem.GetVariable<List<(GameObject, Color)>>("AddToMap");
            List<GameObject> removeFromMap = upgradeItem.GetVariable<List<GameObject>>("RemoveFromMap");
            if (visuals == null || removeFromMap.Contains(visuals))
                return;
            if (addToMap.Any(x => x.Item1 == visuals))
                addToMap.RemoveAll(x => x.Item1 == visuals);
            removeFromMap.Add(visuals);
        }

        internal void AddPlayerToMap(PlayerAvatar playerAvatar)
        {
            UpgradeItem upgradeItem = upgradeItems.FirstOrDefault(x => x.name == "Map Player Tracker");
            if (upgradeItem == null)
                return;
            GameObject visuals = GetVisualsFromComponent(playerAvatar);
            List<(GameObject, Color)> addToMap = upgradeItem.GetVariable<List<(GameObject, Color)>>("AddToMap");
            List<GameObject> removeFromMap = upgradeItem.GetVariable<List<GameObject>>("RemoveFromMap");
            if (visuals == null || addToMap.Any(x => x.Item1 == visuals))
                return;
            if (removeFromMap.Contains(visuals))
                removeFromMap.Remove(visuals);
            Color color = upgradeItem.GetConfig<Color>("Color");
            if (upgradeItem.GetConfig<bool>("Player Color"))
                color = (Color)AccessTools.Field(typeof(PlayerAvatarVisuals), "color").GetValue(playerAvatar.playerAvatarVisuals);
            addToMap.Add((visuals, color));
        }

        internal void RemovePlayerToMap(PlayerAvatar playerAvatar)
        {
            UpgradeItem upgradeItem = upgradeItems.FirstOrDefault(x => x.name == "Map Player Tracker");
            if (upgradeItem == null)
                return;
            GameObject visuals = GetVisualsFromComponent(playerAvatar);
            List<(GameObject, Color)> addToMap = upgradeItem.GetVariable<List<(GameObject, Color)>>("AddToMap");
            List<GameObject> removeFromMap = upgradeItem.GetVariable<List<GameObject>>("RemoveFromMap");
            if (visuals == null || removeFromMap.Contains(visuals))
                return;
            if (addToMap.Any(x => x.Item1 == visuals))
                addToMap.RemoveAll(x => x.Item1 == visuals);
            removeFromMap.Add(visuals);
        }

        internal static class CustomColors
        {
            internal const string modGUID = "x753.CustomColors";

            internal static bool IsLoaded() => Chainloader.PluginInfos.ContainsKey(modGUID);

            internal static void OnAwake() => instance.PatchAll("CustomColorsPatches");
        }

        internal static float ItemValueMultiplier(float itemValueMultiplier, string itemAssetName)
        {
            if (MoreUpgradesManager.instance == null)
                return itemValueMultiplier;
            return instance.upgradeItems.FirstOrDefault(x => x.fullName == itemAssetName) != null ? 1 : itemValueMultiplier;
        }

        internal static float UpgradeValueIncrease(float upgradeValueIncrease, string itemAssetName)
        {
            if (MoreUpgradesManager.instance == null)
                return upgradeValueIncrease;
            UpgradeItem upgradeItem = instance.upgradeItems.FirstOrDefault(x => x.fullName == itemAssetName);
            if (upgradeItem == null)
                return upgradeValueIncrease;
            float value = upgradeItem.HasConfig("Price Increase Scaling") ? upgradeItem.GetConfig<float>("Price Increase Scaling") :
                upgradeItem.upgradeItemBase.priceIncreaseScaling;
            if (value < 0)
                value = upgradeValueIncrease;
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
            upgradeItems = new List<UpgradeItem>();
            UpgradeItem sprintUsage = new UpgradeItem(new UpgradeItemBase
            {
                name = "Sprint Usage",
                maxAmount = 10,
                maxAmountInShop = 2,
                minPrice = 9000,
                maxPrice = 14000
            });
            sprintUsage.AddConfig("Scaling Factor", 0.1f, "Formula: energySprintDrain / (1 + (upgradeAmount * scalingFactor))");
            sprintUsage.onFixedUpdate += () =>
            {
                int amount = sprintUsage.GetAmount();
                if (PlayerController.instance != null && amount != 0)
                {
                    if (!sprintUsage.HasVariable("OriginalEnergySprintDrain"))
                        sprintUsage.AddVariable("OriginalEnergySprintDrain", PlayerController.instance.EnergySprintDrain);
                    float originalEnergySprintDrain = sprintUsage.GetVariable<float>("OriginalEnergySprintDrain");
                    PlayerController.instance.EnergySprintDrain = originalEnergySprintDrain / 
                        (1f + (amount * sprintUsage.GetConfig<float>("Scaling Factor")));
                }
            };
            upgradeItems.Add(sprintUsage);
            UpgradeItem valuableCount = new UpgradeItem(new UpgradeItemBase 
            {
                name = "Valuable Count",
                minPrice = 30000,
                maxPrice = 40000,
                maxPurchaseAmount = 1,
                priceIncreaseScaling = 0
            });
            valuableCount.AddConfig("Display Total Value", true, "Whether to display the total value next to the valuable counter.");
            valuableCount.onInit += () =>
            {
                valuableCount.AddVariable("CurrentValuables", new List<ValuableObject>());
                valuableCount.AddVariable("Changed", false);
                valuableCount.AddVariable("PreviousCount", 0);
                valuableCount.AddVariable("PreviousValue", 0);
                valuableCount.AddVariable("TextLength", 0);
            };
            valuableCount.onUpdate += () =>
            {
                if (SemiFunc.RunIsLobby() || SemiFunc.RunIsShop())
                    return;
                if (MissionUI.instance != null && valuableCount.GetAmount() != 0)
                {
                    TextMeshProUGUI Text = (TextMeshProUGUI)AccessTools.Field(typeof(MissionUI), "Text").GetValue(MissionUI.instance);
                    string messagePrev = (string)AccessTools.Field(typeof(MissionUI), "messagePrev").GetValue(MissionUI.instance);
                    List<ValuableObject> currentValuables = valuableCount.GetVariable<List<ValuableObject>>("CurrentValuables");
                    bool changed = valuableCount.GetVariable<bool>("Changed");
                    int previousCount = valuableCount.GetVariable<int>("PreviousCount");
                    int previousValue = valuableCount.GetVariable<int>("PreviousValue");
                    int textLength = valuableCount.GetVariable<int>("TextLength");
                    int count = currentValuables.Count;
                    bool displayTotalValue = valuableCount.GetConfig<bool>("Display Total Value");
                    int value = displayTotalValue ? currentValuables.Select(x => (int)x.dollarValueCurrent).Sum() : 0;
                    if (!Text.text.IsNullOrWhiteSpace() && (changed || previousCount != count || previousValue != value))
                    {
                        string text = Text.text;
                        if (!changed && (previousCount != count || previousValue != value))
                            text = text.Substring(0, text.Length - textLength);
                        string valuableText = $"\nValuables: <b>{count}</b>" +
                            (displayTotalValue ? $" (<color=#558B2F>$</color><b>{SemiFunc.DollarGetString(value)}</b>)" : "");
                        text += valuableText;
                        valuableCount.SetVariable("PreviousCount", count);
                        valuableCount.SetVariable("PreviousValue", value);
                        valuableCount.SetVariable("TextLength", valuableText.Length);
                        Text.text = text;
                        AccessTools.Field(typeof(MissionUI), "messagePrev").SetValue(MissionUI.instance, text);
                        if (changed) 
                            valuableCount.SetVariable("Changed", false);
                    }
                }
            };
            upgradeItems.Add(valuableCount);
            void UpdateTracker(UpgradeItem upgradeItem)
            {
                if (SemiFunc.PlayerAvatarLocal() != null && upgradeItem.GetAmount() != 0)
                {
                    List<(GameObject, Color)> addToMap = upgradeItem.GetVariable<List<(GameObject, Color)>>("AddToMap");
                    for (int i = addToMap.Count - 1; i >= 0; i--)
                    {
                        (GameObject gameObject, Color color) = addToMap[i];
                        addToMap.RemoveAt(i);
                        MapCustom mapCustom = gameObject.GetComponent<MapCustom>();
                        if (mapCustom != null)
                            continue;
                        mapCustom = gameObject.AddComponent<MapCustom>();
                        mapCustom.color = color;
                        mapCustom.sprite = upgradeItem.GetConfig<bool>("Arrow Icon") ? assetBundle.LoadAsset<Sprite>("Map Tracker") :
                            SemiFunc.PlayerAvatarLocal().playerDeathHead.mapCustom.sprite;
                    }
                    List<GameObject> removeFromMap = upgradeItem.GetVariable<List<GameObject>>("RemoveFromMap");
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
            UpgradeItem mapEnemyTracker = new UpgradeItem(new UpgradeItemBase
            {
                name = "Map Enemy Tracker",
                minPrice = 50000,
                maxPrice = 60000,
                maxPurchaseAmount = 1,
                priceIncreaseScaling = 0
            });
            mapEnemyTracker.AddConfig("Arrow Icon", true, "Whether the icon should appear as an arrow showing direction instead of a dot.");
            mapEnemyTracker.AddConfig("Color", Color.red, "The color of the icon.");
            mapEnemyTracker.AddConfig("Exclude Enemies", "", "Exclude specific enemies from displaying their icon by listing their names." +
                "\nExample: 'Gnome, Clown', seperated by commas.");
            mapEnemyTracker.onInit += () =>
            {
                mapEnemyTracker.AddVariable("AddToMap", new List<(GameObject, Color)>());
                mapEnemyTracker.AddVariable("RemoveFromMap", new List<GameObject>());
            };
            mapEnemyTracker.onUpdate += () =>
            {
                if (SemiFunc.RunIsLobby() || SemiFunc.RunIsShop())
                    return;
                UpdateTracker(mapEnemyTracker);
            };
            upgradeItems.Add(mapEnemyTracker);
            UpgradeItem mapPlayerTracker = new UpgradeItem(new UpgradeItemBase
            {
                name = "Map Player Tracker",
                minPrice = 30000,
                maxPrice = 40000,
                maxPurchaseAmount = 1,
                priceIncreaseScaling = 0
            });
            mapPlayerTracker.AddConfig("Arrow Icon", true, "Whether the icon should appear as an arrow showing direction instead of a dot.");
            mapPlayerTracker.AddConfig("Player Color", false, "Whether the icon should be colored as the player.");
            mapPlayerTracker.AddConfig("Color", Color.blue, "The color of the icon.");
            mapPlayerTracker.onInit += () =>
            {
                mapPlayerTracker.AddVariable("AddToMap", new List<(GameObject, Color)>());
                mapPlayerTracker.AddVariable("RemoveFromMap", new List<GameObject>());
            };
            mapPlayerTracker.onUpdate += () =>
            {
                UpdateTracker(mapPlayerTracker);
            };
            upgradeItems.Add(mapPlayerTracker);
            SceneManager.activeSceneChanged += delegate
            {
                if (RunManager.instance == null || RunManager.instance.levelCurrent == RunManager.instance.levelMainMenu 
                    || RunManager.instance.levelCurrent == RunManager.instance.levelLobbyMenu)
                    return;
                GameObject manager = new GameObject("More Upgrades Manager");
                PhotonView photonView = manager.AddComponent<PhotonView>();
                photonView.ViewID = 1863;
                manager.AddComponent<MoreUpgradesManager>();
            };
            logger.LogMessage($"{modName} has started.");
            PatchAll("Patches");
            if (CustomColors.IsLoaded())
                CustomColors.OnAwake();
        }
    }
}