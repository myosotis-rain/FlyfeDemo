using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerRespawn : MonoBehaviour
{
    public float fallThreshold = -10f;
    public Transform respawnPoint; // Optional: Drag an empty GameObject here

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (transform.position.y < fallThreshold)
        {
            Respawn();
        }
    }

    public void Respawn()
    {
        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            // Reload scene if no point set
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}