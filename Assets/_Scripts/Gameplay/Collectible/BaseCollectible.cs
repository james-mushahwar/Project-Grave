using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.General.Morgue;
using _Scripts.Org;
using UnityEngine;

namespace _Scripts.Gameplay.Collectible {

    public class BaseCollectible : MorgueActor, ISelect, IInteractable
    {
        [SerializeField]
        private UpgradeScriptableObject _upgradeSO;

        public UpgradeScriptableObject UpgradeSO { get => _upgradeSO; set => _upgradeSO = value; }

        public bool IsInteractable(IInteractor interactor = null)
        {
            return CollectibleManager.Instance.IsUpgradeUnlocked(this) == false;
        }
        public bool OnInteract(IInteractor interactor = null)
        {
            if (IsInteractable(interactor))
            {
                bool unlock =  CollectibleManager.Instance.UnlockUpgrade(_upgradeSO);
                CollectibleManager.Instance.ReturnCollectibleToPool(this);
                return unlock;
            }

            return false;
        }

        public void OnDeselected()
        {
            //unhighlight collectible
        }
        public void OnSelected()
        {
            //highlight collectible
        }

        public override void EnterHouseThroughChute()
        {
        }

        public override void ToggleProne(bool set)
        {
        }

        public override void ToggleCollision(bool set)
        {
        }

        public override void Setup()
        {
        }

        public override void Tick()
        {
        }
    }

}
