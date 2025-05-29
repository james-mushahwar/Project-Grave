using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _Scripts.CautionaryTalesScripts;
using Cinemachine;
using static UnityEngine.Rendering.HDROutputUtils;
using UnityEngine.InputSystem;
using _Scripts.Gameplay.Player.Controller;
using _Scripts.Gameplay.General.Identification;
using System;
using _Scripts.Gameplay.General.Morgue;
using UnityEditor;

namespace _Scripts.Gameplay.Architecture.Managers{

    public enum EVirtualCameraType : int
    {
        NONE = -1,
        //player
        FirstPersonView_Normal = 0,
        //scene
        OperatingTable_Above = 100,
        OperatingTable_Torso_Overview,
        OperatingTable_Head_Overview,
        OperatingTable_RArm_Overview,
        OperatingTable_RLeg_Overview,
        OperatingTable_LArm_Overview,
        OperatingTable_LLeg_Overview,

        //Operation states
        OperationState_Torso = 200,
        OperationState_Head,
        OperationState_RArm,
        OperationState_RLeg,
        OperationState_LArm,
        OperationState_LLeg,


        // Generic
        General_Default = 10000,
        General_Operation
    }

    [Serializable]
    public struct FVirtualCamera
    {
        [SerializeField]
        private EVirtualCameraType _camType;
        [SerializeField]
        private CinemachineVirtualCamera _virtualCamera;

        public EVirtualCameraType CamType { get => _camType; }
        public CinemachineVirtualCamera VirtualCamera { get => _virtualCamera; }
    }

    public class CameraManager : GameManager<CameraManager>, IManager
    {
        private Camera _mainCamera;
        public Camera MainCamera
        {
            get
            {
                if (_mainCamera == null)
                {
                    _mainCamera = Camera.main;
                }

                return _mainCamera;
            }
        }

        private CinemachineBrain _cmBrain;
        public CinemachineBrain CmBrain
        {
            get
            {
                if (_cmBrain == null)
                {
                    if (MainCamera != null)
                    {
                        _cmBrain = MainCamera.gameObject.GetComponent<CinemachineBrain>();
                    }
                }

                return _cmBrain;
            }
        }

        private EVirtualCameraType _currentVCamType;
        public EVirtualCameraType CurrentVCamType
        {
            get { return _currentVCamType; }
        }

        private bool _cameraTransitionBuffer;

        private Ray _centreCameraRay;
        public Ray CentreCameraRay
        {
            get => _centreCameraRay;
        }

        private Ray _mousePointerRay;
        public Ray MousePointerRay
        {
            get => _mousePointerRay;
        }

        public Ray CurrentRay
        {
            get
            {
                Ray ray = _centreCameraRay;

                if (PlayerManager.Instance.CurrentPlayerController.PlayerControllerState == EPlayerControllerState.Operating || 
                    PlayerManager.Instance.CurrentPlayerController.PlayerControllerState == EPlayerControllerState.OpenCoat) 
                {
                    ray = _mousePointerRay;
                }
                return ray;
            }
        }

        //private VirtualCameraTypeDictionary _virtualCameraTypeDictionary;
        private Dictionary<string, Dictionary<EVirtualCameraType, CinemachineVirtualCamera>> _runtimeIdVirtualCameraDictionary = new Dictionary<string, Dictionary<EVirtualCameraType, CinemachineVirtualCamera>>();
        //private RuntimeIDVirtualCameraDictionary _runtimeIdVirtualCameraDictionary;

        // as gamestate is being generated
        public virtual void ManagedPreInitialiseGameState() { }
        // after gamestate is generated
        public virtual void ManagedPostInitialiseGameState() 
        {
            if (MainCamera != null)
            {
                MainCamera.enabled = false;
            }

        }
        // before main menu loads
        public virtual void ManagedPreMainMenuLoad() { }
        // after main menu loads
        public virtual void ManagedPostMainMenuLoad() { }
        // before world (level, area, zone) starts loading
        public virtual void ManagedPreInGameLoad() { }
        // after world (level, area, zone) finished loading
        public virtual void ManagedPostInGameLoad()
        {
            if (PlayerManager.Instance.CurrentPlayerController != null)
            {
                GameObject go = CTGlobal.FindGameObjectInChildWithTag(PlayerManager.Instance.CurrentPlayerController.gameObject, "Camera_Holder");

                if (go != null)
                {
                    MainCamera.transform.SetParent(go.transform);
                    MainCamera.transform.localPosition = Vector3.zero;
                    MainCamera.transform.rotation = Quaternion.identity;

                    //CinemachineVirtualCamera firstPersonVCam = go.GetComponentInChildren<CinemachineVirtualCamera>();

                    FVirtualCamera vCam = PlayerManager.Instance.CurrentPlayerController.FirstPersonVCam;
                    if (vCam.VirtualCamera != null)
                    {
                        AssignVirtualCameraType(PlayerManager.Instance.CurrentPlayerController.RuntimeID, vCam.CamType, vCam.VirtualCamera);
                        _currentVCamType = EVirtualCameraType.FirstPersonView_Normal;
                    }
                }
            }
        }
        // save states are restored
        public virtual void ManagedRestoreSave() { }
        // after save states are restored
        public virtual void ManagedPostRestoreSave() { }
        // before play begins 
        public virtual void ManagedPrePlayGame() { }
        // tick for playing game 
        public virtual void ManagedTick()
        {
            _centreCameraRay = MainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            _mousePointerRay = MainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (CmBrain != null)
            {
                bool inTransition = IsCameraInTransition();

                if (inTransition && _cameraTransitionBuffer)
                {
                    _cameraTransitionBuffer = false;
                }

                bool inputState = !inTransition;

                InputManager.Instance.TryToggleAllInput(inputState);

                EVirtualCameraType vCamType = EVirtualCameraType.NONE;
                RuntimeID id = null;
                CinemachineVirtualCamera vCam = null;

                if (OperationManager.Instance.IsInOperationOverview())
                {
                    if (PlayerManager.Instance.CurrentPlayerController.BodyPartMorgueActor != null && PlayerManager.Instance.CurrentPlayerController.OperatingTable != null)
                    {
                        OperatingTable opTable = PlayerManager.Instance.CurrentPlayerController.OperatingTable;
                        vCamType = PlayerManager.Instance.CurrentPlayerController.BodyPartMorgueActor.OperationOverviewVirtualCamera.CamType;
                        id = opTable.RuntimeID;

                        vCam = opTable.GetVirtualCamera(vCamType);
                        //id = PlayerManager.Instance.CurrentPlayerController.BodyPartMorgueActor.RuntimeID;
                    }
                    //else if (PlayerManager.Instance.CurrentPlayerController.BodyPartMorgueActor == null)
                    //{
                    //    vCamType = EVirtualCameraType.FirstPersonView_Normal;
                    //    id = PlayerManager.Instance.CurrentPlayerController.RuntimeID;
                    //}
                }
                else if (OperationManager.Instance.IsOperating())
                {
                    if (OperationManager.Instance.CurrentOperationState != null)
                    {
                        vCamType = OperationManager.Instance.CurrentOperationState.OperationStateVirtualCamera.CamType;
                        id = OperationManager.Instance.CurrentOperationState.RuntimeID;
                    }
                }
                else
                {
                    vCamType = EVirtualCameraType.FirstPersonView_Normal;
                    id = PlayerManager.Instance.CurrentPlayerController.RuntimeID;
                }


                bool activate = false;
                if (vCam != null)
                {
                    activate = ActivateVirtualCamera(vCam);
                }
                else
                {
                    activate = ActivateVirtualCamera(id, vCamType);
                }

            }
        }
        // late update tick for playing game 
        public virtual void ManagedLateTick()
        {
            
        }

        public virtual void ManagedFixedTick()
        {

        }

        // before world (level, area, zone) starts unloading
        public virtual void ManagedPreTearddownGame() { }
        // after world (level, area, zone) unloading
        public virtual void ManagedPostTearddownGame() { }

        //public bool AssignVirtualCameraType(EVirtualCameraType cameraType, CinemachineVirtualCamera vCam)
        //{
        //    bool assign = false;

        //    if (_virtualCameraTypeDictionary.ContainsKey(cameraType) == false)
        //    {
        //        _virtualCameraTypeDictionary.Add(cameraType, vCam);
        //        assign = true;
        //    }

        //    return assign;
        //}
        public bool AssignVirtualCameraType(RuntimeID runtimeID, EVirtualCameraType cameraType, CinemachineVirtualCamera vCam)
        {
            bool assign = false;

            if (_runtimeIdVirtualCameraDictionary.ContainsKey(runtimeID.RuntimeId) == false)
            {
                _runtimeIdVirtualCameraDictionary.Add(runtimeID.RuntimeId, new Dictionary<EVirtualCameraType, CinemachineVirtualCamera>());
            }

            if (_runtimeIdVirtualCameraDictionary.ContainsKey(runtimeID.RuntimeId))
            {
                _runtimeIdVirtualCameraDictionary[runtimeID.RuntimeId].Add(cameraType, vCam);

                assign = true;
            }

            return assign;
        }

        public CinemachineVirtualCamera GetVirtualCamera(RuntimeID runtimeID, EVirtualCameraType cameraType)
        {
            CinemachineVirtualCamera vCam = null;

            if (_runtimeIdVirtualCameraDictionary.ContainsKey(runtimeID.RuntimeId))
            {
                 _runtimeIdVirtualCameraDictionary[runtimeID.RuntimeId].TryGetValue(cameraType, out vCam);
            }

            return vCam;
        }

        public bool ActivateVirtualCamera(RuntimeID runtimeID, EVirtualCameraType cameraType)
        {
            bool activated = false;

            if (_runtimeIdVirtualCameraDictionary.ContainsKey(runtimeID.RuntimeId))
            {
                CinemachineVirtualCamera vCam = null;
                bool found = _runtimeIdVirtualCameraDictionary[runtimeID.RuntimeId].TryGetValue(cameraType, out vCam);

                if (found && vCam != null && (!CmBrain.ActiveVirtualCamera.Equals(vCam)))
                {
                    if (CmBrain.ActiveVirtualCamera.VirtualCameraGameObject != null)
                    {
                        CmBrain.ActiveVirtualCamera.VirtualCameraGameObject.SetActive(false);
                    }

                    vCam.gameObject.SetActive(true);
                    _cameraTransitionBuffer = true;
                    activated = true;
                    _currentVCamType = cameraType;
                }
            }

            return activated;
        }

        public bool ActivateVirtualCamera(CinemachineVirtualCamera vCam)
        {
            bool activated = false;

            if (vCam != null && (!CmBrain.ActiveVirtualCamera.Equals(vCam)))
            {
                if (CmBrain.ActiveVirtualCamera.VirtualCameraGameObject != null)
                {
                    CmBrain.ActiveVirtualCamera.VirtualCameraGameObject.SetActive(false);
                }

                vCam.gameObject.SetActive(true);
                _cameraTransitionBuffer = true;
                activated = true;
                //_currentVCamType = cameraType;
            }
            
            return activated;
        }

        public bool IsCameraInTransition()
        {
            bool inTransition = false;

            if (_cameraTransitionBuffer == false)
            {
                if (CmBrain != null)
                {
                    if (CmBrain.IsBlending || CmBrain.ActiveBlend != null)
                    {
                        inTransition = true;
                    }
                }
            }
            else
            {
                inTransition = true;
            }

            return inTransition;
        }
    }
    
}
