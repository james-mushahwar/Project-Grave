using _Scripts.Gameplay.Collectible;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

namespace _Scripts.Gameplay.Architecture.Managers {
    
    public class CollectibleManager : GameManager<CollectibleManager>, IManager
    {
        private List<bool> _upgradesUnlocked;

        private int _currency = 10;

        [SerializeField]
        private List<UpgradeScriptableObject> _upgrades;

        private List<BaseCollectible> _upgradeGOs;

        private List<UpgradeStorage> _upgradeStorages = new List<UpgradeStorage>();

        public virtual void ManagedPreInGameLoad()
        {
            _upgradesUnlocked = new List<bool>(_upgrades.Count);
            _upgradeGOs = new List<BaseCollectible>(_upgrades.Count);

            for (int i = 0; i < _upgrades.Count; i++)
            {
                _upgradesUnlocked.Add(false);
                BaseCollectible collectible = GameObject.Instantiate(_upgrades[i].UpgradePrefab);
                if (collectible != null)
                {
                    collectible.UpgradeSO = _upgrades[i];
                    _upgradeGOs.Add(collectible);
                }
            }

            //temp here for now
            RefreshCollectiblesAvailable();
        }

        public virtual void ManagedTick()
        {

        }

        public void AddCurrency(int amount) // can be + or -
        {
            _currency += amount;

            Debug.Log("Currency is now " + _currency);
        }

        public bool CanObtainUpgrade(BaseCollectible collectible)
        {
            if (collectible.UpgradeSO == null)
            {
                return false;
            }

            bool canAfford = collectible.UpgradeSO.CurrencyCost <= _currency;

            return canAfford && !IsUpgradeUnlocked(collectible);
        }

        public bool IsUpgradeUnlocked(BaseCollectible collectible)
        {
            int unlockIndex = _upgrades.IndexOf(collectible.UpgradeSO);

            if (unlockIndex == -1)
            {
                return true;
            }

            if (_upgradesUnlocked[unlockIndex] == true)
            {
                return true;
            }

            return false;
        }

        public bool UnlockUpgrade(UpgradeScriptableObject upgrade)
        {
            if (upgrade == null)
            {
                return false;
            }

            AddCurrency(-upgrade.CurrencyCost);

            int unlockIndex = _upgrades.IndexOf(upgrade);

            _upgradesUnlocked[unlockIndex] = true;

            Debug.Log("Unlocked " + upgrade);

            return true;
        }

        public void ReturnCollectibleToPool(BaseCollectible collectible)
        {
            collectible.gameObject.SetActive(false);
            collectible.transform.SetParent(transform, false);
            collectible.transform.localPosition = Vector3.zero;
        }

        public BaseCollectible SpawnCollectibleFromPool(UpgradeScriptableObject upgrade)
        {
            if (upgrade == false)
            {
                return null;
            }

            int unlockIndex = _upgrades.IndexOf(upgrade);

            BaseCollectible foundCollectible = _upgradeGOs[unlockIndex];

            if (foundCollectible == null)
            {
                return null;
            }

            foundCollectible.gameObject.SetActive(true);
            return foundCollectible;
        }

        public UpgradeScriptableObject GetUpgrade()
        {
            List<UpgradeScriptableObject> possibleUpgrades = new List<UpgradeScriptableObject>();

            for (int i = 0; i < _upgrades.Count; i++)
            {
                if (_upgradesUnlocked[i] == true)
                {
                    continue;
                }
                possibleUpgrades.Add(_upgrades[i]);
            }


            if (possibleUpgrades.Count == 0)
            {
                return null;
            }

            return possibleUpgrades[UnityEngine.Random.Range(0, possibleUpgrades.Count - 1)];
        }

        public void RefreshCollectiblesAvailable()
        {
            for (int i = 0; i < _upgradeStorages.Count; i++)
            {
                RefreshStorageCollectible(_upgradeStorages[i]);
            }
        }

        private void RefreshStorageCollectible(UpgradeStorage storage)
        {
            if (storage != null)
            {
                if (storage.UpgradeStored != null)
                {
                    return;
                }

                UpgradeScriptableObject upgrade = GetUpgrade();

                if (upgrade != null)
                {
                    BaseCollectible collectible = SpawnCollectibleFromPool(upgrade);
                    if (collectible)
                    {
                        storage.UpgradeStored = collectible;
                    }
                }
            }
        }

        public void OnUpgradeTrigger(EGameplayEvents trigger)
        {
            for (int i = 0; i < _upgradesUnlocked.Count; i++)
            {
                if (_upgradesUnlocked[i])
                {
                    UpgradeScriptableObject upgrade = _upgrades[i];

                    if (upgrade != null)
                    {
                        if (upgrade.DoesReactToTrigger(trigger))
                        {
                            bool triggered = upgrade.TryTriggerUpgradeBehaviour();
                            if (triggered)
                            {

                            }
                        }
                    }
                }
            }
        }

        public void RegisterCollectibleStorage(UpgradeStorage upgradeStorage)
        {
            if (_upgradeStorages.Contains(upgradeStorage))
            {
                return;
            }
            _upgradeStorages.Add(upgradeStorage);
            RefreshStorageCollectible(upgradeStorage);
        }
    }
    
}
