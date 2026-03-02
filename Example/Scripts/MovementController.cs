using UnityEngine;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    MovementController.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.Example
{
    internal enum MovementState
    {
        Grounded,
        Airborne
    }

    [RequireComponent(typeof(CharacterController))]
    internal class MovementController : MonoBehaviour
    {
        private readonly float GRAVITY = -9.8f;
        private readonly float GROUNDED_VEL = -2f;

        [SerializeField] private Settings _settings;

        private MovementState _state;

        private Vector3 _velocity;
        private Vector2 _horizontalVelocity;
        private Vector2 _movement;

        private CharacterController _controller;
        private InputManager _input;

        public static bool LockMovement { get; set; }
        private float Speed => _settings.Speed;
        private float Gravity => GRAVITY * _settings.GravityMul;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _input = FindAnyObjectByType<InputManager>();

            Unfreeze();
        }

        private void Update()
        {
            if (MovementController.LockMovement) return;
            
            _movement = Vector2.ClampMagnitude(_input.RawMovement, 1f);
            ApplyGravity();
            UpdateMovement();
            MoveController();
        }
        private void ApplyGravity()
        {
            _velocity.y = _state == MovementState.Airborne
                ? Mathf.MoveTowards(_velocity.y, -_settings.TerminalVelocity, -Gravity * Time.deltaTime)
                : GROUNDED_VEL;
        }

        private void UpdateHorizontalVelocity(float controlMultiplier)
        {
            Vector3 local = new Vector3(_movement.x, 0f, _movement.y);
            float multiplier = 1f;
            if (_movement.y < 0f) multiplier *= _settings.BackwardSpeedMul;
            if (_movement.x != 0f) multiplier *= _settings.StrafingSpeedMul;
            Vector3 target3D = transform.TransformDirection(local) * Speed * multiplier;
            Vector2 target2D = new Vector2(target3D.x, target3D.z) * controlMultiplier;

            _horizontalVelocity = Vector2.Lerp(
                _horizontalVelocity,
                target2D,
                _settings.InputSmoothing * Time.deltaTime
            );

            _velocity.x = _horizontalVelocity.x;
            _velocity.z = _horizontalVelocity.y;
        }

        private void UpdateGrounded()
        {
            UpdateHorizontalVelocity(1f);
            if (!_controller.isGrounded) _state = MovementState.Airborne;
        }

        private void UpdateAirborne()
        {
            UpdateHorizontalVelocity(_settings.AirControl);
            if (_controller.isGrounded)
            {
                _state = MovementState.Grounded;
                _velocity.y = GROUNDED_VEL;
            }
        }

        private void MoveController()
        {
            _controller.Move(_velocity * Time.deltaTime);
        }

        private void UpdateMovement()
        {
            switch (_state)
            {
                case MovementState.Grounded: UpdateGrounded(); break;
                case MovementState.Airborne: UpdateAirborne(); break;
            }
        }

        public void Freeze()
        {
            MovementController.LockMovement = true;
            _input.DisableMovement();
            _velocity = Vector2.zero;
            _horizontalVelocity = Vector2.zero;
        }

        public void Unfreeze()
        {
            MovementController.LockMovement = false;
            _input.EnableMovement();
        }
    }
}
