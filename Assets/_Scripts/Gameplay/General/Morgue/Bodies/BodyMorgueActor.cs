using System.Collections;
using System.Collections.Generic;
using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.Input.InputController.Game;
using _Scripts.Gameplay.Player.Controller;
using _Scripts.Org;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue.Bodies{
    
    public class BodyMorgueActor : MorgueActor, IStorable, IInteractable
    {
        private Collider _collider;

        [SerializeField] private FStorable _bodyStorable;
        public EStorableSize StorableSize { get => _bodyStorable.StorableSize; }

        public bool IsStored()
        {
            return _bodyStorable.IsStored();
        }

        public IStorable StoreIntoStorage(IStorage storage)
        {
            return _bodyStorable.StoreIntoStorage(storage);
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
            else
            {
                Debug.Log("Operating on body");
            }
            return true;
        }
    }
    
}
