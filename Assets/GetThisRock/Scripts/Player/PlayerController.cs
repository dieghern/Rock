using UnityEngine;

namespace GetThisRock
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerController : MonoBehaviour
    {
        public PlayerInput input;
        public Transform view;
        public PlayerInteraction interaction;
        [Min(0)] public float speed = 4.5f;
        [Min(0)] public float jumpHeight = 1.1f;
        public float gravity = -22f;
        public float mouseSensitivity = .09f;
        public float contactForce = 90f;
        public float maximumPushSpeed = .65f;
        CharacterController controller;
        float verticalSpeed, pitch;
        Vector3 movement;
        void Awake() => controller = GetComponent<CharacterController>();
        void Update()
        {
            transform.Rotate(0, input.Look.x * mouseSensitivity, 0);
            pitch = Mathf.Clamp(pitch - input.Look.y * mouseSensitivity, -80, 80);
            view.localRotation = Quaternion.Euler(pitch, 0, 0);
            if (controller.isGrounded && verticalSpeed < 0) verticalSpeed = -2;
            if (controller.isGrounded && input.JumpPressed) verticalSpeed = Mathf.Sqrt(jumpHeight * -2 * gravity);
            verticalSpeed += gravity * Time.deltaTime;
            movement = transform.TransformDirection(new Vector3(input.Move.x, 0, input.Move.y));
            controller.Move((movement * (interaction != null && interaction.equipment != null && interaction.equipment.stats != null ? interaction.equipment.stats.MoveSpeed : speed) * (interaction != null ? interaction.MovementMultiplier : 1f) + Vector3.up * verticalSpeed) * Time.deltaTime);
        }
        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (!input.Captured || (interaction != null && interaction.Effort > .01f) || hit.moveDirection.y < -.5f || movement.sqrMagnitude < .01f) return;
            var body = hit.collider.GetComponentInParent<PhysicalObject>();
            if (body != null) body.Push(movement, hit.point, contactForce, maximumPushSpeed, Time.deltaTime);
        }
    }
}
