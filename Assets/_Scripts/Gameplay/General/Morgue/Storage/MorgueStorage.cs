using System.Collections;
using System.Collections.Generic;
using _Scripts.Gameplay.Animate.JitterAnimation;
using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.General.Morgue.Bodies;
using _Scripts.Gameplay.Player.Controller;
using _Scripts.Org;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue.Storage{
    
    public class MorgueStorage : MorgueActor, IStorage, IInteractable, ISubmission
    {
        [SerializeField] protected FStorageSlot _singleSlot;

        [SerializeField] protected JitterBehaviour _jitterBehaviour;

        [SerializeField] protected bool _updateJitter;

        private bool _isHookStorage = false;

        public FStorageSlot StorageSlot
        {
            get { return _singleSlot; }
        }

        public bool IsHookStorage { get => _isHookStorage; }

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

            _isHookStorage = gameObject.tag == "Storage_Hook";
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

        public bool OnSubmitted(MorgueContract contract)
        {
            bool complete = false;

            //populate submitted
            if (contract == null)
            {
                return false;
            }

            //only body submissions
            if (contract.ContractType != EContractType.Body)
            {
                return false;
            }

            contract.Submitted._bodyPart.Clear();

            BodyPartMorgueActor bodyPart = StorageSlot.Storable as BodyPartMorgueActor;

            if (bodyPart != null)
            {
                contract.Submitted._bodyPart.Add(bodyPart.BodyPartType);
            }

            if (contract.Submitted._bodyPart.Count != contract.Requirements._bodyPart.Count)
            {
                return false;
            }

            foreach (EMorgueBodyPart bodyPartType in contract.Submitted._bodyPart)
            {
                if (contract.Requirements._bodyPart.Contains(bodyPartType) == false)
                {
                    return false;
                }
            }

            return true;
        }

        public void ClearSubmission()
        {
            List<IStorable> emptied = TryEmpty();
            foreach (IStorable storable in emptied)
            {
                GameObject storableGO = (storable.GetStorableParent() as MonoBehaviour).gameObject;

                if (storableGO != null)
                {
                    Destroy(storableGO);
                }
            }
        }
    }
    
}
