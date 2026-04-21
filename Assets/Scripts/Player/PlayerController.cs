using UnityEngine;
using Flyfe.Skills;

namespace Flyfe.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 4f; 
        [SerializeField] private float climbSpeed = 5f;

        [Header("Jumping")]
        [SerializeField] private float jumpForce = 12f;

        [Header("Slope Handling")]
        [SerializeField] private float slopeRotateSpeed = 12f; 
        [SerializeField] private float maxRotationAngle = 30f; 
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
        private Vector2 _lastMoveInput;
        private Rigidbody2D _groundFilterRb; // Renamed to avoid confusion
        private ContactFilter2D _groundFilter;
        private Transform _activePlatform;
        private float _unparentTimer = 0f;
        private float _jumpGroundLockout = 0f;
        private float _parentLockout = 0f; // New lockout for parenting
        private const float UNPARENT_GRACE_TIME = 0.15f;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>(); 
            if (_animator != null) _animator.applyRootMotion = false;
            
            _initialGravityScale = _rigidbody.gravityScale;
            _skillManager = GetComponent<SkillManager>(); 

            // Initialize ground filter to ignore triggers
            _groundFilter = new ContactFilter2D();
            _groundFilter.useLayerMask = true;
            _groundFilter.layerMask = groundLayer;
            _groundFilter.useTriggers = false; // THIS IS THE CRITICAL FIX

            if (_rigidbody.interpolation == RigidbodyInterpolation2D.None)
            {
                _rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
            }
            
            // Professional Practice: Continuous collision detection prevents 
            // 'tunneling' through moving platforms or thin floors.
            _rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        void Start()
        {
            if (groundCheck == null)
            {
                groundCheck = transform.Find("ShadowGroundCheck");
            }
        }

        void FixedUpdate()
        {
            UpdateGroundInfo();
            
            if (_isGrounded)
            {
                _skillManager?.ActiveSkill?.Recharge();
            }

            if (IsSkillActive)
            {
                _skillManager.ActiveSkill.UpdateSkill(_rigidbody);
            }

            ApplyMovement(_lastMoveInput);
            AlignWithGround();
        }

        private void UpdateGroundInfo()
        {
            if (_jumpGroundLockout > 0)
            {
                _jumpGroundLockout -= Time.fixedDeltaTime;
                _isGrounded = false;
                _onSlope = false;
                _groundFilterRb = null;
                return;
            }

            if (groundCheck == null)
            {
                _isGrounded = false;
                _onSlope = false;
                _groundNormal = Vector2.up;
                _groundFilterRb = null;
                return;
            }

            // Priority: If we are on a vine, we are not "grounded" in a way that should trigger slope logic
            if (IsOnVine())
            {
                _isGrounded = false;
                _onSlope = false;
                _groundNormal = Vector2.up;
                _groundFilterRb = null;
                return;
            }

            Vector2 rayStart = (Vector2)groundCheck.position + Vector2.up * 0.1f;
            float totalDist = raycastDistance + 0.1f; 
            
            // Use filtered cast to ignore triggers and catch multiple potential hits
            RaycastHit2D[] hits = new RaycastHit2D[5]; 
            int hitCount = Physics2D.BoxCast(rayStart, new Vector2(groundCheckRadius * 1.5f, 0.05f), _rigidbody.rotation, Vector2.down, _groundFilter, hits, totalDist);

            bool foundGround = false;
            for (int i = 0; i < hitCount; i++)
            {
                // Professional Practice: Ignore the player's own collider
                if (hits[i].collider != null && hits[i].collider.gameObject != gameObject)
                {
                    _isGrounded = true;
                    _groundNormal = hits[i].normal;

                    float slopeThreshold = _onSlope ? 0.05f : 0.1f;
                    _onSlope = Mathf.Abs(_groundNormal.x) > slopeThreshold;

                    _groundFilterRb = hits[i].collider.attachedRigidbody;
                    foundGround = true;
                    _unparentTimer = UNPARENT_GRACE_TIME; // Reset the buffer while on ground
                    break;
                }
            }

            if (!foundGround)
            {
                Vector2 center = (Vector2)groundCheck.position + Vector2.down * groundCheckVerticalOffset;
                Collider2D[] results = new Collider2D[5];
                int overlapCount = Physics2D.OverlapBox(center, new Vector2(groundCheckRadius * 2, 0.1f), 0f, _groundFilter, results);
                
                _isGrounded = false;
                _groundFilterRb = null;

                for (int i = 0; i < overlapCount; i++)
                {
                    if (results[i].gameObject != gameObject)
                    {
                        _isGrounded = true;
                        _groundFilterRb = results[i].attachedRigidbody;
                        break;
                    }
                }
                
                _onSlope = false;
                _groundNormal = Vector2.up;
            }
        }

        private void AlignWithGround()
        {
            if (_rigidbody == null) return;

            float targetAngle = 0f;
            // Removed the y-velocity check as it causes jitters on upward-swinging platforms
            if (_isGrounded)
            {
                targetAngle = Vector2.SignedAngle(Vector2.up, _groundNormal);
                targetAngle = Mathf.Clamp(targetAngle, -maxRotationAngle, maxRotationAngle);
            }
            
            float currentAngle = _rigidbody.rotation;
            float angleDiff = Mathf.Abs(Mathf.DeltaAngle(currentAngle, targetAngle));

            if (angleDiff < 0.1f) 
            {
                _rigidbody.MoveRotation(targetAngle);
                _rigidbody.angularVelocity = 0f; // Prevent rotation drift jitter
                return;
            }

            float smoothedAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.fixedDeltaTime * slopeRotateSpeed);
            _rigidbody.MoveRotation(smoothedAngle);
            _rigidbody.angularVelocity = 0f; // Prevent rotation drift jitter
        }

        public void Move(Vector2 moveInput) => _lastMoveInput = moveInput;

        private void ApplyMovement(Vector2 moveInput)
        {
            if (_rigidbody == null) return;

            bool isOnVine = IsOnVine();

            if (_animator != null)
            {
                float animSpeed = (isOnVine || _isGrounded) ? Mathf.Abs(moveInput.x) : 0f;
                if (isOnVine && Mathf.Abs(moveInput.y) > 0.1f) animSpeed = Mathf.Abs(moveInput.y);
                
                _animator.SetFloat("Speed", animSpeed);
                _animator.SetBool("Grounded", _isGrounded || isOnVine);
            }

            if (moveInput.x > 0.01f)
                transform.localScale = new Vector3(1, 1, 1);
            else if (moveInput.x < -0.01f)
                transform.localScale = new Vector3(-1, 1, 1);
            
            if (isOnVine)
            {
                _rigidbody.gravityScale = 0f;
                // STABLE CLIMBING: We set velocity directly.
                // We add a tiny bit of horizontal movement to allow getting off the vine.
                _rigidbody.linearVelocity = new Vector2(moveInput.x * moveSpeed, moveInput.y * climbSpeed);
            }
            else if (IsSkillActive)
            {
                _rigidbody.linearVelocity = new Vector2(moveInput.x * moveSpeed, _rigidbody.linearVelocity.y);
            }
            else
            {
                _rigidbody.gravityScale = _initialGravityScale;
                
                Vector2 platformVel = Vector2.zero;
                // We only parent if we are upright (standing on the flower, not hitting the side)
                bool isUpright = _isGrounded && _groundNormal.y > 0.5f;

                if (_parentLockout > 0) _parentLockout -= Time.fixedDeltaTime;

                if (isUpright && _groundFilterRb != null && _groundFilterRb.bodyType == RigidbodyType2D.Kinematic)
                {
                    platformVel = _groundFilterRb.GetPointVelocity(_rigidbody.position);
                    
                    // Only parent if we are falling or already grounded (to avoid snapping mid-jump)
                    bool isLanding = _rigidbody.bodyType == RigidbodyType2D.Dynamic && _rigidbody.linearVelocity.y <= 0.1f;
                    bool isAlreadyParented = _activePlatform != null;

                    if ((isLanding && _parentLockout <= 0) || isAlreadyParented)
                    {
                        if (_activePlatform != _groundFilterRb.transform)
                        {
                            _activePlatform = _groundFilterRb.transform;
                            transform.SetParent(_activePlatform);
                            
                            // Switch to Kinematic to perfectly follow the flower's rotation/swing
                            _rigidbody.bodyType = RigidbodyType2D.Kinematic;
                            _rigidbody.linearVelocity = Vector2.zero;
                        }

                        _rigidbody.gravityScale = 0;

                        // While Kinematic and parented, we move the transform locally
                        if (Mathf.Abs(moveInput.x) > 0.01f)
                        {
                            transform.Translate(new Vector3(moveInput.x * moveSpeed * Time.fixedDeltaTime, 0, 0), Space.Self);
                        }
                    }
                }
                else if (_activePlatform != null)
                {
                    // Clean Exit (Walking off the edge)
                    _unparentTimer -= Time.fixedDeltaTime;
                    if (_unparentTimer <= 0)
                    {
                        ExitPlatform(platformVel);
                    }
                }

                if (_rigidbody.bodyType == RigidbodyType2D.Dynamic)
                {
                    // Normal Movement (Air or static ground)
                    _rigidbody.linearVelocity = new Vector2(moveInput.x * moveSpeed, _rigidbody.linearVelocity.y);
                }
            }
        }

        private void ExitPlatform(Vector2 inheritance)
        {
            if (_activePlatform == null) return;
            
            _rigidbody.bodyType = RigidbodyType2D.Dynamic;
            _rigidbody.gravityScale = _initialGravityScale;
            _rigidbody.linearVelocity = inheritance;
            
            _activePlatform = null;
            transform.SetParent(null);
            _parentLockout = 0.1f; // Prevent immediate re-parenting to this or other platforms
        }

        public void Jump()
        {
            if (_rigidbody == null) return;
            
            bool isOnVine = IsOnVine();
            if (!isOnVine && !_isGrounded) return;

            // Momentum Inheritance: Capture platform speed before we detach
            Vector2 inheritance = Vector2.zero;
            if (_activePlatform != null && _groundFilterRb != null)
            {
                inheritance = _groundFilterRb.GetPointVelocity(_rigidbody.position);
                ExitPlatform(inheritance);
            }

            _jumpGroundLockout = 0.2f; // Prevent re-sticking for 0.2s
            _rigidbody.gravityScale = _initialGravityScale;
            
            // Launch = (Platform Speed) + (Horizontal Input) + (Jump Force)
            float jumpX = (_lastMoveInput.x * moveSpeed) + inheritance.x;
            _rigidbody.linearVelocity = new Vector2(jumpX, jumpForce + inheritance.y);
            
            _isGrounded = false;
        }

        public void StartSkill() => _skillManager?.ActiveSkill?.StartSkill(_rigidbody);
        public void DeactivateSkill() => _skillManager?.ActiveSkill?.EndSkill(_rigidbody);
        public void CancelSkill() => _skillManager?.ActiveSkill?.CancelSkill();
        public bool IsSkillActive => _skillManager?.ActiveSkill?.IsActive ?? false;
        public bool IsGrounded() => _isGrounded;

        private bool IsOnVine()
        {
            // Use a vertical capsule-like check to ensure we don't fall off the vine while climbing
            Collider2D hit = Physics2D.OverlapBox(transform.position, new Vector2(0.7f, 1.2f), 0f, vineLayer);
            return hit != null;
        }
    }
}
