using _Scripts.Gameplay.Architecture.Managers;
using UnityEngine;

namespace _Scripts.Gameplay.Collectible {
    
    public class UpgradeStorage : MonoBehaviour
    {
        private BaseCollectible _upgradeStored;

        [SerializeField]
        private Transform _upgradeHolderTransform;

        public BaseCollectible UpgradeStored
        {
            get => _upgradeStored;
            set
            {
                if (value)
                {
                    value.transform.SetParent(_upgradeHolderTransform);
                    value.transform.localPosition = Vector3.zero;
                }
                _upgradeStored = value;
            }
        }

        public void Start()
        {
            CollectibleManager.Instance.RegisterCollectibleStorage(this);
        }

    }
    
}
