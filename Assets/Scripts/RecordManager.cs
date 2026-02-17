using System;
using System.Collections.Generic;
using UnityEngine;

public class RecordManager : MonoBehaviour
{
    public static RecordManager Instance { get; private set; }
    public static event Action<State> OnStateChanged;

    public enum State { Present, Memory }
    public State CurrentState { get; private set; } = State.Present;

    [Header("Settings")]
    [SerializeField] private float maxRecordTime = 5.0f;
    [SerializeField] private GameObject shadowPrefab;

    private List<FrameData> _recordedFrames = new List<FrameData>();
    private float _timer;
    private Vector3 _startAnchor;

    [Serializable]
    public struct FrameData
    {
        public Vector3 position;
        public string platformName;
    }

    void Awake() => Instance = this;

    public void ToggleWorld()
    {
        if (CurrentState == State.Present) TransitionToMemory();
        else TransitionToPresent();

        OnStateChanged?.Invoke(CurrentState);
    }

    private void TransitionToMemory()
    {
        CurrentState = State.Memory;
        _timer = 0;
        _recordedFrames.Clear();

        var player = GameObject.FindGameObjectWithTag("Player");
        _startAnchor = player.transform.position;
    }

    private void TransitionToPresent()
    {
        CurrentState = State.Present;
        var player = GameObject.FindGameObjectWithTag("Player");
        player.transform.position = _startAnchor;

        SpawnShadow();
    }

    private void SpawnShadow()
    {
        if (_recordedFrames.Count == 0 || shadowPrefab == null) return;

        var shadow = Instantiate(shadowPrefab, _recordedFrames[0].position, Quaternion.identity);
        shadow.GetComponent<ShadowReplay>().Init(new List<FrameData>(_recordedFrames));
    }

    public void AddFrame(Vector3 pos, string platformName)
    {
        if (CurrentState != State.Memory) return;

        _timer += Time.fixedDeltaTime;
        if (_timer >= maxRecordTime)
        {
            ToggleWorld();
            return;
        }

        _recordedFrames.Add(new FrameData
        {
            position = pos,
            platformName = platformName
        });
    }

    public float GetProgress() => Mathf.Clamp01(_timer / maxRecordTime);
}
