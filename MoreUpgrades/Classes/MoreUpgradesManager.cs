using Photon.Pun;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace MoreUpgrades.Classes
{
    internal class MoreUpgradesManager : MonoBehaviour
    {
        internal static MoreUpgradesManager instance;
        internal PhotonView photonView;
        private bool checkPlayerUpgrades;

        private void Awake()
        {
            instance = this;
            photonView = GetComponent<PhotonView>();
            foreach (UpgradeItem upgradeItem in Plugin.instance.upgradeItems)
            {
                upgradeItem.variables.Clear();
                upgradeItem.onInit?.Invoke();
            }
            if (SemiFunc.IsMasterClientOrSingleplayer())
                StartCoroutine("WaitUntilLevel");
        }

        private void LateUpdate()
        {
            foreach (UpgradeItem upgradeItem in Plugin.instance.upgradeItems)
                upgradeItem.onLateUpdate?.Invoke();
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
                                if (!upgradeItem.appliedPlayerUpgrades.ContainsKey(steamId))
                                    upgradeItem.appliedPlayerUpgrades[steamId] = 0;
                                if (upgradeItem.appliedPlayerUpgrades[steamId] == amount)
                                    continue;
                                Upgrade(upgradeItem.name, steamId, amount - upgradeItem.appliedPlayerUpgrades[steamId]);
                                upgradeItem.appliedPlayerUpgrades[steamId] = amount;
                            }
                        }
                    }
                    if (upgradeItem.HasConfig("Sync Host Upgrades") && upgradeItem.GetConfig<bool>("Sync Host Upgrades"))
                    {
                        int hostAmount = upgradeItem.GetAmount();
                        foreach (PlayerAvatar playerAvatar in SemiFunc.PlayerGetAll().Where(x => x != SemiFunc.PlayerAvatarLocal()))
                        {
                            string steamId = SemiFunc.PlayerGetSteamID(playerAvatar);
                            int amount = upgradeItem.GetAmount(steamId);
                            Upgrade(upgradeItem.name, steamId, hostAmount - amount);
                        }
                    }
                }
                upgradeItem.onFixedUpdate?.Invoke();
            }
        }

        private void Update()
        {
            foreach (UpgradeItem upgradeItem in Plugin.instance.upgradeItems)
                upgradeItem.onUpdate?.Invoke();
        }

        private IEnumerator WaitUntilLevel()
        {
            yield return new WaitUntil(() => SemiFunc.LevelGenDone());
            checkPlayerUpgrades = true;
            if (!GameManager.Multiplayer())
                yield break;
            foreach (UpgradeItem upgradeItem in Plugin.instance.upgradeItems)
            {
                foreach (PlayerAvatar playerAvatar in SemiFunc.PlayerGetAll())
                {
                    string steamId = SemiFunc.PlayerGetSteamID(playerAvatar);
                    photonView.RPC("UpgradeRPC", RpcTarget.OthersBuffered,
                        upgradeItem.name,
                        steamId,
                        upgradeItem.GetAmount(steamId)
                    );
                }
            }
        }

        internal void Upgrade(string upgradeItemName, string steamId, int amount = 1)
        {
            if (amount == 0)
                return;
            UpgradeItem upgradeItem = Plugin.instance.upgradeItems.FirstOrDefault(x => x.name == upgradeItemName);
            if (upgradeItem == null)
                return;
            upgradeItem.playerUpgrades[steamId] += amount;
            if (SemiFunc.PlayerAvatarGetFromSteamID(steamId) == SemiFunc.PlayerAvatarLocal())
                upgradeItem.onChanged?.Invoke();
            if (!GameManager.Multiplayer())
                return;
            photonView.RPC("UpgradeRPC", 
                SemiFunc.IsMasterClient() ? RpcTarget.Others : RpcTarget.MasterClient, 
                upgradeItemName, steamId, upgradeItem.playerUpgrades[steamId]);
        }

        [PunRPC]
        internal void UpgradeRPC(string upgradeItemName, string steamId, int amount)
        {
            UpgradeItem upgradeItem = Plugin.instance.upgradeItems.FirstOrDefault(x => x.name == upgradeItemName);
            if (upgradeItem == null || upgradeItem.playerUpgrades[steamId] == amount)
                return;
            upgradeItem.playerUpgrades[steamId] = amount;
            if (SemiFunc.PlayerAvatarGetFromSteamID(steamId) == SemiFunc.PlayerAvatarLocal())
                upgradeItem.onChanged?.Invoke();
            if (SemiFunc.IsMasterClient())
                photonView.RPC("UpgradeRPC", RpcTarget.Others, upgradeItemName, steamId, amount);
        }
    }
}
