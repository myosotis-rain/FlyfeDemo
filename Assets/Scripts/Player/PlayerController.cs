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
    [SerializeField] private float groundCheckVerticalOffset = 0.05f;

    private Rigidbody2D _rigidbody;
    private float _initialGravityScale;
    private SkillManager _skillManager; // Reference to the attached SkillManager

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _initialGravityScale = _rigidbody.gravityScale;
        _skillManager = GetComponent<SkillManager>(); // Get the SkillManager on this character
    }

    void Start()
    {
        // If groundCheck wasn't assigned in the Inspector, try to find a child named "ShadowGroundCheck"
        if (groundCheck == null)
        {
            groundCheck = transform.Find("ShadowGroundCheck");
        }

        // Safety: If groundLayer is 0 (Nothing), it might be why jump fails
        if (groundLayer == 0)
        {
            Debug.LogWarning(name + " has no Ground Layer set! Jumping will not work.");
        }
    }

    void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = IsGrounded() ? Color.green : Color.red;
            Vector3 center = groundCheck.position + Vector3.down * groundCheckVerticalOffset;
            Gizmos.DrawWireCube(center, new Vector3(groundCheckRadius * 2, 0.1f, 0));
        }
    }

    void FixedUpdate()
    {
        // Recharge the active skill if the character is grounded
        if (IsGrounded())
        {
            _skillManager?.ActiveSkill?.Recharge();
        }
    }

    public void SyncSettings(PlayerController source)
    {
        if (source == null) return;
        this.groundLayer = source.groundLayer;
        this.vineLayer = source.vineLayer;
        this.groundCheckRadius = source.groundCheckRadius;
        this.groundCheckVerticalOffset = source.groundCheckVerticalOffset; // Added sync for offset
        this.moveSpeed = source.moveSpeed;
        this.jumpForce = source.jumpForce;
        this.climbSpeed = source.climbSpeed;
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
        else if (IsSkillActive)
        {
            // If a skill is active (like Hover), we only update horizontal velocity.
            // The skill itself manages gravity and vertical velocity.
            _rigidbody.linearVelocity = new Vector2(moveInput.x * moveSpeed, _rigidbody.linearVelocity.y);
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

    // --- Skill Methods ---
    public void StartSkill()
    {
        _skillManager?.ActiveSkill?.StartSkill(_rigidbody);
    }

    public void DeactivateSkill()
    {
        _skillManager?.ActiveSkill?.EndSkill(_rigidbody);
    }

    public void CancelSkill()
    {
        _skillManager?.ActiveSkill?.CancelSkill();
    }

    public bool IsSkillActive => _skillManager?.ActiveSkill?.IsActive ?? false;

    public bool IsGrounded()
    {
        if (groundCheck == null) return false;
        Vector2 center = (Vector2)groundCheck.position + Vector2.down * groundCheckVerticalOffset;
        Collider2D hit = Physics2D.OverlapBox(center, new Vector2(groundCheckRadius * 2, 0.1f), 0f, groundLayer);
        return hit != null;
    }

    private bool IsOnVine()
    {
        // Using the same check as for ground, but for the vine layer.
        // You might want a different transform or radius for this check depending on your art.
        if (groundCheck == null) return false;
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, vineLayer);
    }
}