using UnityEngine;

namespace Flyfe.Gameplay
{
    public class VineController : MonoBehaviour, IResettable
    {
        [Header("Settings")]
        [SerializeField] private bool startsGrown = false;
        
        [Header("Colliders")]
        [SerializeField] private Collider2D[] segments;

        private Animator _animator;
        private bool _isGrown;

        private static readonly int GrowTrigger = Animator.StringToHash("Grow");
        private static readonly int ShrinkTrigger = Animator.StringToHash("Shrink");
        private static readonly int IsGrownBool = Animator.StringToHash("IsGrown");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void Start()
        {
            ResetState();
        }

        private void Update()
        {
            if (_animator == null || segments == null || segments.Length == 0) return;

            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

            // Sync collider segments to animation progress
            if (stateInfo.IsName("Grow"))
            {
                int activeIndex = Mathf.Clamp(Mathf.FloorToInt(stateInfo.normalizedTime * segments.Length), 0, segments.Length - 1);
                SetFlowerLength(activeIndex);
            }
            else if (stateInfo.IsName("Shrink"))
            {
                int activeIndex = Mathf.Clamp(Mathf.FloorToInt((1f - stateInfo.normalizedTime) * segments.Length), 0, segments.Length - 1);
                SetFlowerLength(activeIndex);
            }
            else
            {
                // Rely on the logical state if not currently playing a transition animation.
                // This fixes the bug where StartsGrown was broken by animator lag on frame 1.
                SetFlowerLength(_isGrown ? segments.Length - 1 : -1);
            }
        }

        public void ResetState()
        {
            _isGrown = startsGrown;
            ApplyVisualState(true);
        }

        private void ApplyVisualState(bool immediate)
        {
            if (_animator != null)
            {
                if (HasParameter("IsGrown")) _animator.SetBool(IsGrownBool, _isGrown);
                
                if (_isGrown)
                {
                    if (immediate) 
                    {
                        _animator.Play("GrownIdle", 0, 1f);
                        SetFlowerLength(segments.Length - 1);
                    }
                    else 
                    {
                        _animator.SetTrigger(GrowTrigger);
                    }
                }
                else
                {
                    if (immediate) 
                    {
                        _animator.Play("Idle", 0, 0f);
                        SetFlowerLength(-1); 
                    }
                    else 
                    {
                        _animator.SetTrigger(ShrinkTrigger);
                    }
                }
            }
        }

        private bool HasParameter(string paramName)
        {
            if (_animator == null) return false;
            foreach (var param in _animator.parameters) if (param.name == paramName) return true;
            return false;
        }

        public void SetFlowerLength(int activeIndex)
        {
            if (segments == null || segments.Length == 0) return;
            for (int i = 0; i < segments.Length; i++)
            {
                // If each collider represents the FULL vine for that specific frame, 
                // we only enable the ONE active collider, ensuring they are individual.
                if (segments[i] != null && segments[i].enabled != (i == activeIndex))
                {
                    segments[i].enabled = (i == activeIndex);
                }
            }
        }

        public void Grow()
        {
            if (_isGrown) return;
            _isGrown = true;
            if (_animator != null) _animator.SetTrigger(GrowTrigger);
        }

        public void Shrink()
        {
            if (!_isGrown) return;
            _isGrown = false;
            if (_animator != null) _animator.SetTrigger(ShrinkTrigger);
        }

        public void OnRotarySwitchChanged(bool isOn) { if (isOn) Grow(); else Shrink(); }
        public void OnMemorySwitchChanged(bool isOn) { if (isOn) Grow(); else Shrink(); }

        public bool IsGrown => _isGrown;
    }
}
