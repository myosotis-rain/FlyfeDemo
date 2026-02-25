using UnityEngine;

public class HoverSkill : MonoBehaviour, ISkill
{
    [SerializeField] private float maxDuration = 1.5f;

    private float _currentDuration;
    private bool _hasCharge = true;
    private bool _isActive = false;
    private float _originalGravityScale;
    private Rigidbody2D _characterRb;

    public bool IsActive => _isActive;

    void Update()
    {
        if (_isActive && _characterRb != null)
        {
            _currentDuration -= Time.deltaTime;
            if (_currentDuration <= 0) EndSkill(_characterRb);
            
            _characterRb.gravityScale = 0;
            _characterRb.linearVelocity = new Vector2(_characterRb.linearVelocity.x, 0);
        }
    }

    public void StartSkill(Rigidbody2D rb)
    {
        if (!_hasCharge) { Debug.Log(name + " Hover failed: No Charge!"); return; }
        if (_isActive) return;

        Debug.Log(name + " Hover STARTED!");
        _characterRb = rb;
        _isActive = true;
        _hasCharge = false;
        _currentDuration = maxDuration;
        _originalGravityScale = rb.gravityScale;
    }

    public void UpdateSkill(Rigidbody2D rb) { }

    public void EndSkill(Rigidbody2D rb)
    {
        if (!_isActive) return;
        rb.gravityScale = _originalGravityScale;
        _isActive = false;
    }

    public void CancelSkill() => EndSkill(_characterRb);

    public void Recharge()
    {
        if (!_isActive) _hasCharge = true;
    }
}
