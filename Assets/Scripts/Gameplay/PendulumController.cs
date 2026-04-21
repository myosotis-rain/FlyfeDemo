using UnityEngine;
using Flyfe.UI;
using Flyfe.Dialogue;
using Flyfe.Core;

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

        private void Awake()
        {
            if (pivotTransform == null) pivotTransform = transform;
            _startRotation = pivotTransform.localRotation;
            _rb = pivotTransform.GetComponent<Rigidbody2D>();

            // Professional Practice: Moving platforms MUST interpolate 
            // so that childed players don't jitter.
            if (_rb != null)
            {
                _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            }
        }

        private void Start()
        {
            ResetState();
        }

        private void FixedUpdate()
        {
            if ((DialogueUI.Instance != null && DialogueUI.Instance.IsOpen) || CutsceneController.AnyCutsceneActive) return;

            if (_isActive)
            {
                _time += Time.fixedDeltaTime * swingSpeed;
                float currentAngle = Mathf.Sin(_time) * maxAngle;
                Quaternion targetRotation = _startRotation * Quaternion.Euler(0, 0, currentAngle);

                if (_rb != null && _rb.bodyType == RigidbodyType2D.Kinematic)
                {
                    _rb.MoveRotation(targetRotation);
                }
                else
                {
                    pivotTransform.localRotation = targetRotation;
                }
            }
        }

        public void ResetState()
        {
            _isActive = startActive;
            _time = 0f;
            
            if (_rb != null && _rb.bodyType == RigidbodyType2D.Kinematic)
            {
                _rb.rotation = _startRotation.eulerAngles.z;
            }
            else
            {
                pivotTransform.localRotation = _startRotation;
            }
        }

        public void Activate() => _isActive = true;
        public void Deactivate() => _isActive = false;
        public void Toggle() => _isActive = !_isActive;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Parenting logic removed because PlayerController handles platform velocity inheritance.
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            // Parenting logic removed because PlayerController handles platform velocity inheritance.
        }
    }
}
