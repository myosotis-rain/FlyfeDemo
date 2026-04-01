using UnityEngine;
using System.Collections;
using Flyfe.Core;

namespace Flyfe.Gameplay
{
    /// <summary>
    /// A professional-grade feedback component for interactable platforms.
    /// Provides isolated visual feedback without affecting physical stability.
    /// </summary>
    [SelectionBase] 
    public class PlatformFeedback : MonoBehaviour, IResettable
    {
        [Header("Appearance")]
        [SerializeField] private Color landedColor = Color.cyan;
        [SerializeField] private float pulseScale = 1.03f;
        [SerializeField] private float pulseDuration = 0.15f;

        [Header("Configuration")]
        [SerializeField] private bool triggerByShadow = true;
        [Tooltip("The visual child that will pulse. If null, this object is used.")]
        [SerializeField] private Transform visualRoot;

        // Internal State
        private SpriteRenderer _spriteRenderer;
        private Color _originalColor;
        private Vector3 _originalScale;
        private bool _isLanded;
        private Coroutine _feedbackCoroutine;
        private float _exitCooldown;

        private void Awake()
        {
            // Cache visuals
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            
            // Default visual root to this object if not specified
            if (visualRoot == null) visualRoot = (_spriteRenderer != null) ? _spriteRenderer.transform : transform;
            
            if (_spriteRenderer != null) _originalColor = _spriteRenderer.color;
            _originalScale = visualRoot.localScale;
        }

        private void OnCollisionEnter2D(Collision2D collision) => HandleEntry(collision.gameObject);
        private void OnTriggerEnter2D(Collider2D other) => HandleEntry(other.gameObject);

        private void HandleEntry(GameObject other)
        {
            if (_isLanded || Time.time < _exitCooldown) return;

            if (other.CompareTag(Tags.Player) || (triggerByShadow && other.CompareTag(Tags.Shadow)))
            {
                // Professional height check: ensures landing is on the top 20% of the object
                float playerFeet = other.transform.position.y;
                float platformTop = transform.position.y;

                if (playerFeet > platformTop - 0.1f)
                {
                    StartFeedback();
                }
            }
        }

        private void OnCollisionExit2D(Collision2D collision) => HandleExit(collision.gameObject);
        private void OnTriggerExit2D(Collider2D other) => HandleExit(other.gameObject);

        private void HandleExit(GameObject other)
        {
            if (other.CompareTag(Tags.Player) || (triggerByShadow && other.CompareTag(Tags.Shadow)))
            {
                _isLanded = false;
                _exitCooldown = Time.time + 0.15f; // Short buffer to prevent jitter
            }
        }

        private void StartFeedback()
        {
            _isLanded = true;
            
            if (_spriteRenderer != null) _spriteRenderer.color = landedColor;

            if (_feedbackCoroutine != null) StopCoroutine(_feedbackCoroutine);
            _feedbackCoroutine = StartCoroutine(FeedbackRoutine());
        }

        private IEnumerator FeedbackRoutine()
        {
            float elapsed = 0f;
            Vector3 targetScale = _originalScale * pulseScale;
            float halfDuration = pulseDuration * 0.5f;

            // Animate Up
            while (elapsed < halfDuration)
            {
                visualRoot.localScale = Vector3.Lerp(_originalScale, targetScale, elapsed / halfDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            visualRoot.localScale = targetScale;

            // Animate Down
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                visualRoot.localScale = Vector3.Lerp(targetScale, _originalScale, elapsed / halfDuration);
                elapsed += Time.deltaTime;
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
            if (_spriteRenderer != null) _spriteRenderer.color = _originalColor;
        }
    }
}
