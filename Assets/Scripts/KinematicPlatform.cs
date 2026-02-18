using UnityEngine;

public class KinematicPlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("How far the platform moves from its starting point.")]
    public Vector3 travelOffset = new Vector3(5, 0, 0); 
    public float transitSpeed = 2.5f;

    private Vector3 _startPosition;
    private Vector3 _targetPosition;

    void Start()
    {
        _startPosition = transform.position;
        _targetPosition = _startPosition + travelOffset;
    }

    void FixedUpdate()
    {
        // Smoothly oscillate between 0 and 1
        float movementFactor = Mathf.PingPong(Time.time * transitSpeed / travelOffset.magnitude, 1f);
        transform.position = Vector3.Lerp(_startPosition, _targetPosition, movementFactor);
    }

    // --- STICKY FEET LOGIC ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!gameObject.activeInHierarchy || collision == null) return;

        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Shadow"))
        {
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!gameObject.activeInHierarchy || collision == null) return;

        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Shadow"))
        {
            collision.transform.SetParent(null);
        }
    }
}
