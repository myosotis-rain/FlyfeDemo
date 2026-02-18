using System;
using System.Collections.Generic;
using UnityEngine;

public class ShadowReplay : MonoBehaviour
{
    private List<RecordManager.FrameData> _frames;
    private int _index;
    private bool _playing;

    private Action _onFinish;

    public void Init(List<RecordManager.FrameData> frames, Action onFinish = null)
    {
        _frames = frames;
        _index = 0;
        _playing = true;
        _onFinish = onFinish;
    }

    void FixedUpdate()
    {
        if (!_playing || _frames == null || _index >= _frames.Count)
        {
            _playing = false;
            _onFinish?.Invoke();
            Destroy(gameObject);
            return;
        }

        var data = _frames[_index];

        if (!string.IsNullOrEmpty(data.platformName))
        {
            GameObject plat = GameObject.Find(data.platformName);

            if (plat != null)
                transform.position = plat.transform.TransformPoint(data.position);
            else
                transform.position = data.position;
        }
        else
        {
            transform.position = data.position;
        }

        _index++;
    }
}
