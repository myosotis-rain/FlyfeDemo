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
        private Rigidbody2D _groundRb;
        private Transform _originalParent;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>(); 
            if (_animator != null) _animator.applyRootMotion = false;
            
            _initialGravityScale = _rigidbody.gravityScale;
            _skillManager = GetComponent<SkillManager>(); 

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
            if (groundCheck == null)
            {
                _isGrounded = false;
                _onSlope = false;
                _groundNormal = Vector2.up;
                _groundRb = null;
                return;
            }

            // Priority: If we are on a vine, we are not "grounded" in a way that should trigger slope logic
            if (IsOnVine())
            {
                _isGrounded = false;
                _onSlope = false;
                _groundNormal = Vector2.up;
                _groundRb = null;
                return;
            }

            Vector2 rayStart = (Vector2)groundCheck.position + Vector2.up * 0.1f;
            float totalDist = raycastDistance + 0.1f; 
            
            RaycastHit2D hit = Physics2D.BoxCast(rayStart, new Vector2(groundCheckRadius * 1.5f, 0.05f), _rigidbody.rotation, Vector2.down, totalDist, groundLayer);

            if (hit.collider != null && hit.collider.gameObject != gameObject)
            {
                _isGrounded = true;
                _groundNormal = hit.normal;
                float slopeThreshold = _onSlope ? 0.02f : 0.05f;
                _onSlope = Mathf.Abs(_groundNormal.x) > slopeThreshold;
                _groundRb = hit.collider.attachedRigidbody;
            }
            else
            {
                Vector2 center = (Vector2)groundCheck.position + Vector2.down * groundCheckVerticalOffset;
                Collider2D overlapHit = Physics2D.OverlapBox(center, new Vector2(groundCheckRadius * 2, 0.1f), 0f, groundLayer);
                _isGrounded = overlapHit != null;
                
                _onSlope = false;
                _groundNormal = Vector2.up;
                _groundRb = overlapHit != null ? overlapHit.attachedRigidbody : null;
            }
        }

        private void AlignWithGround()
        {
            if (_rigidbody == null) return;

            float targetAngle = 0f;
            if (_isGrounded && _rigidbody.linearVelocity.y < 0.1f)
            {
                targetAngle = Vector2.SignedAngle(Vector2.up, _groundNormal);
                targetAngle = Mathf.Clamp(targetAngle, -maxRotationAngle, maxRotationAngle);
            }
            
            float currentAngle = _rigidbody.rotation;
            float angleDiff = Mathf.Abs(Mathf.DeltaAngle(currentAngle, targetAngle));

            if (angleDiff < 0.2f) 
            {
                _rigidbody.MoveRotation(targetAngle);
                return;
            }

            float smoothedAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.fixedDeltaTime * slopeRotateSpeed);
            _rigidbody.MoveRotation(smoothedAngle);
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
                if (_isGrounded && _groundRb != null && _groundRb.bodyType != RigidbodyType2D.Static)
                {
                    // Inherit the exact velocity of the moving/rotating platform at the player's position
                    platformVel = _groundRb.GetPointVelocity(_rigidbody.position);
                }

                if (_isGrounded && _onSlope && _rigidbody.linearVelocity.y < 0.1f)
                {
                    if (Mathf.Abs(moveInput.x) > 0.01f)
                    {
                        Vector2 slopeTangent = new Vector2(_groundNormal.y, -_groundNormal.x);
                        Vector2 moveDirection = slopeTangent * moveInput.x;
                        _rigidbody.linearVelocity = new Vector2(moveDirection.x * moveSpeed + platformVel.x, moveDirection.y * moveSpeed + platformVel.y);
                    }
                    else
                    {
                        // Add a downward 'glue' force to stick to the curving pendulum instead of tangentially flying off
                        _rigidbody.linearVelocity = platformVel - (_groundNormal * 2f);
                        _rigidbody.gravityScale = 0; 
                    }
                }
                else
                {
                    if (Mathf.Abs(moveInput.x) > 0.01f)
                    {
                        _rigidbody.linearVelocity = new Vector2(moveInput.x * moveSpeed + platformVel.x, _rigidbody.linearVelocity.y);
                    }
                    else if (platformVel != Vector2.zero)
                    {
                        // Stick to flat moving platform (like the bottom of the pendulum arc)
                        _rigidbody.linearVelocity = platformVel - (_groundNormal * 2f);
                    }
                    else
                    {
                        _rigidbody.linearVelocity = new Vector2(0, _rigidbody.linearVelocity.y);
                    }
                }
            }
        }

        public void Jump()
        {
            if (_rigidbody == null) return;
            
            bool isOnVine = IsOnVine();
            if (!isOnVine && !_isGrounded) return;

            _rigidbody.gravityScale = _initialGravityScale;
            _rigidbody.linearVelocity = new Vector2(_rigidbody.linearVelocity.x, jumpForce);
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
