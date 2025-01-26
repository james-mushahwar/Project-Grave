using System;
using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.Input.InputController;
using _Scripts.Gameplay.Input.InputController.Game;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using _Scripts.Editortools.Draw;
using UnityEngine.InputSystem;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using _Scripts.Org;

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
        }

        public void PossessLateTick()
        {
            
        }

        public void PossessFixedTick()
        {
            if (InputController.CheckAndNullifyInput(EInputType.SButton))
            {
                OnActionInput();
            }

            if (PlayerControllerState == EPlayerControllerState.Normal)
            {
                HandleRotation();
                HandleMovement();
                HandleJump();
                ApplyGravity();
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

            // Handle inputs
            //if (Keyboard.current.wKey.isPressed) moveZ = 1f;
            //if (Keyboard.current.sKey.isPressed) moveZ = -1f;
            //if (Keyboard.current.aKey.isPressed) moveX = -1f;
            //if (Keyboard.current.dKey.isPressed) moveX = 1f;

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
                }
            }
            else
            {
                Debug.Log("Leaving Operating on body");
            }
            
        }

        public void Operating_OnAction(InputAction.CallbackContext callbackContext)
        {
            
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

        public void OnActionInput()
        {
            IInteractable interactable = null;

            Debug.Log("Ation input");

            GameObject selectedGO = InputController.SelectedObject;
            ISelect selectable = InputController.Selectable;

            if (selectedGO != null)
            {
                interactable = selectedGO.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    if (interactable.IsInteractable())
                    {
                        interactable.OnInteract();
                    }
                }
            }
            else if (selectable != null)
            {
                interactable = selectable as IInteractable;
                if (interactable != null)
                {
                    if (interactable.IsInteractable())
                    {
                        interactable.OnInteract();
                    }
                }
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
                        break;

                    default:
                        break;
                }

            }

            _playerControllerState = EPlayerControllerState.NONE;
        }
    }
}

