using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerRespawn : MonoBehaviour
{
    [SerializeField] private float fallThreshold = -50f;
    [SerializeField] private Transform respawnPoint;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private float _safetyBufferTimer = 0f;

    void Update()
    {
        // Don't respawn if a dialogue or cutscene is active
        bool isBusy = (DialogueUI.Instance != null && DialogueUI.Instance.IsOpen) || CutsceneController.AnyCutsceneActive;

        if (isBusy)
        {
            _safetyBufferTimer = 0.5f; // Keep resetting buffer while busy
            return;
        }

        // Subtract from buffer
        if (_safetyBufferTimer > 0)
        {
            _safetyBufferTimer -= Time.deltaTime;
            return;
        }

        // Only check for fall death if we are NOT in memory mode
        if (transform.position.y < fallThreshold)
            Respawn();
    }

    public void Respawn()
    {
        Debug.Log("Respawn triggered! Player Y: " + transform.position.y + " | Fall Threshold: " + fallThreshold);
        
        // 1. Force the World State back to Present
        if (RecordingService.Instance != null)
        {
            RecordingService.Instance.ForceResetToPresent();
        }

        // 2. Handle the physical relocation
        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;

            // Resync all parallax layers to prevent massive jumps when camera snaps
            ParallaxLayer.ResyncAll();

            // Safety check for Rigidbody before resetting velocity
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f; // Stops the player from spinning if they were
            }
        }
        else
        {
            // Fallback: Reload scene if no checkpoint is set
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}