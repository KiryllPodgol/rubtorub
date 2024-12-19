using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Script
{
    public class ThirdPersonController : MonoBehaviour
    {
        private ThirdPersonActionsAsset playerActionsAsset;
        private InputAction move;
        private Rigidbody rb;
        private Animator animator;
        [SerializeField] private float movementForce = 1f;
        [SerializeField] private float jumpForce = 5f; 
        [SerializeField] private float maxSpeed = 5f;
        private Vector3 forceDirection = Vector3.zero;
        [SerializeField] private Camera playerCamera;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            playerActionsAsset = new ThirdPersonActionsAsset();
        }

        private void OnEnable()
        {
            playerActionsAsset.Player.Jump.started += JumpOnstarted;
            move = playerActionsAsset.Player.Move;
            playerActionsAsset.Player.Enable();
        }

        private void OnDisable()
        {
            playerActionsAsset.Player.Jump.started -= JumpOnstarted;
            playerActionsAsset.Player.Disable();
        }

        private void FixedUpdate()
        {
            // Управление движением
            forceDirection += move.ReadValue<Vector2>().x * GetCameraRight(playerCamera) * movementForce;
            forceDirection += move.ReadValue<Vector2>().y * GetCameraForward(playerCamera) * movementForce;

            // Применение силы к Rigidbody
            rb.AddForce(forceDirection, ForceMode.Impulse);
            forceDirection = Vector3.zero;

            // Применение гравитации
            if (rb.linearVelocity.y < 0f)
                rb.linearVelocity += Vector3.down  * Time.fixedDeltaTime;

            // Ограничение максимальной скорости
            Vector3 horizontalVelocity = rb.linearVelocity;
            horizontalVelocity.y = 0; 

            if (horizontalVelocity.sqrMagnitude > maxSpeed * maxSpeed)
                rb.linearVelocity = horizontalVelocity.normalized * maxSpeed + Vector3.up * rb.linearVelocity.y;

            // Обновление анимации
            UpdateAnimation();

            // Поворот персонажа в направлении движения
            LookAt();
        }

        private void UpdateAnimation()
        {
            animator.SetFloat("speed", rb.linearVelocity.magnitude / maxSpeed);

            if (IsGrounded())
            {
                animator.SetBool("isGrounded", true);
                animator.SetBool("jump", false);
            }
            else
            {
                animator.SetBool("isGrounded", false);
                animator.SetBool("jump", true);
            }
        }


        private void LookAt()
        {
            Vector3 direction = rb.linearVelocity;
            direction.y = 0f; // Игнорируем вертикальную составляющую

            if (move.ReadValue<Vector2>().sqrMagnitude > 0.1f && direction.sqrMagnitude > 0.1f)
                rb.rotation = Quaternion.LookRotation(direction, Vector3.up);
            else
                rb.angularVelocity = Vector3.zero; // Отключаем вращение, если персонаж не движется
        }

        private Vector3 GetCameraForward(Camera camera)
        {
            Vector3 forward = camera.transform.forward;
            forward.y = 0; // Игнорируем вертикальную составляющую
            return forward.normalized;
        }

        private Vector3 GetCameraRight(Camera camera)
        {
            Vector3 right = camera.transform.right;
            right.y = 0; 
            return right.normalized;
        }

        private void JumpOnstarted(InputAction.CallbackContext obj)
        {
            if (IsGrounded())
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                animator.SetBool("jump", true); // Включение анимации прыжка
                animator.SetBool("isGrounded", false);
            }
        }
        public bool IsGrounded()
        {
            Ray ray = new Ray(transform.position + Vector3.up * 0.25f, Vector3.down);
            return Physics.Raycast(ray, out RaycastHit hit, 0.5f); 
        }
    }
}
