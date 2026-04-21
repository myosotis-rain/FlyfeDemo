using UnityEngine;
using Flyfe.UI;
using Flyfe.Dialogue;
using Flyfe.Core;

namespace Flyfe.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class KinematicPlatform : MonoBehaviour, IResettable
    {
        [SerializeField] private Vector3 travelOffset = new Vector3(5, 0, 0); 
        [SerializeField] private float transitSpeed = 2.5f;
        [SerializeField] private float startDelay = 0f;

        private Rigidbody2D _rb;
        private Vector3 _startPosition;
        private Vector3 _targetPosition;
        private float _timeOffset;
        private float _startTime;

        void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.useFullKinematicContacts = true;
            _rb.simulated = true;

            _startPosition = transform.position;
            _targetPosition = _startPosition + travelOffset;
            _startTime = Time.time;
        }

        void FixedUpdate()
        {
            if ((DialogueUI.Instance != null && DialogueUI.Instance.IsOpen) || CutsceneController.AnyCutsceneActive) return;
            if (Time.time < _startTime + startDelay) return;
            
            float distance = travelOffset.magnitude;
            if (distance < 0.01f) return;

            float movementFactor = Mathf.PingPong((Time.time + _timeOffset) * transitSpeed / distance, 1f);
            Vector3 newPos = Vector3.Lerp(_startPosition, _targetPosition, movementFactor);
            
            _rb.MovePosition(newPos);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag(Tags.Player) || collision.gameObject.CompareTag(Tags.Shadow))
            {
                collision.transform.SetParent(transform);
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag(Tags.Player) || collision.gameObject.CompareTag(Tags.Shadow))
            {
                // Return to original root (usually null, but we could check for an 'Actors' root if needed)
                collision.transform.SetParent(null);
            }
        }

        public void ResetState() 
        {
            _startTime = Time.time;
            _timeOffset = -(Time.time + startDelay); 
            transform.position = _startPosition;
        }
    }
}
