using _Scripts.Gameplay.Animate;
using _Scripts.Gameplay.Animate.JitterAnimation;
using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.General.Morgue.Bodies;
using _Scripts.Gameplay.Player.Controller;
using _Scripts.Org;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static SerializableDictionary;

namespace _Scripts.Gameplay.General.Morgue.Storage {
    
    public class GroupMorgueStorage : MorgueActor, IStorage, IInteractable
    {
        [SerializeField] protected List<FStorageSlot> _storageSlots;

        [SerializeField] protected JitterBehaviour _jitterBehaviour;

        [SerializeField] protected MorgueActorAnimator _morgueActorAnimator;

        [SerializeField] protected bool _updateJitter;

        // is object in position to accept storage e.g. Is the hook on the chain down?
        private bool _isAvailableToStore = false;


        public IStorage NextAvailableStorage
        {
            get 
            {
                foreach (FStorageSlot slot in _storageSlots)
                {
                    if (slot.IsFull() == false)
                    {
                        return slot;
                    }
                }

                return null;
            }
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
            //start down state
            //OnAvailable();
            
        }

        public override void Tick()
        {
            if (_updateJitter)
            {
                EJitteryType jitterType = EJitteryType.Standard;

                if (IsFull() == false)
                {
                    PlayerController pc = PlayerManager.Instance.CurrentPlayerController;

                    if (pc)
                    {
                        if (pc.PlayerStorage.IsCarrying<BodyPartMorgueActor>() != null)
                        {
                            jitterType = EJitteryType.ItemOfInterest;
                        }
                    }
                }

                _jitterBehaviour.SetJitter(jitterType);
            }

            TickAnimation();

            if (_isAvailableToStore)
            {
                //make sure in position
                if (IsFull())
                {
                    Debug.Log("Cash in");
                    OnUnavailable();
                }
            }
        }

        private void TickAnimation()
        {
            
        }

        public void ToggleAvailability()
        {
            if (_isAvailableToStore == true)
            {
                OnUnavailable();
            }
            else
            {
                OnAvailable();
            }
        }

        public void OnAvailable()
        {
            _isAvailableToStore = true;

            _morgueActorAnimator?.PlayAnimation(EMorgueAnimType.Available);
        }

        private void OnUnavailable()
        {
            _isAvailableToStore = false;

            _morgueActorAnimator?.PlayAnimation(EMorgueAnimType.Unavailable);

            //cash in/ cause day night evoke?
            ActionSequenceManager.Instance.OnStimulusReceived(EMorgueStimulus.Store_HooksComplete);

        }

        public bool CanStorableFit(IStorable storable)
        {
            return NextAvailableStorage.CanStorableFit(storable);
        }

        public IStorage GetStorageParent()
        {
            return NextAvailableStorage.GetStorageParent();
        }

        public Transform GetStorageSpace(IStorable storable)
        {
            return NextAvailableStorage.GetStorageSpace(storable);
        }

        public bool IsFull()
        {
            bool full = true;

            foreach(FStorageSlot slot in _storageSlots)
            {
                if (slot.IsFull() == false)
                {
                    return false;
                }
            }
            return full;
        }

        public List<IStorable> TryEmpty()
        {
            List<IStorable> emptied = new List<IStorable>();
            List<IStorable> emptyList = new List<IStorable>();

            foreach (FStorageSlot slot in _storageSlots)
            {
                emptyList = slot.TryEmpty();
                emptied.Concat(emptyList);
            }

            return emptied;
        }

        public bool TryFind(IStorable storable)
        {
            bool found = false;

            foreach(FStorageSlot slot in _storageSlots)
            {
                found = slot.TryFind(storable);
                if (found == true)
                {
                    return found;
                }
            }

            return found;
        }

        public IStorable TryRemove(IStorable storable)
        {
            IStorable found = null;

            foreach (FStorageSlot slot in _storageSlots)
            {
                found = slot.TryRemove(storable);
                if (found != null)
                {
                    return found;
                }
            }

            return found;
        }

        public bool TryStore(IStorable storable)
        {
            if (NextAvailableStorage == null || _isAvailableToStore == false)
            {
                return false;
            }
            return NextAvailableStorage.TryStore(storable);
        }

        public T GetStorable<T>() where T : class, IStorable
        {
            T found = null;

            foreach (FStorageSlot slot in _storageSlots)
            {
                found = slot.Storable as T;
                if (found != null)
                {
                    return found;
                }
            }

            return found;
        }

        public bool IsInteractable(IInteractor interactor = null)
        {
            return _isAvailableToStore;
        }

        public bool OnInteract(IInteractor interactor = null)
        {
            bool interact = false;

            if (interactor == null)
            {
                return false;
            }

            PlayerController pc = interactor as PlayerController;

            if (pc != null)
            {
                IStorage hands = pc.PlayerStorage.GetPlayerHands();
                IStorable removed = hands.TryRemove(null);
                if (removed != null)
                {
                    interact = TryStore(removed.GetStorableParent());
                }
                else
                {
                    // try remove from storage and store in hands
                    removed = TryRemove(null);

                    if (removed != null)
                    {
                        interact = hands.TryStore(removed.GetStorableParent());
                    }
                }
            }

            return interact;
        }
    }
    
}
