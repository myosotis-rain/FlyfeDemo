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
        _index = 0;
        _active = true;

        if (TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.simulated = true; 
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.useFullKinematicContacts = true;
        }
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
            _active = false;
            Destroy(gameObject); 
        }
    }
}