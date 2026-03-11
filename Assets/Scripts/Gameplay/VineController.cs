using UnityEngine;

namespace Flyfe.Gameplay
{
    /// <summary>
    /// Optimized Vine Controller for use with Animation Events.
    /// Provides frame-perfect collider syncing for 3-frame animations.
    /// </summary>
    public class VineController : MonoBehaviour, IResettable
    {
        [Header("Settings")]
        [SerializeField] private bool startsGrown = false;
        
        [Header("Colliders")]
        [Tooltip("Assign ColliderSegment1, 2, 3 here in order.")]
        [SerializeField] private Collider2D[] segments;

        private Animator _animator;
        private bool _isGrown;

        private static readonly int GrowTrigger = Animator.StringToHash("Grow");
        private static readonly int ShrinkTrigger = Animator.StringToHash("Shrink");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void Start()
        {
            ResetState();
        }

        public void ResetState()
        {
            _isGrown = startsGrown;
            ApplyVisualState(true);
            
            if (_animator != null)
            {
                _animator.ResetTrigger(GrowTrigger);
                _animator.ResetTrigger(ShrinkTrigger);
            }
        }

        private void ApplyVisualState(bool immediate)
        {
            if (_isGrown)
            {
                if (_animator != null) 
                {
                    if (immediate) _animator.Play("GrownIdle", 0, 1f);
                    else _animator.SetTrigger(GrowTrigger);
                }
                SetFlowerLength(segments.Length);
            }
            else
            {
                if (_animator != null) 
                {
                    if (immediate) _animator.Play("Idle", 0, 0f);
                    else _animator.SetTrigger(ShrinkTrigger);
                }
                SetFlowerLength(1); 
            }
        }

        public void SetFlowerLength(int segmentCount)
        {
            if (segments == null || segments.Length == 0) return;
            for (int i = 0; i < segments.Length; i++)
            {
                if (segments[i] != null) segments[i].enabled = (i < segmentCount);
            }
        }

        public void Grow()
        {
            if (_isGrown) return;
            _isGrown = true;
            if (_animator != null)
            {
                _animator.SetTrigger(GrowTrigger);
                _animator.ResetTrigger(ShrinkTrigger);
            }
        }

        public void Shrink()
        {
            if (!_isGrown) return;
            _isGrown = false;
            if (_animator != null)
            {
                _animator.SetTrigger(ShrinkTrigger);
                _animator.ResetTrigger(GrowTrigger);
            }
        }

        /// <summary>
        /// Explicit method for linking to RotarySwitch UnityEvents
        /// </summary>
        public void OnRotarySwitchChanged(bool isOn)
        {
            if (isOn) Grow();
            else Shrink();
        }

        /// <summary>
        /// Explicit method for linking to MemorySwitch UnityEvents
        /// </summary>
        public void OnMemorySwitchChanged(bool isOn)
        {
            if (isOn) Grow();
            else Shrink();
        }

        public bool IsGrown => _isGrown;
    }
}
