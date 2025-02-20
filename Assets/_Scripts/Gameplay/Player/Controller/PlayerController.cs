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

namespace _Scripts.Gameplay.Player.Controller{

    public enum EPlayerControllerState
    {
        NONE = -1,

        Normal = 0,

        Operating = 100,
    }

    public class PlayerController : MonoBehaviour, IPossess
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float sprintSpeed = 10f;
        [SerializeField] private float jumpForce = 5f;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private LayerMask groundMask;

        private CharacterController characterController;
        private Vector3 _moveVector;
        private Vector3 _lookVector;
        private Vector3 velocity;
        private bool isGrounded;
        private bool isSprinting;

        private EPlayerControllerState _playerControllerState = EPlayerControllerState.NONE;
        public EPlayerControllerState PlayerControllerState
        {
            get => _playerControllerState;
        }

        [Header("Mouse Look Settings")]
        [SerializeField] private float mouseSensitivity = 100f;
        private float xRotation = 0f;

        public InputController InputController { get; private set; }

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
        #endregion

        private void Start()
        {
            characterController = GetComponent<CharacterController>();

            InputManager.Instance.PossessPlayer(this);
        }

        public void PossessTick()
        {
            isGrounded = characterController.isGrounded;
            //isGrounded = Physics.CheckSphere(groundCheck.position, 0.1f, groundMask);

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f; // Reset the vertical velocity when grounded
            }

            IInteractable interactable = GetSelectedObject<IInteractable>();
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
            bool operating = PlayerControllerState == EPlayerControllerState.Operating;

            if (InputController.CheckAndNullifyInput(EInputType.SButton))
            {
                OnActionInput();
            }

            //scroll
            if (operating)
            {
                if (InputController.CheckAndNullifyInput(EInputType.LBumper))
                {
                    OperatingScroll(false);
                }
                else if (InputController.CheckAndNullifyInput(EInputType.RBumper))
                {
                    OperatingScroll(true);
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
                isSprinting = true;
            }
            else
            {
                isSprinting = false;
            }

            // Calculate movement direction
            Vector3 move = transform.right * moveX + transform.forward * moveZ;
            float speed = isSprinting ? sprintSpeed : moveSpeed;
            characterController.Move(move.normalized * speed * Time.deltaTime);
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

            if (CameraManager.Instance.IsCameraInTransition())
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

            if (isGrounded && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                velocity.y += Mathf.Sqrt(jumpForce * -2f * gravity); // Calculate jump force
            }
        }

        private void ApplyGravity()
        {
            velocity.y += gravity * Time.deltaTime; // Apply gravity to the velocity
            characterController.Move(velocity * Time.deltaTime); // Move the character with gravity
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            _moveVector = context.ReadValue<Vector2>();
        }
        public void OnLook(InputAction.CallbackContext context)
        {
            _lookVector = context.ReadValue<Vector2>();
        }

        #region Operating
        public void Operating_OnBack(InputAction.CallbackContext callbackContext)
        {
            Debug.Log("Attempt leave body");

            bool operating = PlayerControllerState == EPlayerControllerState.Operating;

            if (operating)
            {
                if (CameraManager.Instance.ActivateVirtualCamera(EVirtualCameraType.FirstPersonView_Normal))
                {
                    RequestPlayerControllerState(EPlayerControllerState.Normal);

                    BodyMorgueActor storedBody = _operatingTable.GetStorable<BodyMorgueActor>();
                    if (storedBody != null)
                    {
                        storedBody.ToggleCollision(true);
                    }

                    if (_equippedOperatingTool != null)
                    {
                        ReturnOperatingToolToSlot(_equippedOperatingTool);
                    }
                    _operatingTable = null;
                }
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
            if (CameraManager.Instance.IsCameraInTransition())
            {
                return;
            }

            if (_operatingTable == null)
            {
                return;
            }

            int toolsCount = _operatingTable.OperatingToolsCount;
            int toolIndex = _operatingTable.GetOperatingToolIndex(_equippedOperatingTool);

            int newIndex = _equippedOperatingTool == null ? (forward ? 0 : toolsCount - 1) : toolIndex + (forward ? -1 : 1);

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
                        _equippedOperatingTool = newTool;
                    }
                }
            }

            //Debug.Log("Index is now : " + newIndex);
        }

        public bool ReturnOperatingToolToSlot(MorgueToolActor opTool)
        {
            if (opTool == null)
            {
                return false;
            }

            int oldToolIndex = _operatingTable.GetOperatingToolIndex(opTool);
            FStorageSlot opTableToolSlot = _operatingTable.GetOperatingToolStorageSlot(oldToolIndex);
            bool storedOldTool = opTableToolSlot.TryStore(opTool);

            Debug.Log("Stored old tool =  " + (storedOldTool ? "YES" : "NO"));
            if (storedOldTool)
            {
                _equippedOperatingTool = null;
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

        public T GetSelectedObject<T>() where T : class
        {
            T selected = default;

            GameObject selectedGO = InputController.SelectedObject;
            T selectedType = InputController.Selectable as T;

            if (selectedGO != null)
            {
                selected = selectedGO.GetComponent<T>();
                if (selected != null)
                {
                    return selected;
                }
            }
            else if (selectedType != null)
            {
                selected = selectedType as T;
                if (selected != null)
                {
                    return selected;
                }
            }

            return selected;
        }

        public T GetSelectedObjectParent<T>() where T : class
        {
            T selected = default;

            GameObject selectedGO = InputController.SelectedObject;

            if (selectedGO != null)
            {
                selected = selectedGO.GetComponentInParent<T>();
                if (selected != null)
                {
                    return selected;
                }
            }

            return selected;
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
            Debug.Log("Action input");
            bool operating = PlayerControllerState == EPlayerControllerState.Operating;

            if (operating)
            {
                BodyPartMorgueActor bodyPart = GetSelectedObject<BodyPartMorgueActor>();
                if (bodyPart == null)
                {
                    bodyPart = GetSelectedObjectParent<BodyPartMorgueActor>();
                }

                if (bodyPart != null)
                {
                    Debug.Log("Found body part = " + bodyPart.gameObject.name);

                    OperationDismemberMorgueTool dismemberTool = _equippedOperatingTool as OperationDismemberMorgueTool;
                    if (dismemberTool != null)
                    {
                        if (bodyPart.IsConnected())
                        {
                            IConnectable disconnectedPart = bodyPart.TryDisconnect(null);
                            
                            if (disconnectedPart != null)
                            {
                                IStorage nextPlayerStorage = _playerStorage.GetNextBestStorage(true, EPlayerControllerState.Normal);
                                if (nextPlayerStorage != null)
                                {
                                    IStorable prevStored = nextPlayerStorage.TryRemove(null);
                                    if (prevStored != null)
                                    {
                                        //MorgueToolActor oldTool = prevStored.GetStorableParent() as MorgueToolActor;
                                        //if (oldTool != null)
                                        //{
                                        //    ReturnOperatingToolToSlot(oldTool);
                                        //}
                                    }

                                    bool stored = nextPlayerStorage.TryStore(bodyPart);
                                    if (stored)
                                    {
                                        Debug.Log("Stored disconnected part successfully");
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    GameObject selectedObject = InputController.SelectedObject;

                    if (selectedObject != null)
                    {
                        if (selectedObject.layer == LayerMask.NameToLayer("MorgueCollision"))
                        {
                            BodyMorgueActor bodyMorgueActor = selectedObject.GetComponentInParent<BodyMorgueActor>();

                            if (bodyMorgueActor != null)
                            {
                                IStorage hands = PlayerStorage.GetPlayerHands();
                                BodyPartMorgueActor heldBodyPart = hands.GetStorable<BodyPartMorgueActor>();
                                if (heldBodyPart != null && _equippedOperatingTool as OperationAttachmentMorgueTool)
                                {
                                    if (selectedObject.tag == heldBodyPart.gameObject.tag)
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

            }
            else
            {
                IInteractable interactable = GetSelectedObject<IInteractable>();

                if (interactable != null)
                {
                    if (interactable.IsInteractable())
                    {
                        interactable.OnInteract();
                    }
                }
            }
        }

        public void BeginOperatingState(OperatingTable opTable)
        {
            _operatingTable = opTable;

            RequestPlayerControllerState(EPlayerControllerState.Operating);

            BodyMorgueActor storedBody = _operatingTable.GetStorable<BodyMorgueActor>();
            if (storedBody != null)
            {
                storedBody.ToggleCollision(false);
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
                        break;

                    case EPlayerControllerState.Operating:
                        cursorLock = CursorLockMode.Confined;
                        //mpi.Game.Action.RemoveAllBindingOverrides();
                        mpi.Game.Back.started += Operating_OnBack;

                        mpi.Game.Operating_Scroll.Enable();
                        //mpi.Game.Operating_Scroll.started += Operating_OnScroll;
                        if (gameIC)
                        {
                            mpi.Game.Operating_Scroll.started += gameIC.Operating_OnScroll;
                        }
                        //mpi.Game.Operating_Scroll.canceled += ctx => _opScroll = 0.0f;
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
                        break;

                    case EPlayerControllerState.Operating:
                        mpi.Game.Back.started -= Operating_OnBack;
                        
                        mpi.Game.Operating_Scroll.Disable();
                        //mpi.Game.Operating_Scroll.started -= Operating_OnScroll;
                        if (gameIC)
                        {
                            mpi.Game.Operating_Scroll.started -= gameIC.Operating_OnScroll;
                        }
                        break;

                    default:
                        break;
                }

            }

            _playerControllerState = EPlayerControllerState.NONE;
        }
    }
}

