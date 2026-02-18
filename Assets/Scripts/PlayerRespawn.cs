using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerRespawn : MonoBehaviour
{
    public float fallThreshold = -10f;
    public Transform respawnPoint;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Only check for fall death if we are NOT in memory mode 
        // OR if the shadow falls (depending on your game design)
        if (transform.position.y < fallThreshold)
            Respawn();
    }

    public void Respawn()
    {
        // 1. Force the World State back to Present
        if (RecordManager.Instance != null)
        {
            RecordManager.Instance.ForceResetToPresent();
        }

        // 2. Handle the physical relocation
        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
            
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