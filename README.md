# MoreUpgrades
- Adds more upgrade items to the game.
- All players need to have the same config! (**Note:** Some of them can be client-sided or server-sided.)
- The upgrade item designs are made by "Kizzycocoa" (1277336819951730739)
## Upgrade Items
- **Sprint Usage**: Makes you able to use less stamina while sprinting. *Can be upgraded multiple times*.
- **Valuable Count**: Displays the number of valuables under the mission text. *Can be upgraded only once*.
- **Map Enemy Tracker**: Tracks enemies in the map radar. *Can be upgraded only once*.
- **Map Player Tracker**: Tracks players in the map radar. *Can be upgraded only once*.
- **Item Resist**: Makes you able to withstand item hits easier. *Can be upgraded multiple times*. (***Credits to the [ItemResistUpgrade mod](https://thunderstore.io/c/repo/p/TopSandwich/ItemResistUpgrade) creator called "Top Sandwich" (109074579716087808).***)
- **Map Zoom**: Decreases the zoom level of the map radar. *Can be upgraded twice unless you have a mod that spawns them in a level*.
- **Autoscan**: Automatically scans for items within a given range. *Can be upgraded three times unless you have a mod that spawns them in a level*.
- **Item Value**: Increases item value globally during level generation. *Can be upgraded multiple times*.
- **Extra Life**: Makes you able to revive by pressing the Tumble keybind while being dead. *Can be upgraded multiple times, but when used, the upgrade count decreases*.
## REPOLib Configuration
- **Import Upgrades**: Whether to import the upgrades from [REPOLib](https://thunderstore.io/c/repo/p/Zehs/REPOLib).
- **Exclude Upgrade IDs**: Exclude specific [REPOLib](https://thunderstore.io/c/repo/p/Zehs/REPOLib) upgrades by listing their IDs, seperated by commas.
## Upgrade Item Configuration
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