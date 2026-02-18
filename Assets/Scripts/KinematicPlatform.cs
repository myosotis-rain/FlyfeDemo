using UnityEngine;

public class KinematicPlatform : MonoBehaviour
{
    public Vector3 travelOffset = new Vector3(5, 0, 0); 
    public float transitSpeed = 2.5f;

    private Vector3 _startPosition;
    private Vector3 _targetPosition;
    private float _timeOffset;
    private Vector3 _lastPosition;
    private BoxCollider2D _col;

    void Start()
    {
        _col = GetComponent<BoxCollider2D>();
        _startPosition = transform.position;
        _targetPosition = _startPosition + travelOffset;
        _lastPosition = transform.position;
    }

    void FixedUpdate()
    {
        float distance = travelOffset.magnitude;
        if (distance < 0.01f) return;

        float movementFactor = Mathf.PingPong((Time.time + _timeOffset) * transitSpeed / distance, 1f);
        transform.position = Vector3.Lerp(_startPosition, _targetPosition, movementFactor);

        Vector3 delta = transform.position - _lastPosition;

        // Detect anything standing on top
        Vector2 boxSize = new Vector2(_col.size.x * 0.9f, 0.2f);
        Vector2 boxCenter = (Vector2)transform.position + Vector2.up * (_col.size.y / 2 + 0.1f);
        
        RaycastHit2D[] hits = Physics2D.BoxCastAll(boxCenter, boxSize, 0f, Vector2.up, 0.1f);

        foreach (var hit in hits)
        {
            if (hit.collider.CompareTag("Player") || hit.collider.CompareTag("Shadow"))
            {
                hit.collider.transform.position += delta;
            }
        }

        _lastPosition = transform.position;
    }

    public void ResetState() 
    {
        _timeOffset = -Time.time; 
        transform.position = _startPosition;
        _lastPosition = _startPosition;
    }
}