using System;
using System.Collections.Generic;
using UnityEngine;
using Flyfe.Gameplay;

namespace Flyfe.Recording
{
    public class ShadowReplay : MonoBehaviour
    {
        public static event Action OnReplayFinished;

        private List<RecordedFrame> _frames;
        private int _index = 0;
        private bool _active = false;
        private Rigidbody2D _rb;

        // Interpolation variables
        private Vector3 _startPos;
        private Vector3 _targetPos;
        private float _lerpTimer = 0f;

        public float ReplayProgress => (_frames != null && _frames.Count > 0) ? (float)_index / _frames.Count : 0f;

        public void Init(List<RecordedFrame> recordedFrames)
        {
            _frames = recordedFrames;
            _index = 0;
            _active = true;

            if (TryGetComponent<Rigidbody2D>(out _rb))
            {
                _rb.simulated = true; 
                _rb.bodyType = RigidbodyType2D.Kinematic;
                _rb.useFullKinematicContacts = true;
                // Professional Practice: Use Interpolate for smooth visuals during MovePosition
                _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            }

            _startPos = transform.position;
            _targetPos = transform.position;

            SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();
            foreach (var sprite in sprites)
            {
                sprite.color = new Color(0.6f, 0.6f, 0.6f, 1.0f);
            }
        }

        void Update()
        {
            if (!_active || _frames == null) return;

            // Visual Interpolation: Smoothly move between fixed physics frames
            _lerpTimer += Time.deltaTime / Time.fixedDeltaTime;
            transform.position = Vector3.Lerp(_startPos, _targetPos, _lerpTimer);
        }

        void FixedUpdate()
        {
            if (!_active || _frames == null) return;

            if (_index < _frames.Count)
            {
                RecordedFrame currentFrame = _frames[_index];
                
                // Update interpolation targets
                _startPos = transform.position;
                _targetPos = currentFrame.position;
                _lerpTimer = 0f;

                if (_rb != null) _rb.MovePosition(_targetPos);

                if (currentFrame.interacted)
                {
                    PerformInteraction();
                }

                _index++;
            }
            else
            {
                _active = false;
                OnReplayFinished?.Invoke();
                Destroy(gameObject); 
            }
        }

        private void PerformInteraction()
        {
            float interactRadius = 2.0f;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactRadius);
            
            IInteractable closestInteractable = null;
            float minDistance = float.MaxValue;

            foreach (var collider in colliders)
            {
                if (collider.TryGetComponent<IInteractable>(out var interactable))
                {
                    float distance = Vector2.Distance(transform.position, collider.transform.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestInteractable = interactable;
                    }
                }
            }

            if (closestInteractable != null)
            {
                closestInteractable.Interact(gameObject);
            }
        }
    }
}
