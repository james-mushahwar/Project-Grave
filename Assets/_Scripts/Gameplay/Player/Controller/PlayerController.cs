using System;
using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.Input.InputController;
using _Scripts.Gameplay.Input.InputController.Game;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using _Scripts.Editortools.Draw;
using _Scripts.Gameplay.General.Morgue;
using _Scripts.Gameplay.General.Morgue.Operation.Tools;
using UnityEngine.InputSystem;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using _Scripts.Org;
using _Scripts.Gameplay.General.Morgue.Bodies;
using Unity.VisualScripting;
using UnityEditor;
using Cinemachine;
using _Scripts.Gameplay.General.Morgue.Operation.OperationState;
using _Scripts.Gameplay.General.Morgue.Operation.OperationSite;
using IIdentifiable = _Scripts.Org.IIdentifiable;
using _Scripts.Gameplay.General.Identification;

namespace _Scripts.Gameplay.Player.Controller{

    public enum EPlayerControllerState
    {
        NONE = -1,

        Normal = 0,

        Operating = 100,

        OpenCoat = 200,
    }

    public enum EOperationType
    {
        NONE = -1,

        Dismember = 0,

        Attaching = 100, //reattaching through stitching

        Stitching = 200, //cuts

        Forensic = 300,
    }

    public class PlayerController : MonoBehaviour, IPossess, IInteractor, IIdentifiable
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float sprintSpeed = 10f;
        [SerializeField] private float jumpForce = 5f;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private LayerMask groundMask;

        private CharacterController _characterController;
        private Vector3 _moveVector;
        private Vector3 _lookVector;
        private Vector3 velocity;
        private bool _isGrounded;
        private bool _isSprinting;

        private EPlayerControllerState _playerControllerState = EPlayerControllerState.NONE;
        public EPlayerControllerState PlayerControllerState
        {
            get => _playerControllerState;
        }

        public EOperationType OperationType
        {
            get
            {
                if (CurrentOperationState != null)
                {
                    return CurrentOperationState.OperationType;
                }

                return EOperationType.NONE;
            }
        }

        [Header("Mouse Look Settings")]
        [SerializeField] private float mouseSensitivity = 100f;
        private float xRotation = 0f;

        public InputController InputController { get; private set; }

        #region PlayerCharacter
        [SerializeField]
        private Transform _playerCharacterHolder;

        public Transform PlayerCharacterHolder
        {
            get { return _playerCharacterHolder; }
        }
        #endregion

        #region PlayerStorage
        [SerializeField] private PlayerStorage _playerStorage;
        public PlayerStorage PlayerStorage
        {
            get { return _playerStorage; }
        }

        #endregion

        #region Operating
        //private float _opScroll;
        private OperatingTable _operatingTable;
        private MorgueToolActor _equippedOperatingTool;
        private BodyPartMorgueActor _bodyPartMorgueActor;

        public BodyPartMorgueActor BodyPartMorgueActor { get => _bodyPartMorgueActor; }

        private OperationSite _highlightedOperationSite;
        private OperationSite _selectedOperationSite;

        private OperationState _chosenOperationState;
        public OperationState CurrentOperationState 
        { 
            get 
            {
                //if (_bodyPartMorgueActor != null && _playerControllerState == EPlayerControllerState.Operating)
                //{
                //    return _bodyPartMorgueActor.OperationState;
                //}

                return _chosenOperationState;
            } 
        }

        public MorgueToolActor EquippedOperatingTool
        {
            get { return _equippedOperatingTool; }
            set { _equippedOperatingTool = value; }
        }

        #endregion

        [SerializeField]
        private RuntimeID _runtimeID;
        public RuntimeID RuntimeID => _runtimeID;

        [SerializeField]
        private FVirtualCamera _firstPersonVCam;

        public FVirtualCamera FirstPersonVCam { get => _firstPersonVCam; }

        private void Start()
        {
            _characterController = GetComponent<CharacterController>();

            InputManager.Instance.PossessPlayer(this);
        }

        public void PossessTick()
        {
            _isGrounded = _characterController.isGrounded;
            //isGrounded = Physics.CheckSphere(groundCheck.position, 0.1f, groundMask);

            if (_isGrounded && velocity.y < 0)
            {
                velocity.y = -2f; // Reset the vertical velocity when grounded
            }

            IInteractable interactable = InputController.GetSelectedObject<IInteractable>();
            UIManager.Instance.ShowInteractReticle = interactable != null;
        }

        public void PossessLateTick()
        {
            if (PlayerControllerState == EPlayerControllerState.Normal)
            {
                HandleRotation();
                HandleMovement();
                HandleJump();
                ApplyGravity();
            }
        }

        public void PossessFixedTick()
        {
            bool operating = OperationManager.Instance.IsInAnyOperatingMode();

            if (InputController.CheckAndNullifyInput(EInputType.SButton))
            {
                OnActionInput();
                return;
            }

            //scroll
            if (operating)
            {
                if (InputController.CheckAndNullifyInput(EInputType.LBumper))
                {
                    OperatingScroll(false);
                    return;
                }
                else if (InputController.CheckAndNullifyInput(EInputType.RBumper))
                {
                    OperatingScroll(true);
                    return;
                }

                Vector2 dpadDirection = InputController.DPadInput;
                if (InputController.CheckAndNullifyInput(EInputType.DPadN) || InputController.CheckAndNullifyInput(EInputType.DPadS) || InputController.CheckAndNullifyInput(EInputType.DPadE) || InputController.CheckAndNullifyInput(EInputType.DPadW))
                {
                    Operating_OnDPadInput(dpadDirection);
                }
            }
        }

        public void OnDrawGizmos()
        {
            //DrawGizmos.ForArrowGizmo()
        }
        private void HandleMovement()
        {
            float moveX = _moveVector.x; // Horizontal movement
            float moveZ = _moveVector.y; // Forward movement

            // Sprinting
            if (Keyboard.current.leftShiftKey.isPressed)
            {
                _isSprinting = true;
            }
            else
            {
                _isSprinting = false;
            }

            // Calculate movement direction
            Vector3 move = transform.right * moveX + transform.forward * moveZ;
            float speed = _isSprinting ? sprintSpeed : moveSpeed;
            _characterController.Move(move.normalized * speed * Time.deltaTime);
        }

        private void HandleRotation()
        {
            if (_lookVector.sqrMagnitude <= Single.MinValue)
            {
                return;
            }

            if (CameraManager.Instance.CmBrain == null)
            {
                return;
            }

            if (CameraManager.Instance.IsCameraInTransition() || _playerControllerState != EPlayerControllerState.Normal)
            {
                return;
            }

            Vector2 mouseInput = _lookVector;
            float mouseX = mouseInput.x * mouseSensitivity * Time.deltaTime;
            float mouseY = mouseInput.y * mouseSensitivity * Time.deltaTime;

            GameObject vCameraGameObject = CameraManager.Instance.CmBrain.ActiveVirtualCamera.VirtualCameraGameObject;
            if (vCameraGameObject != null)
            {
                vCameraGameObject.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f); // Rotate the camera
            }

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Clamp the vertical rotation
            transform.Rotate(Vector3.up * mouseX); // Rotate the player
        }

        private void HandleJump()
        {
            return;

            if (_isGrounded && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                velocity.y += Mathf.Sqrt(jumpForce * -2f * gravity); // Calculate jump force
            }
        }

        private void ApplyGravity()
        {
            velocity.y += gravity * Time.deltaTime; // Apply gravity to the velocity
            _characterController.Move(velocity * Time.deltaTime); // Move the character with gravity
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            _moveVector = context.ReadValue<Vector2>();
        }
        public void OnLook(InputAction.CallbackContext context)
        {
            _lookVector = context.ReadValue<Vector2>();
        }
        public void OnInventory(InputAction.CallbackContext context)
        {
            bool openCoat = _playerControllerState == EPlayerControllerState.Normal;

            EPlayerControllerState nextState = openCoat ? EPlayerControllerState.OpenCoat : EPlayerControllerState.Normal;
            RequestPlayerControllerState(nextState);

            _playerStorage.ToggleCoatStorage(openCoat);
        }

        #region Operating
        public void Operating_OnBack(InputAction.CallbackContext callbackContext)
        {
            if (CameraManager.Instance.IsCameraInTransition())
            {
                return;
            }

            bool operating = OperationManager.Instance.IsInAnyOperatingMode();

            Debug.Log("Attempt leave");

            if (operating)
            {
                //if (CameraManager.Instance.CmBrain.ActiveVirtualCamera != (ICinemachineCamera)CameraManager.Instance.GetVirtualCamera(EVirtualCameraType.OperatingTable_Above))
                //{
                //    // attempt leave focused body part
                //    bool backToOperatingAbove = CameraManager.Instance.ActivateVirtualCamera(EVirtualCameraType.OperatingTable_Above);
                //    if (backToOperatingAbove)
                //    {
                //        Debug.Log("Back to above operating cameraview");

                //    }
                //}
                //else
                EndOperatingState();

                //if (CameraManager.Instance.ActivateVirtualCamera(EVirtualCameraType.FirstPersonView_Normal))
                //{
                //    RequestPlayerControllerState(EPlayerControllerState.Normal);

                //    BodyMorgueActor storedBody = _operatingTable.GetStorable<BodyMorgueActor>();
                //    if (storedBody != null)
                //    {
                //        storedBody.ToggleCollision(true);
                //    }

                //    if (EquippedOperatingTool != null)
                //    {
                //        //ReturnOperatingToolToSlot(EquippedOperatingTool);
                //    }
                //    _operatingTable = null;
                //}
            }
            else
            {
                Debug.Log("Leaving Operating on body");
            }
            
            
        }

        public void Operating_OnAction(InputAction.CallbackContext callbackContext)
        {
            if (CameraManager.Instance.IsCameraInTransition())
            {
                return;
            }
        }

        public void Operating_OnScroll(InputAction.CallbackContext callbackContext)
        {
            if (CameraManager.Instance.IsCameraInTransition())
            {
                return;
            }

            float opScroll = callbackContext.ReadValue<float>();

            Debug.Log("Playercontroller: scroll = " + opScroll);
        }

        public void OperatingScroll(bool forward = true)
        {
            return;

            if (CameraManager.Instance.IsCameraInTransition())
            {
                return;
            }

            if (_operatingTable == null)
            {
                return;
            }

            int toolsCount = _operatingTable.OperatingToolsCount;
            int toolIndex = _operatingTable.GetOperatingToolIndex(EquippedOperatingTool);

            int newIndex = EquippedOperatingTool == null ? (forward ? 0 : toolsCount - 1) : toolIndex + (forward ? -1 : 1);

            if (newIndex < 0)
            {
                newIndex = toolsCount - 1;
            }
            else if (newIndex == toolsCount)
            {
                newIndex = 0;
            }

            MorgueToolActor newTool = _operatingTable.GetOperatingTool(newIndex);

            if (newTool != null)
            {
                IStorage nextStorage = _playerStorage.GetNextBestStorage();
                if (nextStorage != null)
                {
                    IStorable prevStored = nextStorage.TryRemove(null);
                    if (prevStored != null)
                    {
                        MorgueToolActor oldTool = prevStored.GetStorableParent() as MorgueToolActor;
                        if (oldTool != null)
                        {
                            ReturnOperatingToolToSlot(oldTool);
                        }
                    }

                    bool stored = nextStorage.TryStore(newTool);
                    if (stored)
                    {
                        EquippedOperatingTool = newTool;
                    }
                }
            }

            //Debug.Log("Index is now : " + newIndex);
        }

        public void Operating_ActionL(InputAction.CallbackContext callbackContext)
        {
            if (CameraManager.Instance.IsCameraInTransition())
            {
                return;
            }

            bool operating = OperationManager.Instance.IsInAnyOperatingMode();

            if (operating)
            {
                OperationMorgueToolActor opTool = EquippedOperatingTool as OperationMorgueToolActor; 

                if (opTool != null && _bodyPartMorgueActor != null)
                {
                    bool dismemberOperation = opTool is OperationDismemberMorgueTool;
                    if (dismemberOperation == false)
                    {
                        return;
                    }

                    if (CurrentOperationState != null)
                    {
                        if (EquippedOperatingTool.IsAnimating())
                        {
                            return;
                        }

                        bool proceed = CurrentOperationState.OnActionLInput();
                        if (proceed)
                        {
                            CurrentOperationState.ProceedOperation(1.0f);

                            EquippedOperatingTool.Animate();
                        }
                    }
                }
            }

        }

        public void Operating_ActionR(InputAction.CallbackContext callbackContext)
        {
            if (CameraManager.Instance.IsCameraInTransition())
            {
                return;
            }

            bool operating = OperationManager.Instance.IsInAnyOperatingMode();

            if (operating)
            {
                OperationMorgueToolActor opTool = EquippedOperatingTool as OperationMorgueToolActor;

                if (opTool != null && _bodyPartMorgueActor != null)
                {
                    bool dismemberOperation = opTool is OperationDismemberMorgueTool;
                    if (dismemberOperation == false)
                    {
                        return;
                    }

                    if (CurrentOperationState != null)
                    {
                        if (EquippedOperatingTool.IsAnimating())
                        {
                            return;
                        }

                        bool proceed = CurrentOperationState.OnActionRInput();
                        if (proceed)
                        {
                            CurrentOperationState.ProceedOperation(1.0f);

                            EquippedOperatingTool.Animate();
                        }
                    }
                }
            }

        }

        public void Operating_OnDPadInput(Vector2 dPadInput)
        {
            if (dPadInput.SqrMagnitude() == 0.0f)
            {
                return;
            }

            float direction = dPadInput.x != 0.0f ? dPadInput.x : dPadInput.y;
            bool vertInput = dPadInput.y != 0.0f;

            bool operatingOverview = OperationManager.Instance.IsInOperationOverview();

            if (operatingOverview)
            {
                if (direction > 0.0f)
                {
                    // positive - north and east
                    if (vertInput)
                    {
                        Debug.Log("Hey I'm North");
                        OperationManager.Instance.ScrollOperationSite(true);
                    }
                    else
                    {
                        Debug.Log("Hey I'm East");
                        OperationManager.Instance.ScrollOperationState(true);
                    }
                }
                else
                {
                    // negative - south and west
                    if (vertInput)
                    {
                        Debug.Log("Hey I'm South");
                        OperationManager.Instance.ScrollOperationSite(false);
                    }
                    else
                    {
                        Debug.Log("Hey I'm West");
                        OperationManager.Instance.ScrollOperationState(false);
                    }
                }
            }
        }

        public bool ReturnOperatingToolToSlot(MorgueToolActor opTool)
        {
            if (opTool == null)
            {
                return false;
            }

            //int oldToolIndex = _operatingTable.GetOperatingToolIndex(opTool);
            //FStorageSlot opTableToolSlot = _operatingTable.GetOperatingToolStorageSlot(oldToolIndex);
            //bool storedOldTool = opTableToolSlot.TryStore(opTool);

            bool storedOldTool = _playerStorage.TryStore(opTool, EPlayerControllerState.OpenCoat);

            Debug.Log("Stored old tool =  " + (storedOldTool ? "YES" : "NO"));
            if (storedOldTool)
            {
                EquippedOperatingTool = null;
            }

            return storedOldTool;
        }
        #endregion

        public bool AttemptPossess(InputController controller)
        {
            if (controller is GameInputController == false)
            {
                return false;
            }
            Possess(controller);
            return true;
        }

        public bool AttemptUnpossess(InputController controller)
        {
            if (controller.Possessed != this)
            {
                return false;
            }
            Unpossess(controller);
            return true;
        }

        protected void Possess(InputController controller)
        {
            InputController = controller;

            if (InputManager.Instance != null)
            {
                EnterPlayerControllerState(EPlayerControllerState.Normal);
            }
        }

        protected void Unpossess(InputController controller)
        {
            if (InputManager.Instance != null)
            {
                ExitPlayerControllerState();
            }

            InputController = null;
        }

        public T GetInputController<T>() where T : InputController
        {
            T inputController = null;

            if (InputController as T)
            {
                inputController = InputController as T;
            }

            return inputController;
        }

        public void OnActionInput()
        {
            if (CameraManager.Instance.IsCameraInTransition())
            {
                return;
            }

            Debug.Log("Action input");
            bool operating = OperationManager.Instance.IsInAnyOperatingMode();

            //if (operating)
            //{
            //    BodyPartMorgueActor bodyPart = GetSelectedObject<BodyPartMorgueActor>();
            //    if (bodyPart == null)
            //    {
            //        bodyPart = GetSelectedObjectParent<BodyPartMorgueActor>();
            //    }

            //    if (bodyPart != null)
            //    {
            //        Debug.Log("Found body part = " + bodyPart.gameObject.name);
            //        if (CameraManager.Instance.CmBrain.ActiveVirtualCamera != (ICinemachineCamera)bodyPart.VirtualCamera)
            //        {
            //            Debug.Log("Activating body part cinecam = " + bodyPart.gameObject.name);

            //            CameraManager.Instance.ActivateVirtualCamera(bodyPart.RuntimeID);
            //            return;
            //        }

            //        OperationDismemberMorgueTool dismemberTool = EquippedOperatingTool as OperationDismemberMorgueTool;
            //        if (dismemberTool != null)
            //        {
            //            if (bodyPart.IsConnected())
            //            {
            //                IConnectable disconnectedPart = bodyPart.TryDisconnect(null);

            //                if (disconnectedPart != null)
            //                {
            //                    if (CameraManager.Instance.CmBrain.ActiveVirtualCamera == (ICinemachineCamera)bodyPart.VirtualCamera)
            //                    {
            //                        bool backToOperatingAbove = CameraManager.Instance.ActivateVirtualCamera(EVirtualCameraType.OperatingTable_Above);
            //                        if (backToOperatingAbove)
            //                        {
            //                            Debug.Log("Back to above operating cameraview");

            //                        }
            //                    }

            //                    IStorage nextPlayerStorage = _playerStorage.GetNextBestStorage(true, EPlayerControllerState.Normal);
            //                    if (nextPlayerStorage != null)
            //                    {
            //                        IStorable prevStored = nextPlayerStorage.TryRemove(null);
            //                        if (prevStored != null)
            //                        {
            //                            //MorgueToolActor oldTool = prevStored.GetStorableParent() as MorgueToolActor;
            //                            //if (oldTool != null)
            //                            //{
            //                            //    ReturnOperatingToolToSlot(oldTool);
            //                            //}
            //                        }

            //                        bool stored = nextPlayerStorage.TryStore(bodyPart);
            //                        if (stored)
            //                        {
            //                            Debug.Log("Stored disconnected part successfully");
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //    else
            //    {
            //        GameObject selectedObject = InputController.SelectedObject;

            //        if (selectedObject != null)
            //        {
            //            if (selectedObject.layer == LayerMask.NameToLayer("MorgueCollision"))
            //            {
            //                BodyMorgueActor bodyMorgueActor = selectedObject.GetComponentInParent<BodyMorgueActor>();

            //                if (bodyMorgueActor != null)
            //                {
            //                    IStorage hands = PlayerStorage.GetPlayerHands();
            //                    BodyPartMorgueActor heldBodyPart = hands.GetStorable<BodyPartMorgueActor>();
            //                    if (heldBodyPart != null && EquippedOperatingTool as OperationAttachmentMorgueTool)
            //                    {
            //                        if (selectedObject.tag == heldBodyPart.gameObject.tag)
            //                        {
            //                            IStorable removed = hands.TryRemove(heldBodyPart);
            //                            if (removed != null)
            //                            {
            //                                heldBodyPart.TryConnect(bodyMorgueActor.TorsoMorgueActor);
            //                            }
            //                        }
            //                    }

            //                }


            //            }
            //        }
            //    }

            //}
            if (operating)
            {

            }
            else
            {
                BodyPartMorgueActor bodyPart = InputController.GetSelectedObject<BodyPartMorgueActor>();
                if (bodyPart == null)
                {
                    bodyPart = InputController.GetSelectedObjectParent<BodyPartMorgueActor>();

                    if (bodyPart == null)
                    {
                        bodyPart = MorgueManager.Instance.GetBodyPartActorParent(InputController.SelectedObject);
                    }
                }

                if (bodyPart != null)
                {
                    // is it a body part that can be inspected? - on operating table?
                    if (bodyPart.IsConnected() && bodyPart.OperationState != null)
                    {
                        OperatingTable opTable = bodyPart.BodyMorgueActor.Stored.GetStorageParent() as OperatingTable;
                        if (opTable != null)
                        {
                            BeginOperatingOverview(opTable, bodyPart);
                            return;
                        }
                    }

                    //OperationDismemberMorgueTool dismemberTool = EquippedOperatingTool as OperationDismemberMorgueTool;
                    //if (dismemberTool != null)
                    //{
                    //    if (bodyPart.IsConnected() && bodyPart.OperationState != null)
                    //    {
                    //        //OperatingTable opTable = bodyPart.BodyMorgueActor.Stored.GetStorageParent() as OperatingTable;
                    //        BeginOperatingState(opTable, bodyPart);
                    //        return;
                    //    }
                    //}
                }

                IInteractable interactable = InputController.GetSelectedObject<IInteractable>();
                if (interactable == null)
                {
                    interactable = InputController.GetSelectedObjectParent<IInteractable>();
                }

                if (interactable != null)
                {
                    if (interactable.IsInteractable(this))
                    {
                        interactable.OnInteract(this);
                        return;
                    }
                }

                IStorable storable = InputController.GetSelectedObject<IStorable>();
                if (storable == null)
                {
                    storable = InputController.GetSelectedObjectParent<IStorable>();
                }

                if (storable != null)
                {
                    if (storable.IsStored() == false)
                    {
                        IStorage nextStorage = PlayerStorage.GetNextBestStorage();
                        if (nextStorage != null)
                        {
                            IStorable prevStored = nextStorage.TryRemove(null);
                            if (prevStored != null)
                            {
                                MorgueToolActor oldTool = prevStored.GetStorableParent() as MorgueToolActor;
                                if (oldTool != null)
                                {
                                    ReturnOperatingToolToSlot(oldTool);
                                }
                            }

                            bool stored = nextStorage.TryStore(storable);
                        }
                        return;
                    }
                }

            }
        }

        public void BeginOperatingOverview(OperatingTable opTable, BodyPartMorgueActor bodyPart)
        {
            //EVirtualCameraType cameraType = EVirtualCameraType.OperatingTable_Above;
            //if (bodyPart.OperationCameraType != EVirtualCameraType.NONE)
            //{
            //    cameraType = bodyPart.OperationCameraType;
            //}

            //if (CameraManager.Instance.ActivateVirtualCamera(cameraType))
            //{
                
            //}

            _operatingTable = opTable;

            _bodyPartMorgueActor = bodyPart;

            _chosenOperationState = null;

            OperationManager.Instance.OnStartBodyPartOperationOverview(bodyPart);

            //bodyPart.OperationState.BeginOperationState();

            RequestPlayerControllerState(EPlayerControllerState.Operating);

            //AnimationManager.Instance.StartOperationState(bodyPart);

            BodyMorgueActor storedBody = _operatingTable.GetStorable<BodyMorgueActor>();
            if (storedBody != null)
            {
                storedBody.ToggleCollision(false);
            }
        }

        public void BeginOperatingState(OperatingTable opTable, BodyPartMorgueActor bodyPart)
        {
            //if (CameraManager.Instance.ActivateVirtualCamera(EVirtualCameraType.OperatingTable_Above))
            //{
                
            //}

            _operatingTable = opTable;

            _bodyPartMorgueActor = bodyPart;

            _chosenOperationState = OperationManager.Instance.CurrentOperationState;

            bodyPart.OperationState.BeginOperationState();

            //RequestPlayerControllerState(EPlayerControllerState.Operating);

            AnimationManager.Instance.StartOperationState(bodyPart);

            //BodyMorgueActor storedBody = _operatingTable.GetStorable<BodyMorgueActor>();
            //if (storedBody != null)
            //{
            //    storedBody.ToggleCollision(false);
            //}
        }

        public void EndOperatingState()
        {
            //if (CameraManager.Instance.ActivateVirtualCamera(EVirtualCameraType.FirstPersonView_Normal))
            //{
                
            //}

            if (_operatingTable != null)
            {
                BodyMorgueActor storedBody = _operatingTable.GetStorable<BodyMorgueActor>();
                if (storedBody != null)
                {
                    storedBody.ToggleCollision(true);
                }
            }

            _operatingTable = null;

            _chosenOperationState = null;

            AnimationManager.Instance.EndOperationState(_bodyPartMorgueActor);

            _bodyPartMorgueActor = null;

            RequestPlayerControllerState(EPlayerControllerState.Normal);
        }

        public void RequestPlayerControllerState(EPlayerControllerState state)
        {
            if (_playerControllerState != EPlayerControllerState.NONE)
            {
                ExitPlayerControllerState();
            }

            EnterPlayerControllerState(state);
        }

        private void EnterPlayerControllerState(EPlayerControllerState state)
        {
            if (state == EPlayerControllerState.NONE)
            {
                return;
            }

            CursorLockMode cursorLock = CursorLockMode.Locked;

            MasterPlayerInput mpi = InputManager.Instance.MasterPlayerInput;

            GameInputController gameIC = InputManager.Instance.GetInputController<GameInputController>();

            if (mpi != null)
            {
                switch (state)
                {
                    case EPlayerControllerState.Normal:
                        
                        mpi.Game.Movement.performed += OnMove;
                        mpi.Game.Movement.canceled += OnMove;

                        mpi.Game.Look.performed += OnLook;
                        mpi.Game.Look.canceled += OnLook;

                        mpi.Game.Inventory.performed += OnInventory;
                        break;

                    case EPlayerControllerState.Operating:
                        cursorLock = CursorLockMode.Confined;
                        //mpi.Game.Action.RemoveAllBindingOverrides();
                        mpi.Game.Back.started += Operating_OnBack;
                        mpi.Game.Operating_ActionL.started += Operating_ActionL;
                        mpi.Game.Operating_ActionR.started += Operating_ActionR;

                        mpi.Game.Operating_Cycle.Enable();
                        //mpi.Game.Operating_Scroll.started += Operating_OnScroll;
                        if (gameIC)
                        {
                            mpi.Game.Operating_Cycle.started += gameIC.Operating_OnCycle;
                        }
                        //mpi.Game.Operating_Scroll.canceled += ctx => _opScroll = 0.0f;

                        mpi.Game.Operating_ScrollVert.Enable();
                        mpi.Game.Operating_ScrollHorz.Enable();

                        if (gameIC)
                        {
                            mpi.Game.Operating_ScrollVert.started += gameIC.Operating_ScrollVert;
                            mpi.Game.Operating_ScrollHorz.started += gameIC.Operating_ScrollHorz;
                        }

                        break;
                    case EPlayerControllerState.OpenCoat:
                        cursorLock = CursorLockMode.Confined;

                        mpi.Game.Inventory.performed += OnInventory;
                        mpi.Game.Back.started += OnInventory;
                        break;
                    default:
                        break;
                }
            }

            _playerControllerState = state;

            Cursor.lockState = cursorLock; // Lock the cursor to the center of the screen
        }

        private void ExitPlayerControllerState()
        {
            if (_playerControllerState == EPlayerControllerState.NONE)
            {
                return;
            }

            MasterPlayerInput mpi = InputManager.Instance.MasterPlayerInput;

            GameInputController gameIC = InputManager.Instance.GetInputController<GameInputController>();

            if (mpi != null)
            {
                switch (_playerControllerState)
                {
                    case EPlayerControllerState.Normal:
                        
                        mpi.Game.Movement.performed -= OnMove;
                        mpi.Game.Movement.canceled -= OnMove;

                        mpi.Game.Look.performed -= OnLook;
                        mpi.Game.Look.canceled -= OnLook;

                        mpi.Game.Inventory.performed -= OnInventory;
                        break;

                    case EPlayerControllerState.Operating:
                        mpi.Game.Back.started -= Operating_OnBack;
                        mpi.Game.Operating_ActionL.started -= Operating_ActionL;
                        mpi.Game.Operating_ActionR.started -= Operating_ActionR;
                        
                        mpi.Game.Operating_Cycle.Disable();
                        //mpi.Game.Operating_Scroll.started -= Operating_OnScroll;
                        if (gameIC)
                        {
                            mpi.Game.Operating_Cycle.started -= gameIC.Operating_OnCycle;
                        }
                        mpi.Game.Operating_ScrollVert.Disable();
                        mpi.Game.Operating_ScrollHorz.Disable();

                        if (gameIC)
                        {
                            mpi.Game.Operating_ScrollVert.started -= gameIC.Operating_ScrollVert;
                            mpi.Game.Operating_ScrollHorz.started -= gameIC.Operating_ScrollHorz;
                        }

                        break;

                    case EPlayerControllerState.OpenCoat:
                        mpi.Game.Inventory.performed -= OnInventory;
                        mpi.Game.Back.started -= OnInventory;
                        break;

                    default:
                        break;
                }

            }

            _playerControllerState = EPlayerControllerState.NONE;
        }
    }
}

