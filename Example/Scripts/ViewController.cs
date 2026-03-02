using UnityEngine;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    ViewController.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.Example
{
    internal class ViewController : MonoBehaviour
    {
        [SerializeField] private Settings _settings;
        [SerializeField] private Transform _playerBody;

        private Vector3 _startPos;
        private float _pitch = 0f;
        private float _yaw = 0f;

        private InputManager _input;

        public float Sensitivity => _settings.Sensitivity;
        public bool IsHeadBobbingEnabled => _settings.EnableHeadMotion;

        private void Awake()
        {
            _startPos = transform.localPosition;
            if (!_playerBody) _playerBody = transform.parent;
            _input = FindAnyObjectByType<InputManager>();
        }

        private void Update()
        {
            UpdateRotation();
        }

        private void StartHeadBob()
        {
            Vector3 pos = Vector3.zero;
            pos.x += Mathf.Lerp(pos.x, Mathf.Sin(Time.time * _settings.HeadBobFreqency) * _settings.HeadBobAmount * 1.4f, _settings.HeadBobSmoothing * Time.deltaTime);
            pos.y += Mathf.Lerp(pos.y, Mathf.Sin(Time.time * _settings.HeadBobFreqency) * _settings.HeadBobAmount * 1.4f, _settings.HeadBobSmoothing * Time.deltaTime);
            transform.localPosition += pos;
        }

        private void CheckForHeadMovement()
        {
            float movementAmount = new Vector3(_input.RawMovement.x, 0f, _input.RawMovement.y).magnitude;
            if (movementAmount > 0f)
            {
                StartHeadBob();
            }
        }

        private void StopHeadMovement()
        {
            if (transform.localPosition == _startPos) return;
            transform.localPosition = Vector3.Lerp(transform.localPosition, _startPos, Time.deltaTime);
        }

        private void ProcessHead()
        {
            _pitch = transform.localEulerAngles.AngleAs180().x;
            _pitch -= (_settings.InvertLookY ? -1 : 1) * Sensitivity * _input.RawLook.y;
            _pitch = Mathf.Clamp(_pitch, _settings.YLookLimit.x, _settings.YLookLimit.y);
            transform.localEulerAngles = transform.localEulerAngles.SetX(_pitch);
        }

        private void ProcessBody()
        {
            _yaw = _playerBody.transform.localEulerAngles.y;
            _yaw += (_settings.InvertLookX ? -1 : 1) * Sensitivity * _input.RawLook.x;
            _playerBody.transform.localEulerAngles = _playerBody.transform.localEulerAngles.SetY(_yaw);
        }

        private void UpdateRotation()
        {
            if (MovementController.LockMovement) return;

            ProcessHead();
            ProcessBody();

            if (IsHeadBobbingEnabled)
            {
                CheckForHeadMovement();
                StopHeadMovement();
            }
        }
    }
}
