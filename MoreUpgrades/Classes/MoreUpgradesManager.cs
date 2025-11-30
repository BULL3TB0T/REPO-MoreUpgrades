using HarmonyLib;
using Photon.Pun;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace MoreUpgrades.Classes
{
    internal class MoreUpgradesManager : MonoBehaviour
    {
        internal static MoreUpgradesManager instance;
        private PhotonView photonView;
        private bool checkPlayerUpgrades;

        private void Awake()
        {
            instance = this;
            photonView = gameObject.AddComponent<PhotonView>();
            photonView.ViewID = 876842;
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
                    if (upgradeItem.GetConfig<bool>("Sync Host Upgrades"))
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

        public void Revive(PlayerAvatar playerAvatar)
        {
            string steamId = SemiFunc.PlayerGetSteamID(playerAvatar);
            if (steamId == null)
                return;
            if (!SemiFunc.IsMultiplayer())
            {
                ReviveRPC(steamId);
                return;
            }
            photonView.RPC("ReviveRPC", RpcTarget.MasterClient, new object[] { steamId });
        }

        [PunRPC]
        private void ReviveRPC(string steamId)
        {
            if (SemiFunc.RunIsShop() || SemiFunc.RunIsArena())
                return;
            PlayerAvatar playerAvatar = SemiFunc.PlayerAvatarGetFromSteamID(steamId);
            if (playerAvatar == null ||
                !(bool)AccessTools.Field(typeof(PlayerAvatar), "isDisabled").GetValue(playerAvatar) ||
                !(bool)AccessTools.Field(typeof(PlayerAvatar), "deadSet").GetValue(playerAvatar))
                return;
            UpgradeItem upgradeItem = Plugin.instance.upgradeItems.FirstOrDefault(x => x.upgradeBase.name == "Extra Life");
            if (upgradeItem == null || upgradeItem.playerUpgrade.GetLevel(steamId) <= 0)
                return;
            upgradeItem.playerUpgrade.RemoveLevel(playerAvatar);
            playerAvatar.Revive();
            PlayerHealth playerHealth = playerAvatar.playerHealth;
            playerHealth.HealOther(
                (int)AccessTools.Field(typeof(PlayerHealth), "maxHealth").GetValue(playerHealth) - 1, false);
            bool isMultiplayer = SemiFunc.IsMultiplayer();
            float duration = upgradeItem.GetConfig<float>(
                $"{(isMultiplayer ? "Multiplayer" : "Singleplayer")} Invincibility Timer");
            if (!isMultiplayer)
                SetInvincibleRPC(steamId, duration);
            else
                photonView.RPC("SetInvincibleRPC", RpcTarget.All, new object[] { steamId, duration });
        }

        [PunRPC]
        private void SetInvincibleRPC(string steamId, float duration)
        {
            PlayerAvatar playerAvatar = SemiFunc.PlayerAvatarGetFromSteamID(steamId);
            if (playerAvatar == null ||
                !(bool)AccessTools.Field(typeof(PlayerAvatar), "isDisabled").GetValue(playerAvatar) ||
                !(bool)AccessTools.Field(typeof(PlayerAvatar), "deadSet").GetValue(playerAvatar))
                return;
            playerAvatar.playerHealth.InvincibleSet(duration);
        }
    }
}
