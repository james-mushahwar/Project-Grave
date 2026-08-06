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
using DG.Tweening;
using _Scripts.Gameplay.General.Morgue.Bodies;
using Random = UnityEngine.Random;

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

        //Free flow states 
        FreeFlow_Sawing = 300,

        //Contracts
        Contract_Book_Overview = 400,
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

        [Header("Noise settings")]
        [SerializeField]
        private NoiseSettings _noise_PlayerHeadBob;

        [Header("Camera impulses")]
        [SerializeField]
        private CinemachineImpulseSource _impulseSource_Operation_Success;
        [SerializeField]
        private CinemachineImpulseSource _impulseSource_Operation_FailLong;
        [SerializeField]
        private CinemachineImpulseSource _impulseSource_Operation_FailShort;

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

        [SerializeField] private FloatTweenerProfile _onSuccessInputFOVBehaviour;
        [SerializeField] private FloatTweenerProfile _onPenaltyInputFOVBehaviour;

        private float _defaultFOV;
        private float _targetXOffset;
        private float _targetYOffset;
        private float _targetZOffset;
        private Tweener _targetZOffsetTweener;

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
                _defaultFOV = MainCamera.fieldOfView;
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

                    if (PlayerManager.Instance.CurrentPlayerController.VCamFreeFlowSawing.VirtualCamera != null)
                    {
                        AssignVirtualCameraType(PlayerManager.Instance.CurrentPlayerController.RuntimeID, PlayerManager.Instance.CurrentPlayerController.VCamFreeFlowSawing.CamType, PlayerManager.Instance.CurrentPlayerController.VCamFreeFlowSawing.VirtualCamera);
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

                if (OperationManager.Instance.IsInOperationOverview() || OperationManager.Instance.IsOperating())
                {
                    if (PlayerManager.Instance.CurrentPlayerController.BodyPartMorgueActor != null && PlayerManager.Instance.CurrentPlayerController.OperatingTable != null)
                    {
                        OperatingTable opTable = PlayerManager.Instance.CurrentPlayerController.OperatingTable;
                        EMorgueBodyPart partType = PlayerManager.Instance.CurrentPlayerController.BodyPartMorgueActor.BodyPartType;

                        vCam = opTable.GetVirtualCamera(partType);
                        id = opTable.RuntimeID;
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
                else if (PlayerManager.Instance.CurrentPlayerController.PlayerControllerState == EPlayerControllerState.Contracts)
                {
                    vCamType = EVirtualCameraType.Contract_Book_Overview;
                    id = ContractsManager.Instance.ContractBook.RuntimeID;
                }
                else 
                {
                    vCamType = EVirtualCameraType.FirstPersonView_Normal;
                    id = PlayerManager.Instance.CurrentPlayerController.RuntimeID;
                }

                bool firstPersonCamera = (CmBrain.ActiveVirtualCamera != null && CmBrain.ActiveVirtualCamera.Equals(GetVirtualCamera(PlayerManager.Instance.CurrentPlayerController.RuntimeID, EVirtualCameraType.FirstPersonView_Normal)));
                bool contractView = vCamType == EVirtualCameraType.Contract_Book_Overview;
                
                {
                    Debug.Log("Activated New VCam");
                    Transform playerHolder = PlayerManager.Instance.CurrentPlayerController.PlayerCharacterHolder;
                    Transform newPlayerHolderParent = contractView ? null : GetVirtualCamera(PlayerManager.Instance.CurrentPlayerController.RuntimeID, EVirtualCameraType.FirstPersonView_Normal).transform;
                    if (playerHolder.parent != newPlayerHolderParent)
                    {
                        playerHolder.SetParent(newPlayerHolderParent);
                        Debug.Log("Set new player holder parent");
                    }
                }
                bool activate = false;
                if (vCam != null)
                {
                    activate = ActivateVirtualCamera(vCam);
                }
                else if (ActionSequenceManager.Instance.LockPlay == false )
                {
                    activate = ActivateVirtualCamera(id, vCamType);
                }


                //Noise settings
                NoiseSettings activeNoise = new NoiseSettings();
                if (CmBrain.ActiveVirtualCamera != null && CmBrain.ActiveVirtualCamera.Equals(GetVirtualCamera(PlayerManager.Instance.CurrentPlayerController.RuntimeID, EVirtualCameraType.FirstPersonView_Normal)))
                {
                    if (ActionSequenceManager.Instance.LockPlay == false)
                    {
                        activeNoise = _noise_PlayerHeadBob;
                    }
                }

                if (activeNoise != null)
                {
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
                _runtimeIdVirtualCameraDictionary[runtimeID.RuntimeId].TryAdd(cameraType, vCam);

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

                if (found && vCam != null)
                {
                    if ((CmBrain.ActiveVirtualCamera != null && !CmBrain.ActiveVirtualCamera.Equals(vCam)) && CmBrain.ActiveVirtualCamera.VirtualCameraGameObject != null)
                    {
                        CmBrain.ActiveVirtualCamera.VirtualCameraGameObject.SetActive(false);
                    }

                    if (CmBrain.ActiveVirtualCamera == null || (CmBrain.ActiveVirtualCamera != null && !CmBrain.ActiveVirtualCamera.Equals(vCam)))
                    {
                        vCam.gameObject.SetActive(true);
                        _cameraTransitionBuffer = true;
                        activated = true;
                        _currentVCamType = cameraType;
                    }
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

        public void OnSuccessfulInput()
        {
            _impulseSource_Operation_Success.GenerateImpulse();
            //old?
            KillActiveTween(ref _targetZOffsetTweener);
            float value = _onSuccessInputFOVBehaviour.IsValueAdditive ? _onSuccessInputFOVBehaviour.Value + _defaultFOV : _onSuccessInputFOVBehaviour.Value;

            TweenFOVOffset(ref _targetZOffsetTweener, _defaultFOV, value, _onSuccessInputFOVBehaviour.Duration, _onSuccessInputFOVBehaviour.Ease);
            _targetZOffsetTweener.OnComplete(() => MainCamera.fieldOfView = _defaultFOV);
        }

        public void OnPenaltyInput(bool critical = true)
        {
            Vector3 velocity = new Vector3(Random.Range(-0.01f, 0.01f), Random.Range(-0.01f, 0.01f), 0.0f);
            CinemachineImpulseSource impulseSource = critical ? _impulseSource_Operation_FailLong : _impulseSource_Operation_FailShort;
            impulseSource.GenerateImpulseWithVelocity(velocity);
            //old?
            KillActiveTween(ref _targetZOffsetTweener);
            float value = _onPenaltyInputFOVBehaviour.IsValueAdditive ? _onPenaltyInputFOVBehaviour.Value + _defaultFOV : _onPenaltyInputFOVBehaviour.Value;

            TweenFOVOffset(ref _targetZOffsetTweener, _defaultFOV, _onPenaltyInputFOVBehaviour.Value, _onPenaltyInputFOVBehaviour.Duration, _onPenaltyInputFOVBehaviour.Ease);
            _targetZOffsetTweener.OnComplete(() => MainCamera.fieldOfView = _defaultFOV);
        }

        public Vector3 GetLookDirection(Vector3 origin)
        {
            Vector3 direction = (origin - MainCamera.transform.position).normalized;

            Vector3 lookAt = Quaternion.LookRotation(direction).eulerAngles;

            return lookAt;
        }

        private void KillActiveTween(ref Tweener tweener)
        {
            if (tweener != null)
            {
                if (tweener.IsActive())
                {
                    DOTween.Kill(tweener);
                    tweener = null;
                }
            }
        }

        private void TweenFOVOffset(ref Tweener tweener, float from, float to, float duration, Ease easeType)
        {
            tweener = DOVirtual.Float(from, to, duration, (float value) =>
            {
                MainCamera.fieldOfView = value;
            }).SetEase(easeType);
        }
    }
    
}
