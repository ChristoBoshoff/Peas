using UnityEngine;

namespace Riftwake
{
    /// <summary>
    /// Keyboard movement + mouse facing on the ground plane (classic top-down action feel).
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        [SerializeField] float moveSpeed = 6.5f;
        [SerializeField] float gravity = -20f;

        CharacterController _controller;
        Camera _cam;
        float _verticalVelocity;
        Plane _groundPlane = new Plane(Vector3.up, Vector3.zero);

        public Vector3 AimDirection { get; private set; } = Vector3.forward;
        public bool IsMoving { get; private set; }
        public bool InputEnabled { get; set; } = true;

        public void Configure(float speed, Camera cam)
        {
            moveSpeed = speed;
            _cam = cam;
        }

        void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }

        void Update()
        {
            if (!InputEnabled)
            {
                IsMoving = false;
                ApplyGravityOnly();
                return;
            }

            if (_cam == null)
                _cam = Camera.main;

            UpdateFacing();
            UpdateMovement();
        }

        void ApplyGravityOnly()
        {
            if (_controller.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f;
            else
                _verticalVelocity += gravity * Time.deltaTime;
            _controller.Move(new Vector3(0f, _verticalVelocity, 0f) * Time.deltaTime);
        }

        void UpdateFacing()
        {
            if (_cam == null)
                return;

            var ray = _cam.ScreenPointToRay(Input.mousePosition);
            if (_groundPlane.Raycast(ray, out float enter))
            {
                var hit = ray.GetPoint(enter);
                var flat = hit - transform.position;
                flat.y = 0f;
                if (flat.sqrMagnitude > 0.01f)
                {
                    AimDirection = flat.normalized;
                    transform.rotation = Quaternion.LookRotation(AimDirection, Vector3.up);
                }
            }
        }

        void UpdateMovement()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            var input = new Vector3(h, 0f, v);
            if (input.sqrMagnitude > 1f)
                input.Normalize();

            IsMoving = input.sqrMagnitude > 0.01f;

            if (_controller.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f;
            else
                _verticalVelocity += gravity * Time.deltaTime;

            var motion = input * moveSpeed;
            motion.y = _verticalVelocity;
            _controller.Move(motion * Time.deltaTime);
        }
    }
}
