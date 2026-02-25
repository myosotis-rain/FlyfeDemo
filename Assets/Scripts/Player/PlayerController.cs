using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float climbSpeed = 5f;

    [Header("Jumping")]
    [SerializeField] private float jumpForce = 12f;

    [Header("Collision Layers")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask vineLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.25f;

    private Rigidbody2D _rigidbody;
    private float _initialGravityScale;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _initialGravityScale = _rigidbody.gravityScale;
    }

    public void Move(Vector2 moveInput)
    {
        if (_rigidbody == null) return;

        bool isOnVine = IsOnVine();
        
        if (isOnVine)
        {
            // Vine Movement: Use the Y-axis of the input, disable gravity.
            _rigidbody.gravityScale = 0f;
            _rigidbody.linearVelocity = new Vector2(_rigidbody.linearVelocity.x, moveInput.y * climbSpeed);
        }
        else
        {
            // Ground Movement: Use the X-axis of the input, restore gravity.
            _rigidbody.gravityScale = _initialGravityScale;
            _rigidbody.linearVelocity = new Vector2(moveInput.x * moveSpeed, _rigidbody.linearVelocity.y);
        }
    }

    public void Jump()
    {
        // Do not allow jumping if we are on a vine or not on the ground.
        if (_rigidbody == null || IsOnVine() || !IsGrounded()) return;

        _rigidbody.linearVelocity = new Vector2(_rigidbody.linearVelocity.x, jumpForce);
    }

    public bool IsGrounded()
    {
        if (groundCheck == null) return false;
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private bool IsOnVine()
    {
        // Using the same check as for ground, but for the vine layer.
        // You might want a different transform or radius for this check depending on your art.
        if (groundCheck == null) return false;
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, vineLayer);
    }
}