using System.Collections;
using System.Collections.Generic;
using _Scripts.Gameplay.General.Identification;
using _Scripts.Gameplay.Player.Controller;
using _Scripts.Org;
using UnityEditor;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue.Operation.Tools{
    
    public abstract class MorgueToolActor : MorgueActor, IStorable, IInteractable
    {
        [SerializeField] protected FStorable _toolStorable;

        [SerializeField] protected float _lerpMoveSpeed;
        public ref FStorable ToolStorable { get { return ref _toolStorable; } }
        //public ref FStorageSlot DefaultStorage { get { return ref _defaultStorage; } }
        public EStorableSize StorableSize { get => _toolStorable.StorableSize; }

        public IStorage Stored => _toolStorable.Stored;

        public override void Setup()
        {
            RuntimeID = GetComponent<RuntimeID>();
            if (RuntimeID != null)
            {
                RuntimeID.GenerateRuntimeId();
            }

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
                    Vector3 worldScale = this.gameObject.transform.lossyScale;
                    Vector3 worldPosition = this.gameObject.transform.position;

                    this.gameObject.transform.SetParent(storage.GetStorageSpace(_toolStorable), true);
                    //storableMono.gameObject.transform.localPosition = Vector3.zero;
                    Transform storageSpace = storage.GetStorageSpace(storable);
                    //this.gameObject.transform.rotation = storageSpace.rotation;
                    //this.gameObject.transform.position = worldPosition;
                    //this.gameObject.transform.lossyScale = worldScale;
                    //this.gameObject.transform.localScale = worldScale ;
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

        public virtual bool IsInteractable(IInteractor interactor = null)
        {
            PlayerController pc = interactor as PlayerController;

            if (interactor == null)
            {
                return false;
            }
            if (pc != null)
            {
                if (pc.EquippedOperatingTool == this)
                {
                    return false;
                }
            }

            return true;
        }

        public virtual bool OnInteract(IInteractor interactor = null)
        {
            PlayerController pc = interactor as PlayerController;

            if (pc != null)
            {
                IStorage nextStorage = pc.PlayerStorage.GetNextBestStorage();
                if (nextStorage != null)
                {
                    IStorable prevStored = nextStorage.TryRemove(null);
                    if (prevStored != null)
                    {
                        MorgueToolActor oldTool = prevStored.GetStorableParent() as MorgueToolActor;
                        if (oldTool != null)
                        {
                            pc.ReturnOperatingToolToSlot(oldTool);
                        }
                    }

                    bool stored = nextStorage.TryStore(this);
                    if (stored)
                    {
                        pc.EquippedOperatingTool = this;
                    }
                }

            }

            return true;
        }
    }
    
}
