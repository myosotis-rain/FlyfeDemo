using System.Collections.Generic;
using UnityEngine;

public class ShadowReplay : MonoBehaviour
{
    private List<RecordManager.FrameData> _frames;
    private int _currentFrame = 0;
    private bool _isReplaying = false;

    public void Init(List<RecordManager.FrameData> frames)
    {
        _frames = frames;
        _currentFrame = 0;
        _isReplaying = true;
    }

    void FixedUpdate()
    {
        if (!_isReplaying || _frames == null || _currentFrame >= _frames.Count)
        {
            _isReplaying = false;
            Destroy(gameObject); // auto destroy when done
            return;
        }

        RecordManager.FrameData data = _frames[_currentFrame];
        transform.position = data.position; // world position only
        _currentFrame++;
    }
}
