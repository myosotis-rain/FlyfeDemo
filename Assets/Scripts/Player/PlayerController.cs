using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f; 
    [SerializeField] private float climbSpeed = 5f;

    [Header("Jumping")]
    [SerializeField] private float jumpForce = 12f;

    [Header("Slope Handling")]
    [SerializeField] private float slopeRotateSpeed = 8f; 
    [SerializeField] private float raycastDistance = 0.3f; 

    [Header("Collision Layers")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask vineLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.25f;
    [SerializeField] private float groundCheckVerticalOffset = 0.05f;

    private Rigidbody2D _rigidbody;
    private Animator _animator; 
    private float _initialGravityScale;
    private SkillManager _skillManager; 

    private Vector2 _groundNormal = Vector2.up;
    private bool _onSlope = false;
    private bool _isGrounded = false;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>(); 
        _initialGravityScale = _rigidbody.gravityScale;
        _skillManager = GetComponent<SkillManager>(); 

        // Ensure interpolation is on for smooth movement with Cinemachine
        if (_rigidbody.interpolation == RigidbodyInterpolation2D.None)
        {
            _rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
    }

    void Start()
    {
        if (groundCheck == null)
        {
            groundCheck = transform.Find("ShadowGroundCheck");
        }

        if (groundLayer == 0)
        {
            Debug.LogWarning(name + " has no Ground Layer set! Jumping will not work.");
        }
    }

    void FixedUpdate()
    {
        UpdateGroundInfo();
        
        if (_isGrounded)
        {
            _skillManager?.ActiveSkill?.Recharge();
        }

        // Align with ground MUST be in FixedUpdate to sync perfectly with physics
        AlignWithGround();
    }

    private void UpdateGroundInfo()
    {
        if (groundCheck == null)
        {
            _isGrounded = false;
            _onSlope = false;
            _groundNormal = Vector2.up;
            return;
        }

        // 1. Primary Check: BoxCast for stable normal and grounding
        // Start slightly above feet to avoid being "stuck" inside collider geometry
        Vector2 rayStart = (Vector2)groundCheck.position + Vector2.up * 0.1f;
        float totalDist = raycastDistance + 0.1f; 
        
        // Use a slightly narrower box for more stable grounding on edges
        // Angle the box with the player's current rotation for better slope contact
        RaycastHit2D hit = Physics2D.BoxCast(rayStart, new Vector2(groundCheckRadius * 1.5f, 0.05f), _rigidbody.rotation, Vector2.down, totalDist, groundLayer);

        if (hit.collider != null)
        {
            _isGrounded = true;
            _groundNormal = hit.normal;
            
            // Hysteresis for slope detection to prevent rapid toggling
            float slopeThreshold = _onSlope ? 0.02f : 0.05f;
            _onSlope = Mathf.Abs(_groundNormal.x) > slopeThreshold;
        }
        else
        {
            // 2. Secondary Check: OverlapBox fallback
            // This ensures we can still jump if we're on the absolute edge where the BoxCast might miss
            Vector2 center = (Vector2)groundCheck.position + Vector2.down * groundCheckVerticalOffset;
            _isGrounded = Physics2D.OverlapBox(center, new Vector2(groundCheckRadius * 2, 0.1f), 0f, groundLayer);
            
            // If we found ground via fallback, assume flat ground (safe default for edges)
            _onSlope = false;
            _groundNormal = Vector2.up;
        }
    }

    private void AlignWithGround()
    {
        if (_rigidbody == null) return;

        float targetAngle = 0f;
        // Only align if grounded AND not moving upwards rapidly (to avoid snapping during jump start)
        if (_isGrounded && _rigidbody.linearVelocity.y < 0.1f)
        {
            targetAngle = Vector2.SignedAngle(Vector2.up, _groundNormal);
        }
        
        float currentAngle = _rigidbody.rotation;
        float angleDiff = Mathf.Abs(Mathf.DeltaAngle(currentAngle, targetAngle));

        // Prevent micro-jitter by snapping when very close to target
        if (angleDiff < 0.2f) 
        {
            _rigidbody.MoveRotation(targetAngle);
            return;
        }

        // Smoothly rotate the Rigidbody
        float smoothedAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.fixedDeltaTime * slopeRotateSpeed);
        _rigidbody.MoveRotation(smoothedAngle);
    }

    void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Vector3 center = groundCheck.position + Vector3.down * groundCheckVerticalOffset;
            Gizmos.DrawWireCube(center, new Vector3(groundCheckRadius * 2, 0.1f, 0));
            
            // Draw ground normal for debugging
            if (_isGrounded)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(groundCheck.position, groundCheck.position + (Vector3)_groundNormal);
            }
        }
    }

    public void SyncSettings(PlayerController source)
    {
        if (source == null) return;
        this.groundLayer = source.groundLayer;
        this.vineLayer = source.vineLayer;
        this.groundCheckRadius = source.groundCheckRadius;
        this.groundCheckVerticalOffset = source.groundCheckVerticalOffset; 
        this.moveSpeed = source.moveSpeed;
        this.jumpForce = source.jumpForce;
        this.climbSpeed = source.climbSpeed;
        this.slopeRotateSpeed = source.slopeRotateSpeed;
        this.raycastDistance = source.raycastDistance;
    }

    public void Move(Vector2 moveInput)
    {
        if (_rigidbody == null) return;

        bool isOnVine = IsOnVine();

        // Update Animator parameters
        if (_animator != null)
        {
            // Only show walking speed if grounded; otherwise, stay in Idle/Fall pose
            float animSpeed = _isGrounded ? Mathf.Abs(moveInput.x) : 0f;
            _animator.SetFloat("Speed", animSpeed);
            _animator.SetBool("Grounded", _isGrounded);
        }

        // Flip the character sprite based on movement direction with a threshold to avoid jitter
        if (moveInput.x > 0.01f)
            transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput.x < -0.01f)
            transform.localScale = new Vector3(-1, 1, 1);
        
        if (isOnVine)
        {
            _rigidbody.gravityScale = 0f;
            _rigidbody.linearVelocity = new Vector2(_rigidbody.linearVelocity.x, moveInput.y * climbSpeed);
        }
        else if (IsSkillActive)
        {
            // Skills usually override default physics/slope logic
            _rigidbody.linearVelocity = new Vector2(moveInput.x * moveSpeed, _rigidbody.linearVelocity.y);
        }
        else
        {
            _rigidbody.gravityScale = _initialGravityScale;
            
            // Only apply slope logic if grounded and NOT moving upwards (prevents jump cancellation)
            if (_isGrounded && _onSlope && _rigidbody.linearVelocity.y < 0.1f)
            {
                if (Mathf.Abs(moveInput.x) > 0.01f)
                {
                    // Project movement onto the slope tangent for consistent speed
                    Vector2 slopeTangent = new Vector2(_groundNormal.y, -_groundNormal.x);
                    Vector2 moveDirection = slopeTangent * moveInput.x;
                    _rigidbody.linearVelocity = new Vector2(moveDirection.x * moveSpeed, moveDirection.y * moveSpeed);
                }
                else
                {
                    // Firmly stop on slopes when no input to prevent sliding/jitter
                    _rigidbody.linearVelocity = Vector2.zero;
                    _rigidbody.gravityScale = 0; 
                }
            }
            else
            {
                // Default horizontal movement
                _rigidbody.linearVelocity = new Vector2(moveInput.x * moveSpeed, _rigidbody.linearVelocity.y);
            }
        }
    }

    public void Jump()
    {
        if (_rigidbody == null || IsOnVine() || !_isGrounded) return;

        // When jumping, reset gravity immediately so we don't get "clamped" to the slope
        _rigidbody.gravityScale = _initialGravityScale;
        _rigidbody.linearVelocity = new Vector2(_rigidbody.linearVelocity.x, jumpForce);
        _isGrounded = false;
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

    public bool IsGrounded() => _isGrounded;

    private bool IsOnVine()
    {
        if (groundCheck == null) return false;
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, vineLayer);
    }
}
