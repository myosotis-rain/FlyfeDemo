using UnityEngine;

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
        if (_isGrown)
        {
            _animator.Play("GrownIdle", 0, 1f);
            SetFlowerLength(segments.Length);
        }
        else
        {
            _animator.Play("Idle", 0, 0f);
            SetFlowerLength(1); 
        }
        
        _animator.ResetTrigger(GrowTrigger);
        _animator.ResetTrigger(ShrinkTrigger);
    }

    /// <summary>
    /// CALLED BY ANIMATION EVENTS
    /// Frame 1: SetFlowerLength(1)
    /// Frame 2: SetFlowerLength(2)
    /// Frame 3: SetFlowerLength(3)
    /// </summary>
    public void SetFlowerLength(int segmentCount)
    {
        if (segments == null || segments.Length == 0) return;

        for (int i = 0; i < segments.Length; i++)
        {
            if (segments[i] != null)
            {
                segments[i].enabled = (i < segmentCount);
            }
        }
    }

    public void Grow()
    {
        if (_isGrown) return;
        _isGrown = true;
        _animator.SetTrigger(GrowTrigger);
        _animator.ResetTrigger(ShrinkTrigger);
    }

    public void Shrink()
    {
        if (!_isGrown) return;
        _isGrown = false;
        _animator.SetTrigger(ShrinkTrigger);
        _animator.ResetTrigger(GrowTrigger);
    }

    public void Toggle()
    {
        if (_isGrown) Shrink();
        else Grow();
    }

    public bool IsGrown => _isGrown;
}
