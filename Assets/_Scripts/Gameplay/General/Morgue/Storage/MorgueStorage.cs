using System.Collections;
using System.Collections.Generic;
using _Scripts.Org;
using UnityEngine;
using static SerializableDictionary;

namespace _Scripts.Gameplay.General.Morgue.Storage{
    
    public class MorgueStorage : MorgueActor, IStorage
    {
        [SerializeField] private FStorageSlot _singleSlot;

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
    }
    
}
