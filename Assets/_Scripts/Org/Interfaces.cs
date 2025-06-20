using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.General.Identification;
using _Scripts.Gameplay.General.Morgue.Operation.OperationState;
using _Scripts.Gameplay.Player.Controller;
using Unity.VisualScripting;
using UnityEngine;

namespace _Scripts.Org
{
    public interface IMorgueTickable
    {
        void Setup();
        void Tick();
    }

    public enum EMorgueStimulus
    {
        //OPERATION
        Operation_SuccessInput, 
        Operation_FailureInput, 
        Operation_CancelInput,
        COUNT
    }
    public interface IMorgueReactable
    {
        void OnReaction(EMorgueStimulus stimulus);
    }

    public interface IInteractable
    {
        bool IsInteractable(IInteractor interactor = null);
        bool OnInteract(IInteractor interactor = null);
    }

    public interface IInteractor
    {

    }

    public interface IOperatable
    {
        string RuntimeID { get; }
        IOperator Operator { get; }
        List<OperationState> OperationStates { get; }
        void StartOperation(OperationState opState, IOperator opOwner);
        void TickOperation(OperationState opState);
        void StopOperation(OperationState opState);
    }

    public interface IOperator
    {
        float OperatingSpeed { get; }
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
        public IStorage Stored { get; }
        public bool IsStored();
        public IStorable StoreIntoStorage(IStorage storage);
        public IStorable RemoveFromStorage(IStorage storage);
        public IStorable GetStorableParent();

    }
    public interface IStorage
    {
        public bool IsFull();
        public bool CanStorableFit(IStorable storable);
        public bool TryStore(IStorable storable);
        public IStorable TryRemove(IStorable storable);
        public bool TryFind(IStorable storable);
        public T GetStorable<T>() where T : class, IStorable;
        public List<IStorable> TryEmpty();
        public IStorage GetStorageParent();
        public Transform GetStorageSpace(IStorable storable);
    }

    [Serializable]
    public class FStorable : IStorable
    {
        [SerializeField]
        private EStorableSize _storableSize;
        [SerializeField]
        private EStorableType _storableType;
        private IStorage _stored;
        private IStorable _storableParent;
        public EStorableSize StorableSize
        {
            get { return _storableSize; }
        }
        public EStorableType StorableType
        {
            get { return _storableType; }
        }
        public IStorage Stored
        {
            get { return _stored; }
        }

        public IStorable StorableParent { get => _storableParent; set => _storableParent = value; }

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

            _stored = storage;

            return this;
        }

        public IStorable RemoveFromStorage(IStorage storage)
        {
            IStorable storable = null;

            if (storage == null)
            {
                _stored = null;
                storable = this;
            }
            else
            {
                if (Stored == storage)
                {
                    _stored = null;
                    storable = this;
                }
            }

            return storable;
        }

        public IStorable GetStorableParent()
        {
            return StorableParent;
        }
    }
    [Serializable]
    public class FStorageSlot : IStorage
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
                if (storable.IsStored())
                {
                    IStorable removed = storable.Stored.TryRemove(storable);

                    if (removed == Storable)
                    {
                        Storable = null;
                    }
                }

                IStorable stored = storable.StoreIntoStorage(this);

                if (stored != null)
                {
                    //MonoBehaviour storableMono = storable.GetStorableParent() as MonoBehaviour;
                    //if (storableMono != null)
                    //{
                    //    storableMono.gameObject.transform.SetParent(StorageSpace, true);
                    //    //storableMono.gameObject.transform.localPosition = Vector3.zero;
                    //    storableMono.gameObject.transform.rotation = StorageSpace.rotation;
                    //}

                    Storable = storable;
                }
                else
                {
                    store = false;
                }
            }

            return store;
        }

        public IStorable TryRemove(IStorable storable)
        {
            IStorable removed = null;
            if (storable == null)
            {
                if (Storable != null)
                {
                    removed = Storable.RemoveFromStorage(this);
                }
            }
            else 
            {
                if (TryFind(storable))
                {
                    removed = Storable.RemoveFromStorage(this);
                }
            }

            if (removed != null)
            {
                Storable = null;
            }

            return removed;
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
                    Storable = null;
                    storablesEmptied.Add(removed);
                }
            }

            return storablesEmptied;
        }

        public IStorage GetStorageParent()
        {
            return StorageParent;
        }

        public Transform GetStorageSpace(IStorable storable)
        {
            return StorageSpace;
        }

        public T GetStorable<T>() where T : class, IStorable
        {
            return _storable as T;
        }
    }
    #endregion

    public interface IConnectable
    {
        public bool IsConnected();
        public bool TryConnect(IConnectable child);
        public IConnectable ConnectToConnectable(IConnectable parent);
        public IConnectable TryDisconnect(IConnectable child);
        public void OnDisconnect(IConnectable parent);
        public IConnectable TryFindConnected(IConnectable child);
        public Transform Transform { get; }
        public IConnectable GetParentConnected();
    }

    [Serializable]
    public class FPropConnector : IConnectable
    {
        [SerializeField] private Transform _socket;
        public IConnectable _parentConnected;
        public IConnectable _childConnected;

        public IConnectable ParentConnectable
        {
            get { return _parentConnected; }
            set { _parentConnected = value; }
        }
        public IConnectable ChildConnectable
        {
            get { return _childConnected; }
            set { _childConnected = value; }
        }

        public Transform Transform
        {
            get { return _socket; }
        }

        public bool TryConnect(IConnectable child)
        {
            return false;
        }

        public IConnectable ConnectToConnectable(IConnectable parent)
        {
            if (_childConnected == null)
            {
                return null;
            }

            _socket = parent.Transform;
            _parentConnected = parent;

            return _childConnected;
        }

        public IConnectable TryDisconnect(IConnectable child)
        {
            return null;
        }

        public void OnDisconnect(IConnectable parent)
        {
            _parentConnected = null;
        }

        public IConnectable TryFindConnected(IConnectable child)
        {
            if (child == null)
            {
                return null;
            }

            if (_parentConnected == child.GetParentConnected())
            {
                return this;
            }

            return null;
        }

        public bool IsConnected()
        {
            return _parentConnected != null;
        }

        public IConnectable GetParentConnected()
        {
            return _parentConnected;
        }
    }

    #region TickGroup
    public interface ITickMaster
    {
        bool IsUsingTickMaster();
        Int16 GetTickID();
    }

    public interface ITickGroup
    {
        public UniqueTickGroup UniqueTickGroup { get; }
    }
    #endregion

    #region Identity
    public interface IIdentifiable
    {
        public RuntimeID RuntimeID { get; }
    }
    #endregion


    #region Volume
    public enum EVolumeOverride
    {
        None = 0,
        Bloom, 

    }
    #endregion
}
