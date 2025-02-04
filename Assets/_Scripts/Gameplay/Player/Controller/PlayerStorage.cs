using System.Collections;
using System.Collections.Generic;
using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Org;
using Unity.Mathematics;
using UnityEngine;

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

        public FStorageSlot GetPrimaryHand()
        {
            bool isOperating = (PlayerManager.Instance.CurrentPlayerController.PlayerControllerState ==
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
        public FStorageSlot GetSecondaryHand()
        {
            bool isOperating = (PlayerManager.Instance.CurrentPlayerController.PlayerControllerState ==
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

        public IStorage GetNextBestStorage(bool singleHand = true)
        {
            FStorageSlot hand = GetPrimaryHand();

            if (singleHand)
            {
                return hand;
            }
            else
            {
                if (hand.IsFull())
                {
                    return GetSecondaryHand();
                }
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

                Vector3 newPosition = CameraManager.Instance.MainCamera.transform.position +
                                      (cameraDir * _operatingHandsRootOffset);

                if (_operatingHandsRoot != null)
                {
                    _operatingHandsRoot.position = newPosition;

                    _operatingHandsRoot.rotation = Quaternion.LookRotation(new Vector3(cameraDir.x, 0.0f, cameraDir.y), Vector3.up);
                }
            }
            //If you get an error with the above line, replace it with this:
            //mousePosition = new Vector3(mousePosition.x, mousePosition.y, zAxis);
        }
    }
    
}
