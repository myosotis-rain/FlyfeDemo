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

    public float ReplayProgress => (_frames != null && _frames.Count > 0) ? (float)_index / _frames.Count : 0f;

    public void Init(List<RecordedFrame> recordedFrames)
    {
        _frames = recordedFrames;
        _index = 0;
        _active = true;
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.simulated = true; 
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.useFullKinematicContacts = true;
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
            transform.position = currentFrame.position;

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
        float interactRadius = 1.5f;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactRadius);
        
        foreach (var collider in colliders)
        {
            if (collider.TryGetComponent<IInteractable>(out var interactable))
            {
                interactable.Interact(gameObject);
                Debug.Log(name + " replayed interaction with: " + collider.name);
                break;
            }
        }
    }
}