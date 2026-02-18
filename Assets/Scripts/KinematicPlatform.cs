using UnityEngine;

public class KinematicPlatform : MonoBehaviour
{
    public Vector3 travelOffset = new Vector3(5, 0, 0); 
    public float transitSpeed = 2.5f;

    private Vector3 _startPosition;
    private Vector3 _targetPosition;
    private float _timeOffset;

    void Start()
    {
        _startPosition = transform.position;
        _targetPosition = _startPosition + travelOffset;
        _timeOffset = 0;
    }

    void FixedUpdate()
    {
        float distance = travelOffset.magnitude;
        if (distance < 0.01f) return;

        float movementFactor = Mathf.PingPong((Time.time + _timeOffset) * transitSpeed / distance, 1f);
        transform.position = Vector3.Lerp(_startPosition, _targetPosition, movementFactor);
    }

    public void ResetState() 
    {
        _timeOffset = -Time.time; 
        transform.position = _startPosition;
    }

    // --- THE FIX: Call this BEFORE deactivating the folder ---
    public void ManualReleaseChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.CompareTag("Player") || child.CompareTag("Shadow"))
            {
                child.SetParent(null);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Shadow"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                float platformTop = transform.position.y + (GetComponent<BoxCollider2D>().size.y / 2);
                if (contact.normal.y < -0.8f && collision.transform.position.y > platformTop - 0.1f) 
                {
                    collision.transform.SetParent(transform);
                    break;
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Safety check: Don't detach if the object is already being disabled
        if (!gameObject.activeInHierarchy) return;

        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Shadow"))
        {
            if (collision.transform.parent == transform)
                collision.transform.SetParent(null);
        }
    }
}