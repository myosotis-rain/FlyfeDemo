using UnityEngine;
using Flyfe.UI;
using Flyfe.Dialogue;
using System.Collections.Generic;

namespace Flyfe.Gameplay
{
    public class PendulumController : MonoBehaviour, IResettable
    {
        [Header("Pendulum Settings")]
        [Tooltip("The maximum angle (in degrees) the pendulum swings to either side.")]
        [SerializeField] private float maxAngle = 45f;
        [Tooltip("How fast the pendulum swings.")]
        [SerializeField] private float swingSpeed = 2f;
        [Tooltip("Whether the pendulum is swinging by default when the scene starts.")]
        [SerializeField] private bool startActive = false;
        
        [Header("Hierarchy (Optional)")]
        [Tooltip("If left empty, this object's transform will be used as the top center pivot.")]
        [SerializeField] private Transform pivotTransform;

        private bool _isActive;
        private float _time;
        private Quaternion _startRotation;
        private Rigidbody2D _rb;
        private HashSet<Rigidbody2D> _riders = new HashSet<Rigidbody2D>();

        private void Awake()
        {
            if (pivotTransform == null) pivotTransform = transform;
            
            _startRotation = pivotTransform.localRotation;
            
            // Try to find a Rigidbody2D. If found, we will use it for smoother physics interactions 
            // (e.g. if the player can grab onto it or stand on it).
            _rb = pivotTransform.GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            ResetState();
        }

        private void FixedUpdate()
        {
            // Pause movement during dialogue or cutscenes (consistent with other moving mechanics)
            if ((DialogueUI.Instance != null && DialogueUI.Instance.IsOpen) || CutsceneController.AnyCutsceneActive) return;

            if (_isActive)
            {
                float previousTime = _time;
                _time += Time.fixedDeltaTime * swingSpeed;
                
                // Calculate current angle based on a smooth sine wave
                float currentAngle = Mathf.Sin(_time) * maxAngle;
                float previousAngle = Mathf.Sin(previousTime) * maxAngle;
                float deltaAngle = currentAngle - previousAngle;
                
                Quaternion targetRotation = _startRotation * Quaternion.Euler(0, 0, currentAngle);

                if (_rb != null && _rb.bodyType == RigidbodyType2D.Kinematic)
                {
                    _rb.MoveRotation(targetRotation);
                }
                else
                {
                    pivotTransform.localRotation = targetRotation;
                }

                // Move any characters standing on the pendulum
                if (deltaAngle != 0f && _riders.Count > 0)
                {
                    Vector2 pivotPos = pivotTransform.position;
                    Quaternion rotationStep = Quaternion.Euler(0, 0, deltaAngle);

                    _riders.RemoveWhere(r => r == null); // Clean up destroyed objects

                    foreach (var rider in _riders)
                    {
                        Vector2 offset = rider.position - pivotPos;
                        Vector3 rotatedOffset = rotationStep * (Vector3)offset;
                        rider.position = pivotPos + (Vector2)rotatedOffset;
                    }
                }
            }
        }

        public void ResetState()
        {
            _isActive = startActive;
            _time = 0f; // Resets the pendulum back to the center 
            
            if (_rb != null && _rb.bodyType == RigidbodyType2D.Kinematic)
            {
                _rb.rotation = _startRotation.eulerAngles.z;
            }
            else
            {
                pivotTransform.localRotation = _startRotation;
            }
        }

        // Methods to be called by UnityEvents (like from the RotarySwitchController)
        public void Activate()
        {
            _isActive = true;
        }

        public void Deactivate()
        {
            _isActive = false;
        }

        public void Toggle()
        {
            _isActive = !_isActive;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Shadow"))
            {
                if (collision.rigidbody != null) _riders.Add(collision.rigidbody);
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Shadow"))
            {
                if (collision.rigidbody != null) _riders.Remove(collision.rigidbody);
            }
        }
    }
}
