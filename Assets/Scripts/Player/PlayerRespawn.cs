using UnityEngine;
using UnityEngine.SceneManagement;
using Flyfe.UI;
using Flyfe.Dialogue;
using Flyfe.Recording;
using Flyfe.Camera;

namespace Flyfe.Player
{
    public class PlayerRespawn : MonoBehaviour
    {
        [SerializeField] private float fallThreshold = -50f;
        [SerializeField] private Transform respawnPoint;

        private Rigidbody2D _rb;
        private float _startTime;
        private float _lastRespawnTime;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _startTime = Time.time;
        }

        void Update()
        {
            // Give systems a moment to settle, and add a small cooldown after respawn
            if (Time.time - _startTime < 0.5f || Time.time - _lastRespawnTime < 0.5f) return;

            // Pause during dialogue/cutscenes
            if ((DialogueUI.Instance != null && DialogueUI.Instance.IsOpen) || CutsceneController.AnyCutsceneActive) return;

            // Trigger respawn if below threshold
            if (transform.position.y < fallThreshold)
            {
                Respawn();
            }
        }

        public void SetRespawnPoint(Transform newPoint) => respawnPoint = newPoint;

        public void Respawn()
        {
            _lastRespawnTime = Time.time;

            if (respawnPoint == null)
            {
                respawnPoint = Checkpoint.GetNearestVisitedTransform(transform.position);
            }

            if (respawnPoint != null)
            {
                // 1. Clear ALL momentum
                if (_rb != null)
                {
                    _rb.linearVelocity = Vector2.zero;
                    _rb.angularVelocity = 0f;
                    _rb.totalForce = Vector2.zero;
                    _rb.totalTorque = 0f;
                }

                // 2. Perform the teleport
                transform.SetPositionAndRotation(respawnPoint.position, respawnPoint.rotation);

                // 3. Resync world elements (this triggers the camera snap and background stabilization)
                ParallaxLayer.ResyncAll();

                // 4. Reset Recording system
                if (RecordingService.Instance != null)
                {
                    RecordingService.Instance.ForceResetToPresent();
                }
            }
            else
            {
                // Fallback: Reload Scene
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }
}
