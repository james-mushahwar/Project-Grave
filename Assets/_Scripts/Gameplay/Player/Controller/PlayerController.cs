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

namespace _Scripts.Gameplay.Player.Controller{
    
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

        [Header("Mouse Look Settings")]
        [SerializeField] private float mouseSensitivity = 100f;
        private float xRotation = 0f;

        public InputController InputController { get; private set; }

        private void Start()
        {
            characterController = GetComponent<CharacterController>();
            Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the center of the screen

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

            HandleRotation();
            HandleMovement();
            HandleJump();
            ApplyGravity();
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

            Vector2 mouseInput = _lookVector;
            float mouseX = mouseInput.x * mouseSensitivity * Time.deltaTime;
            float mouseY = mouseInput.y * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Clamp the vertical rotation
            transform.Rotate(Vector3.up * mouseX); // Rotate the player
            Camera.main.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f); // Rotate the camera
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
                MasterPlayerInput mpi = InputManager.Instance.MasterPlayerInput;
                if (mpi != null)
                {
                    #region Game
                    mpi.Game.Movement.performed += OnMove;
                    mpi.Game.Movement.canceled += OnMove;

                    mpi.Game.Look.performed += OnLook;
                    mpi.Game.Look.canceled  += OnLook;
                    #endregion
                }
            }
        }

        protected void Unpossess(InputController controller)
        {
            if (InputManager.Instance != null)
            {
                MasterPlayerInput mpi = InputManager.Instance.MasterPlayerInput;
                if (mpi != null)
                {
                    #region Game
                    mpi.Game.Movement.performed -= OnMove;
                    mpi.Game.Movement.canceled  -= OnMove;

                    mpi.Game.Look.performed -= OnLook;
                    mpi.Game.Look.canceled  -= OnLook;

                    #endregion
                }
            }

            InputController = null;
        }
    }
}

