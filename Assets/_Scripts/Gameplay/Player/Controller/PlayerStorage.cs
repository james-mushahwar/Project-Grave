using System.Collections;
using System.Collections.Generic;
using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Org;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Scripts.Gameplay.Player.Controller{
    
    public class PlayerStorage : MonoBehaviour, IMorgueTickable
    {
        [SerializeField]
        private bool _leftHanded = false;
        [SerializeField]
        private PlayerHands _hands;
        [SerializeField] 
        private PlayerHands _operatingHands;
        [SerializeField]
        private Transform _operatingHandsRoot;

        [SerializeField] private float _operatingHandsRootOffset;

        private List<IStorage> _pockets = new List<IStorage>();

        public FStorageSlot GetPrimaryHand(EPlayerControllerState forceState = EPlayerControllerState.NONE)
        {
            EPlayerControllerState playerState = forceState != EPlayerControllerState.NONE ? forceState : PlayerManager.Instance.CurrentPlayerController.PlayerControllerState;

            bool isOperating = (playerState ==
                                EPlayerControllerState.Operating);
            if (isOperating)
            {
                return (_leftHanded ? _operatingHands.LHand : _operatingHands.RHand);
            }
            else
            {
                return (_leftHanded ? _hands.LHand : _hands.RHand);
            }
        }
        public FStorageSlot GetSecondaryHand(EPlayerControllerState forceState = EPlayerControllerState.NONE)
        {
            EPlayerControllerState playerState = forceState != EPlayerControllerState.NONE ? forceState : PlayerManager.Instance.CurrentPlayerController.PlayerControllerState;

            bool isOperating = (playerState ==
                                EPlayerControllerState.Operating);
            if (isOperating)
            {
                return (_leftHanded ? _operatingHands.RHand : _operatingHands.LHand);
            }
            else
            {
                return (_leftHanded ? _hands.RHand : _hands.LHand);
            }
        }

        public IStorage GetNextBestStorage(bool singleHand = true, EPlayerControllerState forceState = EPlayerControllerState.NONE)
        {
            FStorageSlot hand = GetPrimaryHand(forceState);

            if (singleHand)
            {
                return hand;
            }
            else
            {
                if (hand.IsFull())
                {
                    return GetSecondaryHand(forceState);
                }
            }

            return null;
        }

        public IStorage GetPlayerHands(EStorableType force = EStorableType.None)
        {
            if (force == EStorableType.None)
            {
                return _hands;
            }

            if (force == EStorableType.Operation)
            {
                return _operatingHands;
            }

            return null;
        }

        public void Setup()
        {
        }

        public void Tick()
        {
            // operating hands position
            bool isOperating = (PlayerManager.Instance.CurrentPlayerController.PlayerControllerState ==
                                       EPlayerControllerState.Operating);

            _operatingHandsRoot.gameObject.SetActive(isOperating);

            if (isOperating)
            {
                Vector3 cameraDir = CameraManager.Instance.CentreCameraRay.direction;

                Vector2 mousePos = Mouse.current.position.ReadValue();
                //Ray ray = Camera.main.ScreenPointToRay(mousePos);
                Vector3 displacement = CameraManager.Instance.MainCamera.ScreenPointToRay(mousePos).origin;

                //Vector3 newPosition = (CameraManager.Instance.MainCamera.transform.position + displacement) + (cameraDir * _operatingHandsRootOffset);
                Vector3 newPosition = (displacement) + (cameraDir * _operatingHandsRootOffset);

                if (_operatingHandsRoot != null)
                {
                    _operatingHandsRoot.position = newPosition;

                    _operatingHandsRoot.rotation = Quaternion.LookRotation(new Vector3(cameraDir.x, 0.0f, cameraDir.z), CameraManager.Instance.MainCamera.transform.forward);
                }
            }
            //If you get an error with the above line, replace it with this:
            //mousePosition = new Vector3(mousePosition.x, mousePosition.y, zAxis);
        }

        public bool TryStore(EPlayerControllerState forceState = EPlayerControllerState.NONE)
        {
            bool stored = false;

            return stored;
        }

        public bool TryRemove(EPlayerControllerState forceState = EPlayerControllerState.NONE)
        {
            bool stored = false;

            return stored;
        }
    }
    
}
