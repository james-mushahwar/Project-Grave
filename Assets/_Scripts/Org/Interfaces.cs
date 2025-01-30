using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace _Scripts.Org
{
    public interface IMorgueTickable
    {
        void Setup();
        void Tick();
    }

    public interface IInteractable
    {
        bool IsInteractable();
        bool OnInteract();
    }

    public interface IOperatable
    {
        void StartOperation();
        void StopOperation();
    }

    //public interface ICarryable
    //{
    //    public bool IsCarried { get; }
    //}
    //public interface ICarrier
    //{
    //    bool CanCarry(ICarryable carry);
    //    bool TryCarry(ICarryable carry);
    //    bool TryDrop(ICarryable carry);
    //}

    public enum EStorableSize : uint
    {
        Small = 0, Medium = 10, Large = 20
    }

    public enum EStorableType : uint
    {
        None = 0,
        Operation = 100,

    }

    #region Storage
    public interface IStorable
    {
        public EStorableSize StorableSize { get; }
        public bool IsStored();
        public IStorable StoreIntoStorage(IStorage storage);
        public IStorable RemoveFromStorage(IStorage storage);
    }
    public interface IStorage
    {
        public bool IsFull();
        public bool CanStorableFit(IStorable storable);
        public bool TryStore(IStorable storable);
        public IStorable TryRemove(IStorable storable);
        public bool TryFind(IStorable storable);
        public List<IStorable> TryEmpty();
        public IStorage GetStorageParent();
    }

    [Serializable]
    public struct FStorable : IStorable
    {
        [SerializeField]
        private EStorableSize _storableSize;
        [SerializeField]
        private EStorableType _storableType;
        private IStorage _stored;

        public readonly EStorableSize StorableSize
        {
            get { return _storableSize; }
        }
        public readonly EStorableType StorableType
        {
            get { return _storableType; }
        }
        public IStorage Stored
        {
            readonly get { return _stored; }
            set { _stored = value; }
        }

        public bool IsStored()
        {
            return Stored != null;
        }

        public IStorable StoreIntoStorage(IStorage storage)
        {
            if (storage == null)
            {
                return null;
            }

            if (Stored != null)
            {
                return null;
            }

            Stored = storage;

            return this;
        }

        public IStorable RemoveFromStorage(IStorage storage)
        {
            IStorable storable = null;

            if (storage == null)
            {
                Stored = null;
                storable = this;
            }
            else
            {
                if (Stored == storage)
                {
                    Stored = null;
                    storable = this;
                }
            }

            return storable;
        }
    }
    [Serializable]
    public struct FStorageSlot : IStorage
    {
        [SerializeField] private EStorableSize _storableSize;
        [SerializeField] private List<EStorableType> _storableTypeFilter;
        [SerializeField] private Transform _storageSpace;
        [SerializeField] private IStorable _storable;
        private IStorage _storageParent;

        public EStorableSize StorableSize
        {
            get { return _storableSize; }
            set { _storableSize = value; }
        }
        public List<EStorableType> StorableTypeFilter
        {
            get { return _storableTypeFilter; }
            set { _storableTypeFilter = value; }
        }
        public Transform StorageSpace
        {
            get { return _storageSpace; }
            set { _storageSpace = value; }
        }
        public IStorable Storable
        {
            get { return _storable; }
            set { _storable = value; }
        }
        public IStorage StorageParent
        {
            get { return _storageParent; }
            set { _storageParent = value; }
        }

        public bool IsFull()
        {
            return Storable != null;
        }

        public bool CanStorableFit(IStorable storable)
        {
            return storable.StorableSize <= _storableSize;
        }

        public bool TryStore(IStorable storable)
        {
            if (storable == null)
            {
                return false;
            }

            bool store = !IsFull() && CanStorableFit(storable);

            if (store)
            {
                MonoBehaviour storableMono = storable as MonoBehaviour;
                if (storableMono != null)
                {
                    storableMono.gameObject.transform.SetParent(StorageSpace, false);
                }

                Storable = storable;
                storable.StoreIntoStorage(this);
            }

            return store;
        }

        public IStorable TryRemove(IStorable storable)
        {
            if (storable == null)
            {
                return Storable.RemoveFromStorage(this);
            }
            else 
            {
                if (TryFind(storable))
                {
                    return Storable.RemoveFromStorage(this);
                }
            }

            return null;
        }

        public bool TryFind(IStorable storable)
        {
            bool found = Storable == storable;
            return found;
        }

        public List<IStorable> TryEmpty()
        {
            List<IStorable> storablesEmptied = new List<IStorable>();

            if (Storable != null)
            {
                IStorable removed = Storable.RemoveFromStorage(this);

                if (removed != null)
                {
                    storablesEmptied.Add(removed);
                }
            }

            return storablesEmptied;
        }

        public IStorage GetStorageParent()
        {
            return StorageParent;
        }
    }
    #endregion

    public interface IEquip
    {
        
    }
}
