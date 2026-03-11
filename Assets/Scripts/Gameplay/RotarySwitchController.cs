using UnityEngine;
using UnityEngine.Events;
using Flyfe.Core;

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
                
                // Ignore Player/Shadow collisions to prevent glitches
                Collider2D[] childColliders = pivotTransform.GetComponentsInChildren<Collider2D>();
                foreach (var col in childColliders)
                {
                    GameObject player = GameObject.FindGameObjectWithTag(Tags.Player);
                    if (player != null && player.TryGetComponent<Collider2D>(out var pCol))
                        Physics2D.IgnoreCollision(col, pCol, true);
                }
            }
        }

        public void ResetState()
        {
            _isOn = _initialState;
            _outOfRangeTimer = 0f;
            if (pivotTransform != null) pivotTransform.localRotation = _isOn ? _endRot : _startRot;
            // No events on reset to prevent recursive loops
        }

        public void Interact(GameObject actor)
        {
            Debug.Log($"[RotarySwitch] {name} Interacted with by {actor.name}. Current State: {_isOn}");
            if (!_isOn) SetState(true);
            else SetState(false);
        }

        public string GetInteractPrompt() => _isOn ? "Close" : "Open";

        void Update()
        {
            // Smooth Visual Rotation
            if (pivotTransform != null)
            {
                Quaternion targetRot = _isOn ? _endRot : _startRot;
                pivotTransform.localRotation = Quaternion.Slerp(pivotTransform.localRotation, targetRot, Time.deltaTime * smoothSpeed);
            }
            else
            {
                Debug.LogWarning($"[RotarySwitch] {name} has NO pivotTransform assigned!");
            }

            // AUTO-OFF LOGIC
            if (_isOn && autoOff)
            {
                bool isNear = false;
                GameObject player = GameObject.FindGameObjectWithTag(Tags.Player);
                if (player != null && Vector2.Distance(player.transform.position, transform.position) <= interactRange)
                {
                    isNear = true;
                }

                // Shadows can also keep it open
                if (!isNear)
                {
                    GameObject[] shadows = GameObject.FindGameObjectsWithTag(Tags.Shadow);
                    foreach (var s in shadows)
                    {
                        if (Vector2.Distance(s.transform.position, transform.position) <= interactRange)
                        {
                            isNear = true;
                            break;
                        }
                    }
                }

                if (isNear)
                {
                    _outOfRangeTimer = 0f; // Reset timer while someone is here
                }
                else
                {
                    _outOfRangeTimer += Time.deltaTime;
                    if (_outOfRangeTimer >= bufferTime)
                    {
                        Debug.Log($"[RotarySwitch] {name} auto-closing due to range.");
                        SetState(false);
                    }
                }
            }
        }

        private void SetState(bool on)
        {
            Debug.Log($"[RotarySwitch] Attempting to set state to: {on}. Previous: {_isOn}");
            if (_isOn == on) return; // Prevent redundant firing
            
            _isOn = on;
            _outOfRangeTimer = 0f;

            if (_isOn) onSwitchOn.Invoke();
            else onSwitchOff.Invoke();
            
            Debug.Log($"<color=cyan>[RotarySwitch]</color> {name} confirmed transition to {(_isOn ? "ON" : "OFF")}");
        }
    }
}
