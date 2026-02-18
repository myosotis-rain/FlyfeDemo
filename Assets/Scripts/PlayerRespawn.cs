using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerRespawn : MonoBehaviour
{
    public float fallThreshold = -10f;
    public Transform respawnPoint;

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (transform.position.y < fallThreshold)
            Respawn();
    }

    public void Respawn()
    {
        // Always force present world on death
        if (RecordManager.Instance != null)
            RecordManager.Instance.ForceResetToPresent();

        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
