using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.Input.InputController.Game;
using _Scripts.Gameplay.Player.Controller;
using _Scripts.Org;
using UnityEditor;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue.Bodies{
    
    public class BodyMorgueActor : MorgueActor, IStorable, IInteractable
    {
        private Collider _collider;

        [SerializeField] private FStorable _bodyStorable;
        public EStorableSize StorableSize { get => _bodyStorable.StorableSize; }

        public IStorage Stored => _bodyStorable.Stored;

        private HeadMorgueActor _headMorgueActor;

        [SerializeField]
        private GameObject _bodyGeometryGO;
        [SerializeField]
        private GameObject _bodyRigGO;

        public bool IsStored()
        {
            return _bodyStorable.IsStored();
        }

        public IStorable StoreIntoStorage(IStorage storage)
        {
            IStorable storable = _bodyStorable.StoreIntoStorage(storage);
            if (storable != null)
            {
                if (_bodyStorable.GetStorableParent() != null)
                {
                    MonoBehaviour storableMono = _bodyStorable.GetStorableParent() as MonoBehaviour;
                    if (storableMono != null)
                    {
                        Vector3 localScale = storableMono.transform.localScale;
                        storableMono.gameObject.transform.SetParent(storage.GetStorageSpace(_bodyStorable), false);
                        //storableMono.gameObject.transform.localPosition = Vector3.zero;
                        //storableMono.gameObject.transform.rotation = StorageSpace.rotation;
                        //storableMono.gameObject.transform.localScale = localScale;
                    }
                }
            }

            return storable;
        }

        public IStorable RemoveFromStorage(IStorage storage)
        {
            return _bodyStorable.RemoveFromStorage(storage);
        }

        private Collider Collider
        {
            get
            {
                if (_collider == null)
                {
                    _collider = GetComponent<Collider>();
                }

                return _collider;
            }
        }


        public override void EnterHouseThroughChute()
        {
            Debug.Log("Try enter house through chute");
            Animation anim = MorgueManager.Instance.TryEnterHouseChuteAnimation(this);
            if (anim != null)
            {
                CurrentAnimation = anim;
            }
        }

        public override void Setup()
        {
            _bodyStorable.StorableParent = this;
        }

        public override void Tick()
        {
        }

        public override void ToggleProne(bool set)
        {
            if (Collider != null)
            {
                Collider.isTrigger = set;
            }
        }

        public bool IsInteractable()
        {
            bool interact = false;

            bool normal = PlayerManager.Instance.CurrentPlayerController.PlayerControllerState ==
                          EPlayerControllerState.Normal;
            bool operating = PlayerManager.Instance.CurrentPlayerController.PlayerControllerState ==
                             EPlayerControllerState.Operating;

            if (_bodyStorable.Stored != null)
            {
                OperatingTable opTable = _bodyStorable.Stored.GetStorageParent() as OperatingTable;
                if (opTable != null)
                {
                    interact = true;
                }
            }

            return interact;
        }

        public bool OnInteract()
        {
            //Debug.Log("Interact with body");

            bool normal = PlayerManager.Instance.CurrentPlayerController.PlayerControllerState ==
                          EPlayerControllerState.Normal;
            bool operating = PlayerManager.Instance.CurrentPlayerController.PlayerControllerState ==
                             EPlayerControllerState.Operating;
            OperatingTable opTable = _bodyStorable.Stored.GetStorageParent() as OperatingTable;

            if (normal)
            {
                if (CameraManager.Instance.ActivateVirtualCamera(EVirtualCameraType.OperatingTable_Above))
                {
                    PlayerManager.Instance.CurrentPlayerController.BeginOperatingState(opTable);
                }
            }
            else if (operating)
            {
                
            }
            
            return true;
        }

        public IStorable GetStorableParent()
        {
            return this;
        }

        public override void ToggleCollision(bool set)
        {
            if (Collider != null)
            {
                Collider.enabled = set;
            }
        }

        public BodyPartMorgueActor GetBodyPartByTag(string tag)
        {
            BodyPartMorgueActor[] bodyParts = _bodyGeometryGO.GetComponentsInChildren<BodyPartMorgueActor>(true);
            foreach (var bodyPart in bodyParts)
            {
                if (String.Equals(bodyPart.tag, tag, StringComparison.CurrentCultureIgnoreCase))
                {
                    return bodyPart;
                }
            }

            return null;
        }
    }
    
}
