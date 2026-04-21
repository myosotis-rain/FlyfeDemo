using UnityEngine;
using System.Collections;
using Flyfe.Core;

namespace Flyfe.Gameplay
{
    /// <summary>
    /// An elite-grade feedback component for platforms.
    /// Uses Animation Curves for professional "game feel" and preserves physics stability.
    /// </summary>
    [SelectionBase] 
    public class PlatformFeedback : MonoBehaviour, IResettable
    {
        [Header("Visuals")]
        [SerializeField] private Color landedColor = Color.cyan;
        [SerializeField] private AnimationCurve pulseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private float pulseIntensity = 0.05f; // Extra scale added during pulse
        [SerializeField] private float pulseDuration = 0.2f;

        [Header("Detection Settings")]
        [Tooltip("How much higher than the platform's center the footer must be to trigger.")]
        [SerializeField] private float heightThreshold = -0.1f;
        [SerializeField] private float exitBuffer = 0.15f;
        [SerializeField] private bool triggerByShadow = true;
        
        [Header("References")]
        [Tooltip("Explicitly assign the SpriteRenderer that should change color.")]
        [SerializeField] private SpriteRenderer targetRenderer;
        [Tooltip("The visual child that will pulse. DO NOT use the object with the Collider.")]
        [SerializeField] private Transform visualRoot;

        // Internal State
        private Color _originalColor;
        private Vector3 _originalScale;
        private bool _isLanded;
        private Coroutine _feedbackCoroutine;
        private float _exitCooldown;

        private void Awake()
        {
            // Search Logic: 1. Manual Assignment -> 2. Visual Root -> 3. Children
            if (targetRenderer == null && visualRoot != null) targetRenderer = visualRoot.GetComponent<SpriteRenderer>();
            if (targetRenderer == null) targetRenderer = GetComponentInChildren<SpriteRenderer>();
            
            // Auto-assign visualRoot if missing
            if (visualRoot == null) visualRoot = (targetRenderer != null) ? targetRenderer.transform : transform;
            
            if (targetRenderer != null) 
            {
                _originalColor = targetRenderer.color;
                Debug.Log($"[PlatformFeedback] Found Renderer: {targetRenderer.name}, Original Color: {_originalColor}");
            }
            else
            {
                Debug.LogError("[PlatformFeedback] No SpriteRenderer found on " + name);
            }

            _originalScale = visualRoot.localScale;
        }

        private void OnCollisionEnter2D(Collision2D collision) 
        {
            bool isTopCollision = false;
            
            // Iterate through all contacts to find any that indicate a "top surface" hit
            // A surface facing UP has a normal pointing DOWN from the actor's perspective (negative Y)
            for (int i = 0; i < collision.contactCount; i++)
            {
                if (collision.GetContact(i).normal.y < -0.4f) // ~66 degree slope tolerance
                {
                    isTopCollision = true;
                    break;
                }
            }
            
            HandleEntry(collision.gameObject, isTopCollision);
        }

        private void OnTriggerEnter2D(Collider2D other) => HandleEntry(other.gameObject, false);

        private void HandleEntry(GameObject other, bool isTopCollision)
        {
            bool isPlayer = other.CompareTag(Tags.Player);
            bool isShadow = triggerByShadow && other.CompareTag(Tags.Shadow);
            
            if (!isPlayer && !isShadow) return;

            // Use a slightly more generous cooldown for stability on irregular surfaces
            if (_isLanded || Time.time < _exitCooldown) return;

            if (isTopCollision)
            {
                TriggerFeedback();
                return;
            }

            // Fallback Height Check: Essential for Triggers or ambiguous physics contacts
            Collider2D otherCollider = other.GetComponent<Collider2D>();
            Collider2D platformCollider = GetComponent<Collider2D>();

            if (otherCollider != null && platformCollider != null)
            {
                float actorFeet = otherCollider.bounds.min.y;
                
                // For irregular polygons/slopes, comparing to the CENTER Y is much more stable 
                // than comparing to the MAX Y (the peak), which would fail at the bottom of a slope.
                float platformReferenceHeight = platformCollider.bounds.center.y;

                if (actorFeet > platformReferenceHeight + heightThreshold)
                {
                    TriggerFeedback();
                }
            }
        }

        private void TriggerFeedback()
        {
            _isLanded = true;
            StartFeedback();
        }

        private void OnCollisionExit2D(Collision2D collision) => HandleExit(collision.gameObject);
        private void OnTriggerExit2D(Collider2D other) => HandleExit(other.gameObject);

        private void HandleExit(GameObject other)
        {
            if (other.CompareTag(Tags.Player) || (triggerByShadow && other.CompareTag(Tags.Shadow)))
            {
                Debug.Log("[PlatformFeedback] Exited platform");
                _isLanded = false;
                _exitCooldown = Time.time + exitBuffer;
                
                // Return color on exit
                if (targetRenderer != null) targetRenderer.color = _originalColor;
            }
        }

        private void StartFeedback()
        {
            if (targetRenderer != null) 
            {
                Debug.Log($"[PlatformFeedback] Applying color: {landedColor}");
                targetRenderer.color = landedColor;
            }

            if (_feedbackCoroutine != null) StopCoroutine(_feedbackCoroutine);
            _feedbackCoroutine = StartCoroutine(PulseRoutine());
        }

        private IEnumerator PulseRoutine()
        {
            float elapsed = 0f;

            while (elapsed < pulseDuration)
            {
                elapsed += Time.deltaTime;
                float normalizedTime = elapsed / pulseDuration;
                
                // Evaluate the curve to get the scale multiplier
                float curveValue = pulseCurve.Evaluate(normalizedTime);
                float currentScaleOffset = curveValue * pulseIntensity;
                
                visualRoot.localScale = _originalScale + (Vector3.one * currentScaleOffset);
                
                yield return null;
            }
            
            visualRoot.localScale = _originalScale;
            _feedbackCoroutine = null;
        }

        public void ResetState()
        {
            _isLanded = false;
            _exitCooldown = 0;
            
            if (_feedbackCoroutine != null)
            {
                StopCoroutine(_feedbackCoroutine);
                _feedbackCoroutine = null;
            }

            if (visualRoot != null) visualRoot.localScale = _originalScale;
            if (targetRenderer != null) targetRenderer.color = _originalColor;
        }
    }
}
