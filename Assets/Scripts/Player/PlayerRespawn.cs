using UnityEngine;
using UnityEngine.SceneManagement;
using Flyfe.UI;
using Flyfe.Dialogue;
using Flyfe.Recording;
using Flyfe.Camera;
using Flyfe.Core;
using System.Collections;

namespace Flyfe.Player
{
    /// <summary>
    /// Professional Respawn System.
    /// Handles death (hazards/pits), screen transitions, and world-state resets.
    /// </summary>
    public class PlayerRespawn : MonoBehaviour
    {
        [Header("Death Settings")]
        [Tooltip("The Y-coordinate below which the player is considered to have fallen out of bounds.")]
        [SerializeField] private float fallThreshold = -50f;
        [Tooltip("How long the screen stays black during respawn.")]
        [SerializeField] private float fadeDuration = 0.4f;

        [Header("Runtime State")]
        [SerializeField] private Transform respawnPoint;

        private Rigidbody2D _rb;
        private PlayerController _controller;
        private bool _isRespawning = false;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _controller = GetComponent<PlayerController>();
        }

        void Update()
        {
            if (_isRespawning) return;

            // 1. Fall Detection
            if (transform.position.y < fallThreshold)
            {
                Die();
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // 2. Spike/Hazard Detection (using Collision)
            if (collision.gameObject.CompareTag(Tags.Hazard)) Die();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // 2b. Spike/Hazard Detection (using Trigger for overlapping hazards)
            if (other.CompareTag(Tags.Hazard)) Die();
        }

        public void SetRespawnPoint(Transform newPoint) => respawnPoint = newPoint;

        public void Die()
        {
            if (_isRespawning) return;
            StartCoroutine(RespawnSequence());
        }

        private IEnumerator RespawnSequence()
        {
            _isRespawning = true;

            // A. Freeze Player
            if (_rb != null) _rb.simulated = false;
            if (_controller != null) _controller.enabled = false;

            // B. Fade Out
            if (ScreenFader.Instance != null)
            {
                yield return ScreenFader.Instance.FadeOutCoroutine(fadeDuration);
            }

            // C. Relocate & Reset
            PerformTeleport();

            // D. Wait a tiny bit for camera/physics to settle while black
            yield return new WaitForSeconds(0.1f);

            // E. Fade In
            if (ScreenFader.Instance != null)
            {
                yield return ScreenFader.Instance.FadeInCoroutine(fadeDuration);
            }

            // F. Restore Player
            if (_rb != null) _rb.simulated = true;
            if (_controller != null) _controller.enabled = true;
            
            _isRespawning = false;
        }

        private void PerformTeleport()
        {
            // 1. Determine destination
            if (respawnPoint == null)
            {
                respawnPoint = Checkpoint.GetNearestVisitedTransform(transform.position);
            }

            if (respawnPoint != null)
            {
                // 2. Hierarchy and Physics Cleanup
                transform.SetParent(null);

                if (_rb != null)
                {
                    _rb.linearVelocity = Vector2.zero;
                    _rb.angularVelocity = 0f;
                    _rb.totalForce = Vector2.zero;
                    _rb.totalTorque = 0f;
                    
                    _rb.position = respawnPoint.position;
                    _rb.rotation = respawnPoint.rotation.eulerAngles.z;
                }

                transform.SetPositionAndRotation(respawnPoint.position, respawnPoint.rotation);

                // 3. System Resync
                ParallaxLayer.ResyncAll();

                if (RecordingService.Instance != null)
                {
                    RecordingService.Instance.ForceResetToPresent();
                }
            }
            else
            {
                // Absolute Fallback: Reload current scene
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }
}
