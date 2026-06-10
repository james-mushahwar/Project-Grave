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
using static UnityEngine.Rendering.DebugUI;
using _Scripts.Gameplay.Animate.Player;
using DG.Tweening;
using _Scripts.Gameplay.General.Morgue.Operation.Tools.Profiles;
using static SerializableDictionary;

namespace _Scripts.Gameplay.Player.Controller{

    public enum EPlayerControllerState
    {
        NONE = -1,

        Normal = 0,

        Operating = 100,

        OpenCoat = 200,

        Contracts = 300,
    }

    public enum EOperationType
    {
        NONE = -1,

        Dismember = 0,

        Attaching = 100, //reattaching through stitching

        Stitching = 200, //cuts

        Forensic = 300,
    }

    public class PlayerController : MonoBehaviour, IPossess, IInteractor, IIdentifiable, IOperator, IToolUser
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float sprintSpeed = 10f;
        [SerializeField] private float jumpForce = 5f;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private LayerMask groundMask;

        [SerializeField]
        private CharacterController _characterController;
        private Vector3 _moveVector;
        private Vector3 _lookVector = Vector3.zero;
        private Vector3 _previousFacingDirection;
        private Vector3 velocity;
        private bool _isGrounded;
        private bool _isSprinting;
        private bool _isFirstMove = true;

        private float _operatingHorz;
        private float _operatingVert;

        private bool _inputLeftDirection;
        private bool _inputRightDirection;

        private EPlayerControllerState _playerControllerState = EPlayerControllerState.NONE;
        public EPlayerControllerState PlayerControllerState
        {
            get => _playerControllerState;
        }

        public EOperationType OperationType
        {
            get
            {
                if (ChosenOperationState != null)
                {
                    return ChosenOperationState.OperationType;
                }

                return EOperationType.NONE;
            }
        }

        [Header("Mouse Look Settings")]
        [SerializeField] private float mouseSensitivity = 100f;
        private float _xRotation = 0f;

        public InputController InputController { get; private set; }
        public void PrePossessState()
        {
            _characterController.detectCollisions = false;
            CharacterController.enabled = false;
        }

        public void PostPossessState()
        {
            CharacterController.enabled = true;
            _characterController.detectCollisions = true;
            _characterController.Move(Vector3.zero);

        }

        #region PlayerCharacter
        [SerializeField]
        private Transform _playerCharacterHolder;

        public Transform PlayerCharacterHolder
        {
            get { return _playerCharacterHolder; }
        }
        [SerializeField]
        private PlayerCharacterAnimator _playerCharacterAnimator;

        public PlayerCharacterAnimator PlayerCharacterAnimator
        {
            get { return _playerCharacterAnimator; }
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
        public OperatingTable OperatingTable
        { 
            get { return _operatingTable; } 
        }

        private MorgueToolActor _equippedOperatingTool;
        private BodyPartMorgueActor _bodyPartMorgueActor;

        public BodyPartMorgueActor BodyPartMorgueActor { get => _bodyPartMorgueActor; }

        private OperationSite _highlightedOperationSite;
        private OperationSite _selectedOperationSite;

        private OperationState _chosenOperationState;
        public OperationState ChosenOperationState 
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
        [SerializeField]
        private FVirtualCamera _vCamFreeFlowSawing;

        public FVirtualCamera FirstPersonVCam { get => _firstPersonVCam; }
        public FVirtualCamera VCamFreeFlowSawing { get => _vCamFreeFlowSawing; }

        public float OperatingSpeed
        {
            get 
            {
                return _playerCharacterAnimator.CurrentMomentum;
            }
        }

        public ETimingType OperatingTiming
        {
            get
            {
                return PlayerCharacterAnimator.OperationTimingZone;
            }
        }

        public CharacterController CharacterController
        {
            get
            {
                if (_characterController == null)
                {
                    _characterController = GetComponent<CharacterController>();
                }
                return _characterController;
            }
        }

        public float MoveSpeed { get => moveSpeed; }
        public Vector3 Velocity { get => velocity; }
        public Vector3 MoveVector { get => _moveVector; }
        public Vector3 FacingDirection { get => this.transform.forward; }
        public float FacingDirectionChange
        {
            get
            {
                if (PlayerControllerState != EPlayerControllerState.Normal)
                {
                    return 0.0f;
                }

                return Vector3.SignedAngle(FacingDirection, _previousFacingDirection, Vector3.up);
            }
        }

        private void Start()
        {
            _lookVector = Vector3.zero;
            _isFirstMove = true;

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

            IInteractable interactable = InputController.GetSelectedObject<IInteractable>(null, true);
            UIManager.Instance.ShowInteractReticle = interactable != null;
        }

        public void PossessLateTick()
        {
            if (PlayerControllerState == EPlayerControllerState.Normal)
            {
                _previousFacingDirection = FacingDirection;

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

        public bool CanPlayerCharacterMove()
        {
            bool blocked = ActionSequenceManager.Instance.LockPlay || MorgueManager.Instance.IsCriticalCoroutinePlaying;
            return _characterController.enabled && Application.isPlaying && Application.isFocused && !blocked;
        }

        private void HandleMovement()
        {
            if (!CanPlayerCharacterMove())
            {
                return;
            }

            float moveX = _moveVector.x; // Horizontal movement
            float moveZ = _moveVector.y; // Forward movement

            // Sprinting
            //if (Keyboard.current.leftShiftKey.isPressed)
            //{
            //    _isSprinting = true;
            //}
            //else
            //{
            //    _isSprinting = false;
            //}

            // Calculate movement direction
            Vector3 move = transform.right * moveX + transform.forward * moveZ;
            float speed = _isSprinting ? sprintSpeed : moveSpeed;
            _characterController.Move(move * speed * Time.deltaTime);
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

            if (!CanPlayerCharacterMove())
            {
                return;
            }

            Vector2 mouseInput = _lookVector;
            float mouseX = mouseInput.x * mouseSensitivity * Time.deltaTime;
            float mouseY = mouseInput.y * mouseSensitivity * Time.deltaTime;

            if (CameraManager.Instance.CmBrain.ActiveVirtualCamera != null)
            {
                GameObject vCameraGameObject = CameraManager.Instance.CmBrain.ActiveVirtualCamera.VirtualCameraGameObject;
                if (vCameraGameObject != null)
                {
                    vCameraGameObject.transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f); // Rotate the camera
                }
            }
            

            _xRotation -= mouseY;
            _xRotation = Mathf.Clamp(_xRotation, -90f, 90f); // Clamp the vertical rotation
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
            if (!CanPlayerCharacterMove())
            {
                return;
            }

            velocity.y += gravity * Time.deltaTime; // Apply gravity to the velocity
            _characterController.Move(velocity * Time.deltaTime); // Move the character with gravity
            velocity = _characterController.velocity;
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            if (!CanPlayerCharacterMove())
            {
                return;
            }

            if (PlayerCharacterAnimator.IsAnimationBlockingInput())
            {
                _moveVector = Vector2.zero;
                return;
            }
            _moveVector = context.ReadValue<Vector2>();
        }
        public void OnLook(InputAction.CallbackContext context)
        {
            if (!CanPlayerCharacterMove())
            {
                return;
            }

            if (PlayerCharacterAnimator.IsAnimationBlockingInput())
            {
                _lookVector = Vector2.zero;
                return;
            }
            //Debug.Log("Look Vec is: " + _lookVector);

            if (_isFirstMove)
            {
                _isFirstMove = false;
                return;
            }

            _lookVector = context.ReadValue<Vector2>();
        }
        public void OnInventory(InputAction.CallbackContext context)
        {
            ContractsManager.Instance.ShowPortableContract();

            return;
            bool openCoat = _playerControllerState == EPlayerControllerState.Normal;

            EPlayerControllerState nextState = openCoat ? EPlayerControllerState.OpenCoat : EPlayerControllerState.Normal;
            RequestPlayerControllerState(nextState);

            _playerStorage.ToggleCoatStorage(openCoat);
        }

        #region Contract select mode
        public void OnContracts_NavigateLR(InputAction.CallbackContext context)
        {
            bool inTransition = CameraManager.Instance.IsCameraInTransition();
            if (inTransition)
            {
                return;
            }

            Vector2 leftStickInput = context.ReadValue<Vector2>();
            _inputLeftDirection = leftStickInput.x < -0.1f;
            _inputRightDirection = leftStickInput.x > 0.1f;

            if (_inputLeftDirection)
            {
                Debug.Log("Go left");
                ContractsManager.Instance.SelectNextContract(false);
            }
            else if ( _inputRightDirection) 
            {
                Debug.Log("Go right");
                ContractsManager.Instance.SelectNextContract(true);

            }
        }

        public void OnContracts_Select(InputAction.CallbackContext context)
        {
            bool inTransition = CameraManager.Instance.IsCameraInTransition();
            if (inTransition)
            {
                return;
            }
            ContractsManager.Instance.ChooseContract();
        }

        private void OnContracts_Back(InputAction.CallbackContext context)
        {
            bool inTransition = CameraManager.Instance.IsCameraInTransition();
            if (inTransition)
            {
                return;
            }

            //leave camera view
            //leave input control mode
            RequestPlayerControllerState(EPlayerControllerState.Normal);
        }
        #endregion

        #region Operating

        public void Operating_OnNavigate(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();

            _operatingHorz = input.x;
            _operatingVert = input.y;
        }

        public void Operating_OnBack(InputAction.CallbackContext callbackContext)
        {
            if (CameraManager.Instance.IsCameraInTransition())
            {
                return;
            }

            if (PlayerCharacterAnimator.IsAnimationBlockingInput())
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
                //Debug.Log("Leaving Operating on body");
            }
            
            
        }

        public void Operating_OnAction(InputAction.CallbackContext callbackContext)
        {
            if (CameraManager.Instance.IsCameraInTransition())
            {
                return;
            }

            if (PlayerCharacterAnimator.IsAnimationBlockingInput())
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

            if (PlayerCharacterAnimator.IsAnimationBlockingInput())
            {
                return;
            }

            float opScroll = callbackContext.ReadValue<float>();

            //Debug.Log("Playercontroller: scroll = " + opScroll);
        }

        public void OperatingScroll(bool forward = true)
        {
            return;

            if (CameraManager.Instance.IsCameraInTransition())
            {
                return;
            }

            if (PlayerCharacterAnimator.IsAnimationBlockingInput())
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

        public void Operating_ActionLPressed(InputAction.CallbackContext callbackContext)
        {
            Debug.LogWarning("Action");

            //if (PlayerCharacterAnimator.IsAnimationBlockingInput())
            //{
            //    return;
            //}

            ActionL(callbackContext);

        }

        public void Operating_ActionLReleased(InputAction.CallbackContext callbackContext)
        {
            //if (PlayerCharacterAnimator.IsAnimationBlockingInput())
            //{
            //    return;
            //}

            ActionL(callbackContext);
        }

        private void ActionL(InputAction.CallbackContext callbackContext)
        {
            if (CameraManager.Instance.IsCameraInTransition())
            {
                return;
            }

            //if (PlayerCharacterAnimator.IsAnimationBlockingInput())
            //{
            //    return;
            //}

            bool operating = OperationManager.Instance.IsInAnyOperatingMode();
            bool pressed = callbackContext.started;
            if (operating)
            {
                if (_bodyPartMorgueActor != null)
                {
                    if (ChosenOperationState != null)
                    {
                        ChosenOperationState.OnActionLInput(pressed);
                        _playerCharacterAnimator.OnActionLInput(pressed);
                    }
                }
            }
        }

        public void Operating_ActionRPressed(InputAction.CallbackContext callbackContext)
        {
            ActionR(callbackContext);
        }

        public void Operating_ActionRReleased(InputAction.CallbackContext callbackContext)
        {
            ActionR(callbackContext);
        }

        public void ActionR(InputAction.CallbackContext callbackContext)
        {
            if (CameraManager.Instance.IsCameraInTransition())
            {
                return;
            }

            if (PlayerCharacterAnimator.IsAnimationBlockingInput())
            {
                return;
            }

            bool operating = OperationManager.Instance.IsInAnyOperatingMode();
            bool pressed = callbackContext.ReadValueAsButton();

            if (operating)
            {
                if (_bodyPartMorgueActor != null)
                {
                    if (ChosenOperationState != null)
                    {
                        ChosenOperationState.OnActionRInput(pressed);
                        //_playerCharacterAnimator.OnActionLInput(pressed);
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
                        //Debug.Log("Hey I'm North");
                        OperationManager.Instance.ScrollOperationSite(true);
                    }
                    else
                    {
                        //Debug.Log("Hey I'm East");
                        OperationManager.Instance.ScrollOperationState(true);
                    }
                }
                else
                {
                    // negative - south and west
                    if (vertInput)
                    {
                        //Debug.Log("Hey I'm South");
                        OperationManager.Instance.ScrollOperationSite(false);
                    }
                    else
                    {
                        //Debug.Log("Hey I'm West");
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

            //Debug.Log("Stored old tool =  " + (storedOldTool ? "YES" : "NO"));
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

        public void ClearInput()
        {
            _moveVector = Vector3.zero;
            _lookVector = Vector3.zero;
            velocity = Vector3.zero;
            _characterController.Move(Vector3.zero);
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

            if (PlayerCharacterAnimator.IsAnimationBlockingInput())
            {
                return;
            }

            //Debug.Log("Action input");
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
                if (OperationManager.Instance.CurrentOperationSite != null)
                {
                    if (_chosenOperationState == null)
                    {
                        BeginOperatingState();
                        return;
                    }
                }
            }
            else 
            {
                // empty handed
                BodyPartMorgueActor bodyPart = InputController.GetSelectedObject<BodyPartMorgueActor>();
                BodyMorgueActor bodyMorgueActor = null;
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
                    bodyMorgueActor = bodyPart.BodyMorgueActor;
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

                //holding body parts
                //attach back to body
                GameObject selectedObject = InputController.SelectedObject;

                if (selectedObject != null)
                {
                    if (selectedObject.layer == LayerMask.NameToLayer("MorgueActor"))
                    {
                        if (bodyMorgueActor == null)
                        {
                            bodyMorgueActor = selectedObject.GetComponentInParent<BodyMorgueActor>();
                        }

                        if (bodyMorgueActor != null)
                        {
                            TorsoMorgueActor torsoBodyPart = bodyMorgueActor.TorsoMorgueActor;

                            if (torsoBodyPart != null)
                            {
                                IStorage hands = PlayerStorage.GetPlayerHands();
                                BodyPartMorgueActor heldBodyPart = hands.GetStorable<BodyPartMorgueActor>();
                                if (heldBodyPart != null)//&& EquippedOperatingTool as OperationAttachmentMorgueTool)
                                {
                                    //if (selectedObject.tag == heldBodyPart.gameObject.tag)
                                    {
                                        IStorable removed = hands.TryRemove(heldBodyPart);
                                        if (removed != null)
                                        {
                                            heldBodyPart.TryConnect(bodyMorgueActor.TorsoMorgueActor);
                                        }
                                    }
                                }
                            }
                        }


                    }
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

        public void Event_ExamineSaw_Pickup()
        {
            PlayerCharacterAnimator.PlayExamineSawAnimation();
        }

        public void Event_TryEquipTool(MorgueToolActor tool)
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

                bool stored = nextStorage.TryStore(tool);

                if (stored)
                {
                    EquippedOperatingTool = tool;
                    tool.gameObject.SetActive(true);
                    tool.transform.localEulerAngles = Vector3.zero;
                }
            }
        }

        public void Event_TryUnequipTool()
        {
            if (EquippedOperatingTool)
            {
                ReturnOperatingToolToSlot(EquippedOperatingTool);
                EquippedOperatingTool.gameObject.SetActive(false);
            }
            PlayerCharacterAnimator.PlayExamineSawEndAnimation();
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

        public void BeginOperatingState()
        {
            //if (CameraManager.Instance.ActivateVirtualCamera(EVirtualCameraType.OperatingTable_Above))
            //{
                
            //}
            if (OperationManager.Instance.CurrentOperationState == null)
            {
                Debug.LogError("No chosen operation state, can't begin op state");
                return;
            }
            MorgueToolActor toolToUse = _playerStorage.GetToolOfType(OperationManager.Instance.CurrentOperationState.OperationType);

            if (toolToUse == null)
            {
                Debug.LogError("No valid tool to equip for operation state! Operation type is: " + OperationManager.Instance.CurrentOperationState.OperationType);
                return;
            }

            _chosenOperationState = OperationManager.Instance.CurrentOperationState;

            _bodyPartMorgueActor.StartOperation(_chosenOperationState, this);

            ClearInput();

            AudioManager.Instance.TransitionToSnapshot(EAudioSnapshot.Operation_Calm, 0.5f);
            //_bodyPartMorgueActor.OperationState.BeginOperationState();

            if (toolToUse != null)
            {
                bool equipped = toolToUse.OnInteract(this);

                if (equipped)
                {
                    toolToUse.gameObject.SetActive(true);
                    toolToUse.SetVisible(true);
                }
            }

            //RequestPlayerControllerState(EPlayerControllerState.Operating);

            AnimationManager.Instance.StartOperationState(_bodyPartMorgueActor);

            //play equip saw animation
            PlayerCharacterAnimator.PlayPickupToolAnimation();

            //BodyMorgueActor storedBody = _operatingTable.GetStorable<BodyMorgueActor>();
            //if (storedBody != null)
            //{
            //    storedBody.ToggleCollision(false);
            //}
        }

        public void EndOperatingState(bool leaveOpEntirely = false)
        {
            //if (CameraManager.Instance.ActivateVirtualCamera(EVirtualCameraType.FirstPersonView_Normal))
            //{

            //}
            FeedbackManager.Instance.StopFeedbackPattern();

            AudioManager.Instance.TransitionToSnapshot(EAudioSnapshot.Default, 0.5f);

            bool leaveOpTable = false;

            if (_bodyPartMorgueActor != null)
            {
                _bodyPartMorgueActor.StopOperation(_chosenOperationState);
                if (_bodyPartMorgueActor.BodyMorgueActor == null)
                {
                    _bodyPartMorgueActor = null;
                    leaveOpTable = true;
                }
            }
            
            if (_chosenOperationState == null)
            {
                leaveOpTable = true;
            }

            if (_chosenOperationState != null)
            {
                _chosenOperationState = null;
                AnimationManager.Instance.EndOperationState(_bodyPartMorgueActor);
            }
            
            if (leaveOpTable || leaveOpEntirely)
            {
                if (_operatingTable != null)
                {
                    BodyMorgueActor storedBody = _operatingTable.GetStorable<BodyMorgueActor>();
                    if (storedBody != null)
                    {
                        storedBody.ToggleCollision(true);
                    }
                }

                ClearInput();

                ReturnOperatingToolToSlot(EquippedOperatingTool);

                _operatingTable = null;

                //_chosenOperationState = null;

                //AnimationManager.Instance.EndOperationState(_bodyPartMorgueActor);

                _bodyPartMorgueActor = null;

                RequestPlayerControllerState(EPlayerControllerState.Normal);
            }
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

                        mpi.Game.Operating_ActionL.started += Operating_ActionLPressed;
                        mpi.Game.Operating_ActionL.canceled += Operating_ActionLReleased;

                        mpi.Game.Operating_ActionR.started += Operating_ActionRPressed;
                        mpi.Game.Operating_ActionR.canceled += Operating_ActionRReleased;

                        mpi.Game.Operating_Cycle.Enable();
                        //mpi.Game.Operating_Scroll.started += Operating_OnScroll;
                        if (gameIC)
                        {
                            mpi.Game.Operating_Cycle.started += gameIC.Operating_OnCycle;
                        }
                        //mpi.Game.Operating_Scroll.canceled += ctx => _opScroll = 0.0f;

                        
                        mpi.Game.Operating_Navigate.Enable();
                        mpi.Game.Operating_Navigate.performed += Operating_OnNavigate;
                        mpi.Game.Operating_Navigate.canceled += Operating_OnNavigate;

                        if (gameIC)
                        {
                            mpi.Game.Operating_ScrollVert.Enable();
                            mpi.Game.Operating_ScrollHorz.Enable();
                            mpi.Game.Operating_ScrollVert.performed += gameIC.Operating_ScrollVert;
                            mpi.Game.Operating_ScrollVert.canceled += gameIC.Operating_ScrollVert;
                            mpi.Game.Operating_ScrollHorz.performed += gameIC.Operating_ScrollHorz;
                            mpi.Game.Operating_ScrollHorz.canceled += gameIC.Operating_ScrollHorz;
                        }

                        break;
                    case EPlayerControllerState.OpenCoat:
                        cursorLock = CursorLockMode.Confined;

                        mpi.Game.Inventory.performed += OnInventory;
                        mpi.Game.Back.started += OnInventory;
                        break;
                    case EPlayerControllerState.Contracts:
                        cursorLock = CursorLockMode.Confined;

                        mpi.Game.Movement.performed += OnContracts_NavigateLR;
                        mpi.Game.Select.started += OnContracts_Select;
                        mpi.Game.Back.started += OnContracts_Back;
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

                        mpi.Game.Operating_ActionL.started -= Operating_ActionLPressed;
                        mpi.Game.Operating_ActionL.canceled -= Operating_ActionLReleased;

                        mpi.Game.Operating_ActionR.started -= Operating_ActionRPressed;
                        mpi.Game.Operating_ActionR.canceled -= Operating_ActionRReleased;
                        
                        mpi.Game.Operating_Cycle.Disable();
                        //mpi.Game.Operating_Scroll.started -= Operating_OnScroll;
                        if (gameIC)
                        {
                            mpi.Game.Operating_Cycle.started -= gameIC.Operating_OnCycle;
                        }
                        
                        mpi.Game.Operating_Navigate.Disable();
                        mpi.Game.Operating_Navigate.performed -= Operating_OnNavigate;
                        mpi.Game.Operating_Navigate.canceled -= Operating_OnNavigate;

                        mpi.Game.Operating_ScrollVert.Disable();
                        mpi.Game.Operating_ScrollHorz.Disable();

                        if (gameIC)
                        {
                            mpi.Game.Operating_ScrollVert.performed -= gameIC.Operating_ScrollVert;
                            mpi.Game.Operating_ScrollVert.canceled -= gameIC.Operating_ScrollVert;
                            mpi.Game.Operating_ScrollHorz.performed -= gameIC.Operating_ScrollHorz;
                            mpi.Game.Operating_ScrollHorz.canceled -= gameIC.Operating_ScrollHorz;
                        }

                        break;

                    case EPlayerControllerState.OpenCoat:
                        mpi.Game.Inventory.performed -= OnInventory;
                        mpi.Game.Back.started -= OnInventory;
                        break;

                    case EPlayerControllerState.Contracts:
                        mpi.Game.Movement.performed -= OnContracts_NavigateLR;
                        mpi.Game.Select.started -= OnContracts_Select;
                        mpi.Game.Back.started -= OnContracts_Back;
                        break;

                    default:
                        break;
                }

            }

            _playerControllerState = EPlayerControllerState.NONE;
        }

        public EOperationMinigameState GetToolUserState()
        {
            bool inFreeFlow = ChosenOperationState.OpMinigame.GetInFreeFlow();

            return inFreeFlow ? EOperationMinigameState.FreeFlow : EOperationMinigameState.BuildingMomentum;
        }

        public int GetBuildMomentumCounts()
        {
            bool isOperating = ChosenOperationState != null;
            return isOperating && ChosenOperationState.OpMinigame.CheckOperationState(EOperationMinigameState.BuildingMomentum) ? ChosenOperationState.OpMinigame.GetMomentumChecks() : -1;
        }

        public void TriggerDrowsy()
        {
            VolumeManager.Instance.SetPlayerDrowsy(true);
        }
    }
}

