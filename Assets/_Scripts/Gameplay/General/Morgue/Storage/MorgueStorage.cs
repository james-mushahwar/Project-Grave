using System.Collections;
using System.Collections.Generic;
using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.Player.Controller;
using _Scripts.Org;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue.Storage{
    
    public class MorgueStorage : MorgueActor, IStorage, IInteractable
    {
        [SerializeField] protected FStorageSlot _singleSlot;

        protected FStorageSlot StorageSlot
        {
            get { return _singleSlot; }
        }

        //MorgueActor
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
            _singleSlot.StorageParent = this;
        }

        public override void Tick()
        {
        }

        //IStorage
        public bool IsFull()
        {
            return StorageSlot.IsFull();
        }

        public bool CanStorableFit(IStorable storable)
        {
            return StorageSlot.CanStorableFit(storable);
        }

        public bool TryStore(IStorable storable)
        {
            return StorageSlot.TryStore(storable);
        }

        public IStorable TryRemove(IStorable storable)
        {
            return StorageSlot.TryRemove(storable);
        }

        public bool TryFind(IStorable storable)
        {
            return StorageSlot.TryFind(storable);
        }

        public List<IStorable> TryEmpty()
        {
            return StorageSlot.TryEmpty();
        }

        public IStorage GetStorageParent()
        {
            return StorageSlot.GetStorageParent();
        }

        public Transform GetStorageSpace(IStorable storable)
        {
            return StorageSlot.GetStorageSpace(storable);
        }

        public T GetStorable<T>() where T : class, IStorable
        {
            return StorageSlot.Storable as T;
        }

        public virtual bool IsInteractable(IInteractor interactor = null)
        {
            return true;
        }

        public virtual bool OnInteract(IInteractor interactor = null)
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
