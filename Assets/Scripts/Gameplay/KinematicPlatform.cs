using UnityEngine;
using Flyfe.UI;
using Flyfe.Dialogue;
using Flyfe.Core;

namespace Flyfe.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class KinematicPlatform : MonoBehaviour
    {
        [SerializeField] private Vector3 travelOffset = new Vector3(5, 0, 0); 
        [SerializeField] private float transitSpeed = 2.5f;

        private Rigidbody2D _rb;
        private Vector3 _startPosition;
        private Vector3 _targetPosition;
        private float _timeOffset;
        private Vector3 _lastPosition;
        private BoxCollider2D _col;

        void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.useFullKinematicContacts = true;
            _rb.simulated = true;

            _col = GetComponent<BoxCollider2D>();
            _startPosition = transform.position;
            _targetPosition = _startPosition + travelOffset;
            _lastPosition = transform.position;
        }

        void FixedUpdate()
        {
            // Pause movement during dialogue or cutscenes
            if ((DialogueUI.Instance != null && DialogueUI.Instance.IsOpen) || CutsceneController.AnyCutsceneActive) return;
            
            float distance = travelOffset.magnitude;
            if (distance < 0.01f) return;

            float movementFactor = Mathf.PingPong((Time.time + _timeOffset) * transitSpeed / distance, 1f);
            Vector3 newPos = Vector3.Lerp(_startPosition, _targetPosition, movementFactor);
            
            Vector3 delta = newPos - _lastPosition;

            _rb.MovePosition(newPos);

            Vector2 boxSize = new Vector2(_col.size.x * 0.9f, 0.2f);
            Vector2 boxCenter = (Vector2)newPos + Vector2.up * (_col.size.y / 2 + 0.1f);
            
            RaycastHit2D[] hits = Physics2D.BoxCastAll(boxCenter, boxSize, 0f, Vector2.up, 0.1f);

            foreach (var hit in hits)
            {
                // Safety: If the player is already parented to us, don't manually move them (engine handles it)
                if (hit.collider.transform.IsChildOf(transform)) continue;

                if (hit.collider.CompareTag(Tags.Player) || hit.collider.CompareTag(Tags.Shadow))
                {
                    if (hit.collider.TryGetComponent<Rigidbody2D>(out var targetRb))
                    {
                        targetRb.position += (Vector2)delta;
                    }
                    else
                    {
                        hit.collider.transform.position += delta;
                    }
                }
            }

            _lastPosition = newPos;
        }

        public void ResetState() 
        {
            _timeOffset = -Time.time; 
            transform.position = _startPosition;
            _lastPosition = _startPosition;
        }
    }
}
