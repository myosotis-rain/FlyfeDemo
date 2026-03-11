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

        public void ResetState()
        {
            _isGrown = startsGrown;
            ApplyVisualState(true);
        }

        private void ApplyVisualState(bool immediate)
        {
            if (_animator != null)
            {
                // Use a Boolean parameter if it exists for more stability
                if (HasParameter("IsGrown")) _animator.SetBool(IsGrownBool, _isGrown);
                
                if (_isGrown)
                {
                    if (immediate) _animator.Play("GrownIdle", 0, 1f); // If this fails, it logs but doesn't crash
                    else _animator.SetTrigger(GrowTrigger);
                    SetFlowerLength(segments.Length);
                }
                else
                {
                    if (immediate) _animator.Play("Idle", 0, 0f);
                    else _animator.SetTrigger(ShrinkTrigger);
                    SetFlowerLength(1); 
                }
            }
        }

        private bool HasParameter(string paramName)
        {
            if (_animator == null) return false;
            foreach (var param in _animator.parameters) if (param.name == paramName) return true;
            return false;
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
