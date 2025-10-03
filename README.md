# MoreUpgrades
- Adds more upgrade items to the game.
- All players need to have the same config! (**Note:** Some of them can be client-sided.)
## Items
- **Sprint Usage**: It uses less stamina when sprinting. *Can be upgraded multiple times*.
- **Valuable Count**: Displays the number of valuables under the mission text. *Can be upgraded only once*.
- **Map Enemy Tracker**: Tracks enemies in the map radar. *Can be upgraded only once*.
- **Map Player Tracker**: Tracks players in the map radar. *Can be upgraded only once*.
- **Item Resist**: It adds item resistance to withstand the hits easier. *Can be upgraded multiple times*. (***Credits to the original mod creator called "Top Sandwich" (109074579716087808).***)
## REPOLib Configuration
- **Import Upgrades**: Whether to import the upgrades from [REPOLib](https://thunderstore.io/c/repo/p/Zehs/REPOLib).
- **Exclude Upgrade IDs**: Exclude specific [REPOLib](https://thunderstore.io/c/repo/p/Zehs/REPOLib) upgrades by listing their IDs, seperated by commas.
## Item Configuration
- **Enabled**: Whether the upgrade item can be spawned to the shop.
- **Max Amount**: The maximum number of times the upgrade item can appear in the truck.
- **Max Amount In Shop**: The maximum number of times the upgrade item can appear in the shop.
- **Minimum Price**: The minimum cost to purchase the upgrade item.
- **Maximum Price**: The maximum cost to purchase the upgrade item.
- **Price Increase Scaling**: The scale of the price increase based on the total number of upgrade item purchased.
- **Price Multiplier**: The multiplier of the price.
- **Max Purchase Amount**: The maximum number of times the upgrade item can be purchased before it is no longer available in the shop.
- **Allow Team Upgrades**: Whether the upgrade item applies to the entire team instead of just one player.
- **Sync Host Upgrades**: Whether the host should sync the item upgrade for the entire team.
- **Starting Amount**: The number of times the upgrade item is applied at the start of the game.
## Note
Some upgrade items have more configuration.
Check the config file after updates, as values may change between versions.
## Adding Custom Upgrade Items
1. **Use the Project Patcher**: Follow the instructions in the [R.E.P.O. Project Patcher](https://github.com/Kesomannen/unity-repo-project-patcher)'s README.
2. **Find an Existing Item**: Search for an item in the project that you want to use as a reference.
3. **Create Your Own Item and Prefab**: Duplicate it and modify the item and the prefab as you wish.
4. **Make an Asset Bundle**: Package your item and prefab into an asset bundle so they can be loaded to the game.
- Afterwards you can use these methods:
```
MoreUpgradesLib.IsManagerActive() => bool

MoreUpgradesLib.GetCoreUpgradeItems() => IReadOnlyList<UpgradeItem>

MoreUpgradesLib.GetUpgradeItemsByMod(
    string modGUID
) => IReadOnlyList<UpgradeItem>

MoreUpgradesLib.Register(
    string modGUID,
    Item item,
    GameObject prefab,
    UpgradeItemBase upgradeItemBase
) => UpgradeItem
```