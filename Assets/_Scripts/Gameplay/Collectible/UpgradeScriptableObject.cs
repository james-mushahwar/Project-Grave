using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.Collectible {
    
    public enum EUpgradeBehaviour
    {
        Passive,
        OneShot,
    }

    public enum EUpgradeType
    {
        Coins,      // ca
        SlowDayTime,
        AutoSawing, // can kick in and start perfect sawing
        Sharpness,  // affect the effectiveness of sawing
        OrderWait,  // how long cutsomers wait for, can be longer or shorter
    }

    public enum EGameplayEvents
    {
        PerfectSaw = 0,
        PoorSaw,
        Dismembered,            
        BodyFullyDismembered,   // all limbs dismembered
        BodyPartDisposed,
        PerfectBodyPart,        // happens on dismember completed

        OrderComplete = 100,    // customer receives order
        OrderFailed,            // customer leaves without order
        PerfectOrder,           // everything in perfect condition
        SpeedyOrder,            // customer receives order below time threshold

        DayStart = 200,         // the day starts
        DayOvertime,            // overtime in day is reached
        DayEnd,                 // day ends, sleep
        TaxCollectorVisits,     // when tax collector arrives

        PurchasedUpgrade = 300, // purchased upgrade
        ExpiredUpgrade,         // upgrade has gone/been used up
        UpgradeTriggered,        // SUPER - trigger every time another upgrade is triggered
    }

    public abstract class UpgradeScriptableObject : ScriptableObject
    {
        protected EUpgradeType _upgradeType;

        [SerializeField]
        protected EUpgradeBehaviour _upgradeBehaviour;
        [SerializeField]
        protected List<EGameplayEvents> _upgradesTriggers; 

        [SerializeField]
        protected int _currencyCost;
        [SerializeField, UnityEngine.RangeAttribute(0, 10)]
        protected int _rarity; // 0 to 10, 10 being rarest

        [SerializeField]
        protected float _cooldownTime = 0.0f; // 0 == no cooldown when used 
        [SerializeField]
        protected float _behaviourChance = 1.0f;  // 1 == always happens
        [SerializeField]
        protected int _expireAfterUses = -1; // -1 means never expire

        [SerializeField]
        private BaseCollectible _upgradePrefab;

        public int CurrencyCost { get => _currencyCost; }

        public bool TriggerOnChance { get { return Random.Range(0.0f, 1.0f) < _behaviourChance; } }

        public BaseCollectible UpgradePrefab { get => _upgradePrefab; }

        public virtual bool TryTriggerUpgradeBehaviour()
        {
            return false;
        }

        public bool DoesReactToTrigger(EGameplayEvents trigger)
        {
            return _upgradesTriggers.Contains(trigger);
        }
    }
    
}
