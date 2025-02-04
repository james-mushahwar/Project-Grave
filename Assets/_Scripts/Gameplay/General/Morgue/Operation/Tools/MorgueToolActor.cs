using System.Collections;
using System.Collections.Generic;
using _Scripts.Org;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue.Operation.Tools{
    
    public abstract class MorgueToolActor : MorgueActor, IEquip, IStorable
    {
        [SerializeField] protected FStorable _toolStorable;

        [SerializeField] protected float _lerpMoveSpeed;
        public ref FStorable ToolStorable { get { return ref _toolStorable; } }
        //public ref FStorageSlot DefaultStorage { get { return ref _defaultStorage; } }
        public EStorableSize StorableSize { get => _toolStorable.StorableSize; }

        public IStorage Stored => _toolStorable.Stored;

        public override void Setup()
        {
            _toolStorable.StorableParent = this;

            //DefaultStorage.TryStore(_toolStorable);
        }

        public virtual IStorable StoreIntoStorage(IStorage storage)
        {
            IStorable storable = null;
            storable = _toolStorable.StoreIntoStorage(storage);
            
            if (storable != null)
            {
                if (_toolStorable.GetStorableParent() != null)
                {
                    Vector3 localPosition = this.gameObject.transform.localPosition;
                    this.gameObject.transform.SetParent(storage.GetStorageSpace(_toolStorable), true);
                    //storableMono.gameObject.transform.localPosition = Vector3.zero;
                    Transform storageSpace = storage.GetStorageSpace(storable);
                    this.gameObject.transform.rotation = storageSpace.rotation;
                    this.gameObject.transform.localPosition = localPosition;
                }
            }

            return storable;
        }

        public virtual IStorable RemoveFromStorage(IStorage storage)
        {
            return _toolStorable.RemoveFromStorage(storage);
        }

        public bool IsStored()
        {
            return _toolStorable.IsStored();
        }

        public IStorable GetStorableParent()
        {
            return this;
        }
    }
    
}
