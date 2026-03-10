using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class KinematicPlatform : MonoBehaviour
{
    [SerializeField] private Vector3 travelOffset = new Vector3(5, 0, 0); 
    [SerializeField] private float transitSpeed = 2.5f;

    private Rigidbody2D _rb;
    private Vector3 _startPosition;
    private Vector3 _targetPosition;
    private float _timeOffset;
    private Vector3 _lastPosition;
    private BoxCollider2D _col;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.useFullKinematicContacts = true;
        _rb.simulated = true;

        _col = GetComponent<BoxCollider2D>();
        _startPosition = transform.position;
        _targetPosition = _startPosition + travelOffset;
        _lastPosition = transform.position;
    }

    void FixedUpdate()
    {
        // Pause movement during dialogue or cutscenes
        if ((DialogueUI.Instance != null && DialogueUI.Instance.IsOpen) || CutsceneController.AnyCutsceneActive) return;
        
        float distance = travelOffset.magnitude;
        if (distance < 0.01f) return;

        // Use Time.timeSinceLevelLoad or similar for more consistent movement across state swaps if needed, 
        // but Time.time + _timeOffset is what was there.
        float movementFactor = Mathf.PingPong((Time.time + _timeOffset) * transitSpeed / distance, 1f);
        Vector3 newPos = Vector3.Lerp(_startPosition, _targetPosition, movementFactor);
        
        // Calculate delta BEFORE moving
        Vector3 delta = newPos - _lastPosition;

        // Move the platform's Rigidbody
        _rb.MovePosition(newPos);

        // Detect anything standing on top
        Vector2 boxSize = new Vector2(_col.size.x * 0.9f, 0.2f);
        // Cast from the NEW position to find what WILL be on top
        Vector2 boxCenter = (Vector2)newPos + Vector2.up * (_col.size.y / 2 + 0.1f);
        
        RaycastHit2D[] hits = Physics2D.BoxCastAll(boxCenter, boxSize, 0f, Vector2.up, 0.1f);

        foreach (var hit in hits)
        {
            if (hit.collider.CompareTag(Tags.Player) || hit.collider.CompareTag(Tags.Shadow))
            {
                // If it has a Rigidbody, move its physics position to maintain interpolation
                if (hit.collider.TryGetComponent<Rigidbody2D>(out var targetRb))
                {
                    targetRb.position += (Vector2)delta;
                }
                else
                {
                    hit.collider.transform.position += delta;
                }
            }
        }

        _lastPosition = newPos;
    }

    public void ResetState() 
    {
        _timeOffset = -Time.time; 
        transform.position = _startPosition;
        _lastPosition = _startPosition;
    }
}
