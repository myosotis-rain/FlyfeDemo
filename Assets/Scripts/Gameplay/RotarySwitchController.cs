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

        [Header("Animation Settings")]
        [SerializeField] private float targetAngle = -90f;
        [SerializeField] private float smoothSpeed = 10f;

        [Header("Events & Effects")]
        public UnityEvent onSwitchOn;
        public UnityEvent onSwitchOff;

        private bool _isOn = false;
        private Quaternion _startRot;
        private Quaternion _endRot;
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
        }

        private void SetState(bool on)
        {
            Debug.Log($"[RotarySwitch] Attempting to set state to: {on}. Previous: {_isOn}");
            if (_isOn == on) return; // Prevent redundant firing
            
            _isOn = on;

            if (_isOn) onSwitchOn.Invoke();
            else onSwitchOff.Invoke();
            
            Debug.Log($"<color=cyan>[RotarySwitch]</color> {name} confirmed transition to {(_isOn ? "ON" : "OFF")}");
        }
    }
}
