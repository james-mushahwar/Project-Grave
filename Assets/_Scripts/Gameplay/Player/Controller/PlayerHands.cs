using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using _Scripts.Org;
using Unity.VisualScripting;
using UnityEngine;

namespace _Scripts.Gameplay.Player.Controller{

    public class PlayerHands : MonoBehaviour, IMorgueTickable, IStorage 
    {
        [SerializeField] private FStorageSlot _lHand;
        [SerializeField] private FStorageSlot _rHand;
        public ref FStorageSlot LHand
        {
            get { return ref _lHand; }
        }

        public ref FStorageSlot RHand
        {
            get { return ref _rHand; }
        }

        public EStorableSize StorableSize { get; }
        public List<EStorableType> StorableTypeFilter { get; }
        public List<Transform> StorableSpaces { get; }
        public List<IStorable> Storables { get; }

        public void Setup()
        {
            _lHand.StorageParent = this;
            _rHand.StorageParent = this;
        }

        public void Tick()
        {
        }

        public bool IsFull()
        {
            return false;
        }

        public bool CanStorableFit(IStorable storable)
        {
            bool canFit = false;

            canFit = LHand.CanStorableFit(storable) || RHand.CanStorableFit(storable);

            return canFit;
        }

        public bool TryStore(IStorable storable)
        {
            bool stored = false;

            if (RHand.IsFull())
            {
                IStorable removed = RHand.TryRemove(storable);
            }

            if (!RHand.IsFull())
            {
                stored = RHand.TryStore(storable);
            }

            return stored;
        }

        public IStorable TryRemove(IStorable storable)
        {
            if (storable == null)
            {
                IStorable removed = LHand.TryRemove(null);

                if (removed != null)
                {
                    return removed;
                }

                removed = RHand.TryRemove(null);

                return removed;
            }
            else
            {
                if (LHand.TryFind(storable))
                {
                    return LHand.TryRemove(storable);
                }

                if (RHand.TryFind(storable))
                {
                    return RHand.TryRemove(storable);
                }
                
            }

            return null;
        }

        public bool TryFind(IStorable storable)
        {
            return false;
        }

        public List<IStorable> TryEmpty()
        {
            return null;
        }

        public IStorage GetStorageParent()
        {
            return this;
        }

        public Transform GetStorageSpace(IStorable storable)
        {
            if (storable == null)
            {
                return null;
            }

            if (LHand.TryFind(storable))
            {
                return LHand.GetStorageSpace(storable);
            }

            if (RHand.TryFind(storable))
            {
                return RHand.GetStorageSpace(storable);
            }

            return null;
        }
    }
    
}
