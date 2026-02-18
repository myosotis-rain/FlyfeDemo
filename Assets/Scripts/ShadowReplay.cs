using System.Collections.Generic;
using UnityEngine;

public class ShadowReplay : MonoBehaviour
{
    private List<Vector3> _frames;
    private int _index = 0;
    private bool _active = false;

    public void Init(List<Vector3> recordedFrames)
    {
        _frames = recordedFrames;
        _active = true;

        // Ghosts should be Kinematic/Non-simulated to act as platforms or guides
        if (TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.simulated = false; // Prevents it from falling or reacting to physics
        }
        
        // If you want to JUMP on the ghost, keep the Collider enabled. 
        // If you want to walk THROUGH it, disable the Collider:
        // if (TryGetComponent<Collider2D>(out var col)) col.enabled = false;
    }

    void FixedUpdate()
    {
        if (!_active || _frames == null) return;

        if (_index < _frames.Count)
        {
            transform.position = _frames[_index];
            _index++;
        }
        else
        {
            // When the recording ends, the ghost stays at its last frame
            _active = false; 
        }
    }
}