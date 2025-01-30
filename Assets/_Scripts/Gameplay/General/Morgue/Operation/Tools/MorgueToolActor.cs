using System.Collections;
using System.Collections.Generic;
using _Scripts.Org;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue.Operation.Tools{
    
    public abstract class MorgueToolActor : MorgueActor, IEquip, IStorable
    {
        [SerializeField] protected FStorable _toolStorable;
        public EStorableSize StorableSize { get => _toolStorable.StorableSize; }
        
        public virtual IStorable StoreIntoStorage(IStorage storage)
        {
            IStorable storable = null;
            storable = _toolStorable.StoreIntoStorage(storage);
            if (storable != null)
            {
                //transform.SetParent(_toolStorable.Stored.st);
                //transform.localPosition = Vector3.zero;
                //transform.rotation = Quaternion.identity;
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
    }
    
}
