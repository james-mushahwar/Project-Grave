using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _Scripts.CautionaryTalesScripts;
using Cinemachine;
using static UnityEngine.Rendering.HDROutputUtils;

namespace _Scripts.Gameplay.Architecture.Managers{

    public enum EVirtualCameraType : uint
    {
        //player
        FirstPersonView_Normal = 0,
        //scene
        OperatingTable_Above = 100,
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

        private bool _cameraTransitionBuffer;

        [SerializeField] private VirtualCameraTypeDictionary _virtualCameraTypeDictionary;

        // as gamestate is being generated
        public virtual void ManagedPreInitialiseGameState() { }
        // after gamestate is generated
        public virtual void ManagedPostInitialiseGameState() 
        {
            if (MainCamera != null)
            {
                
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

                    CinemachineVirtualCamera firstPersonVCam =
                        go.GetComponentInChildren<CinemachineVirtualCamera>();

                    if (firstPersonVCam != null)
                    {
                        AssignVirtualCameraType(EVirtualCameraType.FirstPersonView_Normal, firstPersonVCam);
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
        }
        // late update tick for playing game 
        public virtual void ManagedLateTick()
        {
            if (CmBrain != null)
            {
                bool inTransition = IsCameraInTransition();

                if (inTransition && _cameraTransitionBuffer)
                {
                    _cameraTransitionBuffer = false;
                }

                bool inputState = !inTransition;

                InputManager.Instance.TryToggleAllInput(inputState);
            }
        }

        public virtual void ManagedFixedTick()
        {

        }

        // before world (level, area, zone) starts unloading
        public virtual void ManagedPreTearddownGame() { }
        // after world (level, area, zone) unloading
        public virtual void ManagedPostTearddownGame() { }

        public bool AssignVirtualCameraType(EVirtualCameraType cameraType, CinemachineVirtualCamera vCam)
        {
            bool assign = false;

            if (_virtualCameraTypeDictionary.ContainsKey(cameraType) == false)
            {
                _virtualCameraTypeDictionary.Add(cameraType, vCam);
                assign = true;
            }

            return assign;
        }

        public bool ActivateVirtualCamera(EVirtualCameraType cameraType)
        {
            bool activated = false;

            if (_virtualCameraTypeDictionary.ContainsKey(cameraType))
            {
                CinemachineVirtualCamera vCam =_virtualCameraTypeDictionary[cameraType];
                if (vCam != null)
                {
                    if (CmBrain.ActiveVirtualCamera.VirtualCameraGameObject != null)
                    {
                        CmBrain.ActiveVirtualCamera.VirtualCameraGameObject.SetActive(false);
                    }

                    vCam.gameObject.SetActive(true);
                    _cameraTransitionBuffer = true;
                    activated = true;
                }
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
