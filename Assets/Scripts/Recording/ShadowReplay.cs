using System;
using System.Collections.Generic;
using UnityEngine;

public class ShadowReplay : MonoBehaviour
{
    public static event Action OnReplayFinished;

    private List<RecordedFrame> _frames;
    private int _index = 0;
    private bool _active = false;
    private SpriteRenderer _spriteRenderer;
    private Rigidbody2D _rb;

    public float ReplayProgress => (_frames != null && _frames.Count > 0) ? (float)_index / _frames.Count : 0f;

    public void Init(List<RecordedFrame> recordedFrames)
    {
        _frames = recordedFrames;
        _index = 0;
        _active = true;
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (TryGetComponent<Rigidbody2D>(out _rb))
        {
            _rb.simulated = true; 
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.useFullKinematicContacts = true;
        }

        // Darken all parts of the replay ghost
        SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();
        foreach (var sprite in sprites)
        {
            sprite.color = new Color(0.6f, 0.6f, 0.6f, 1.0f);
        }
    }

    void FixedUpdate()
    {
        if (!_active || _frames == null) return;

        if (_index < _frames.Count)
        {
            RecordedFrame currentFrame = _frames[_index];
            
            // Using MovePosition for smoothness (retained fix)
            if (_rb != null) _rb.MovePosition(currentFrame.position);
            else transform.position = currentFrame.position;

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
        // Retaining the improved "Closest Object" logic for accuracy
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
