using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Scripts.Gameplay.Player.Controller{
    
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float sprintSpeed = 10f;
        [SerializeField] private float jumpForce = 5f;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private LayerMask groundMask;

        private CharacterController characterController;
        private Vector3 velocity;
        private bool isGrounded;
        private bool isSprinting;

        [Header("Mouse Look Settings")]
        [SerializeField] private float mouseSensitivity = 100f;
        private float xRotation = 0f;

        private void Start()
        {
            characterController = GetComponent<CharacterController>();
            Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the center of the screen
        }

        private void Update()
        {
            isGrounded = characterController.isGrounded;
            //isGrounded = Physics.CheckSphere(groundCheck.position, 0.1f, groundMask);

            Debug.Log("Is grounded = " + isGrounded);

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f; // Reset the vertical velocity when grounded
            }

            HandleMovement();
            HandleJump();
            ApplyGravity();
        }

        private void HandleMovement()
        {
            float moveX = 0f; // Horizontal movement
            float moveZ = 0f; // Forward movement

            // Handle inputs
            if (Keyboard.current.wKey.isPressed) moveZ = 1f;
            if (Keyboard.current.sKey.isPressed) moveZ = -1f;
            if (Keyboard.current.aKey.isPressed) moveX = -1f;
            if (Keyboard.current.dKey.isPressed) moveX = 1f;

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

        public void OnLook(InputAction.CallbackContext context)
        {
            Vector2 mouseInput = context.ReadValue<Vector2>();
            float mouseX = mouseInput.x * mouseSensitivity * Time.deltaTime;
            float mouseY = mouseInput.y * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Clamp the vertical rotation
            Camera.main.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f); // Rotate the camera
            transform.Rotate(Vector3.up * mouseX); // Rotate the player
        }
    }
}

