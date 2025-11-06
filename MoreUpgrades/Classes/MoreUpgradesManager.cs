using System.Collections;
using System.Linq;
using UnityEngine;

namespace MoreUpgrades.Classes
{
    internal class MoreUpgradesManager : MonoBehaviour
    {
        internal static MoreUpgradesManager instance;
        private bool checkPlayerUpgrades;

        private void Awake()
        {
            instance = this;
            foreach (UpgradeItem upgradeItem in Plugin.instance.upgradeItems)
            {
                upgradeItem.variables?.Clear();
                upgradeItem.upgradeBase.onVariablesStart?.Invoke();
            }
            if (SemiFunc.IsMasterClientOrSingleplayer())
                StartCoroutine("WaitUntilLevel");
        }

        private void Update()
        {
            foreach (UpgradeItem upgradeItem in Plugin.instance.upgradeItems)
                upgradeItem.upgradeBase.onUpdate?.Invoke();
        }
        private void LateUpdate()
        {
            foreach (UpgradeItem upgradeItem in Plugin.instance.upgradeItems)
                upgradeItem.upgradeBase.onLateUpdate?.Invoke();
        }

        private void FixedUpdate()
        {
            foreach (UpgradeItem upgradeItem in Plugin.instance.upgradeItems)
            {
                if (checkPlayerUpgrades)
                {
                    if (upgradeItem.HasConfig("Starting Amount"))
                    {
                        int amount = upgradeItem.GetConfig<int>("Starting Amount");
                        if (amount >= 0)
                        {
                            foreach (PlayerAvatar playerAvatar in SemiFunc.PlayerGetAll())
                            {
                                string steamId = SemiFunc.PlayerGetSteamID(playerAvatar);
                                if (!upgradeItem.appliedPlayerDictionary.ContainsKey(steamId))
                                    upgradeItem.appliedPlayerDictionary[steamId] = 0;
                                if (upgradeItem.appliedPlayerDictionary[steamId] == amount)
                                    continue;
                                upgradeItem.playerUpgrade.SetLevel(playerAvatar, amount);
                                upgradeItem.appliedPlayerDictionary[steamId] = amount;
                            }
                        }
                    }
                    if (upgradeItem.HasConfig("Sync Host Upgrades") && upgradeItem.GetConfig<bool>("Sync Host Upgrades"))
                    {
                        PlayerAvatar localPlayerAvatar = SemiFunc.PlayerAvatarLocal();
                        int hostAmount = upgradeItem.playerUpgrade.GetLevel(localPlayerAvatar);
                        foreach (PlayerAvatar playerAvatar in SemiFunc.PlayerGetAll().Where(x => x != localPlayerAvatar))
                        {
                            if (hostAmount == upgradeItem.playerUpgrade.GetLevel(playerAvatar))
                                continue;
                            upgradeItem.playerUpgrade.SetLevel(playerAvatar, hostAmount);
                        }
                    }
                }
                upgradeItem.upgradeBase.onFixedUpdate?.Invoke();
            }
        }

        private IEnumerator WaitUntilLevel()
        {
            yield return new WaitUntil(() => SemiFunc.LevelGenDone());
            checkPlayerUpgrades = true;
        }
    }
}
