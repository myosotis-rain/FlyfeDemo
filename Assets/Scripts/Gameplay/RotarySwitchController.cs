using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Flyfe.Gameplay
{
    public class RotarySwitchController : MonoBehaviour, IInteractable, IResettable
    {
        [Header("Hierarchy References")]
        [SerializeField] private Transform pivotTransform; 
        [SerializeField] private Transform backplate;

        [Header("Auto-Off Settings")]
        [SerializeField] private bool autoOff = true;
        [SerializeField] private float interactRange = 3.5f;
        [SerializeField] private float bufferTime = 1.0f; 

        [Header("Animation Settings")]
        [SerializeField] private float targetAngle = -90f;
        [SerializeField] private float smoothSpeed = 10f;

        [Header("Events & Effects")]
        public UnityEvent onSwitchOn;
        public UnityEvent onSwitchOff;

        private bool _isOn = false;
        private Quaternion _startRot;
        private Quaternion _endRot;
        private float _outOfRangeTimer = 0f;
        private bool _initialState;

        void Awake()
        {
            _initialState = _isOn;
            if (pivotTransform != null)
            {
                _startRot = pivotTransform.localRotation;
                _endRot = Quaternion.Euler(0, 0, targetAngle);
            }
        }

        public void ResetState()
        {
            _isOn = _initialState;
            _outOfRangeTimer = 0f;
            
            if (pivotTransform != null)
            {
                pivotTransform.localRotation = _isOn ? _endRot : _startRot;
            }
            
            if (_isOn) onSwitchOn.Invoke();
            else onSwitchOff.Invoke();
        }

        public void Interact(GameObject actor) => Toggle();

        public string GetInteractPrompt() => _isOn ? "Close" : "Open";

        void Update()
        {
            if (pivotTransform != null)
            {
                Quaternion targetRot = _isOn ? _endRot : _startRot;
                pivotTransform.localRotation = Quaternion.Slerp(pivotTransform.localRotation, targetRot, Time.deltaTime * smoothSpeed);
            }

            if (_isOn && autoOff)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    float dist = Vector2.Distance(player.transform.position, transform.position);
                    if (dist > interactRange)
                    {
                        _outOfRangeTimer += Time.deltaTime;
                        if (_outOfRangeTimer >= bufferTime) Toggle();
                    }
                    else
                    {
                        _outOfRangeTimer = 0f;
                    }
                }
            }
        }

        public void Toggle()
        {
            _isOn = !_isOn;
            _outOfRangeTimer = 0f;

            if (_isOn) onSwitchOn.Invoke();
            else onSwitchOff.Invoke();
        }
    }
}
