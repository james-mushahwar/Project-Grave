using _Scripts.Gameplay.Architecture.Managers;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Scripts.Gameplay.Collectible {

    [CreateAssetMenu(fileName = "CoinUpgrade_", menuName = "Scriptable Objects/UpgradeScriptableObject/CoinUpgrade")]
    public class Coins_UpgradeScriptableObject : UpgradeScriptableObject
    {
        [SerializeField]
        private Vector2Int _currencyRewardMinMax; // can be + or -

        public int GetCurrencyReward
        {
            get
            {
                return Random.Range(_currencyRewardMinMax.x, _currencyRewardMinMax.y);
            }
        }

        private void Awake()
        {
            _upgradeType = EUpgradeType.Coins;
        }

        public override bool TryTriggerUpgradeBehaviour()
        {
            if (TriggerOnChance)
            {
                CollectibleManager.Instance.AddCurrency(GetCurrencyReward);

                return true;
            }

            return false;
        }

    }

}
