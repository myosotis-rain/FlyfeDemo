using UnityEngine;

namespace Flyfe.Skills
{
    public class HoverSkill : MonoBehaviour, ISkill
    {
        [SerializeField] private float maxDuration = 1.5f;

        private float _currentDuration;
        private bool _hasCharge = true;
        private bool _isActive = false;
        private float _originalGravityScale;
        private Rigidbody2D _characterRb;

        public bool IsActive => _isActive;

        public void StartSkill(Rigidbody2D rb)
        {
            // TOGGLE LOGIC: If already active, pressing the button again cancels the skill
            if (_isActive)
            {
                EndSkill(rb);
                return;
            }

            if (!_hasCharge) return;

            Debug.Log(name + " Hover STARTED (Toggle)!");
            _characterRb = rb;
            _isActive = true;
            _hasCharge = false;
            _currentDuration = maxDuration;
            _originalGravityScale = rb.gravityScale;
            
            rb.gravityScale = 0;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        }

        public void UpdateSkill(Rigidbody2D rb)
        {
            if (!_isActive) return;

            _currentDuration -= Time.fixedDeltaTime;
            
            // CANCEL LOGIC: If the player presses 'Down', cancel the hover immediately
            // We check the PlayerInput directly or rely on the Controller to call Cancel
            
            if (_currentDuration <= 0) 
            {
                EndSkill(rb);
                return;
            }
            
            rb.gravityScale = 0;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        }

        public void EndSkill(Rigidbody2D rb)
        {
            if (!_isActive) return;
            if (rb != null) 
            {
                rb.gravityScale = _originalGravityScale;
            }
            _isActive = false;
            Debug.Log(name + " Hover ENDED.");
        }

        public void CancelSkill() => EndSkill(_characterRb);

        public void Recharge()
        {
            if (!_isActive) _hasCharge = true;
        }
    }
}
