using System;
using System.Collections.Generic;
using UnityEngine;

public class RecordManager : MonoBehaviour
{
    public static RecordManager Instance { get; private set; }

    public static event Action<State> OnStateChanged;

    public enum State
    {
        Present,
        Memory
    }

    public State CurrentState { get; private set; } = State.Present;

    [Header("Worlds")]
    public GameObject presentWorld;
    public GameObject memoryWorld;

    [Header("Recording")]
    public float maxRecordTime = 6f;
    public GameObject shadowPrefab;
    public GameObject playerAnchorPrefab;

    private float _timer;
    private Vector3 _anchorPosition;

    private List<FrameData> _frames = new List<FrameData>();

    private GameObject _activeShadow;
    private GameObject _activeAnchor;

    [Serializable]
    public struct FrameData
    {
        public Vector3 position;
        public string platformName;
    }

    void Awake()
    {
        Instance = this;
        SwapWorld(State.Present);
    }

    void FixedUpdate()
    {
        if (CurrentState != State.Memory) return;

        _timer += Time.fixedDeltaTime;

        if (_timer >= maxRecordTime)
            EndRecording();
    }

    // ---------- PUBLIC BUTTON CALL ----------
    public void ToggleRecord()
    {
        if (CurrentState == State.Present)
            StartRecording();
        else
            EndRecording();
    }

    // ---------- RECORD START ----------
    private void StartRecording()
    {
        var player = GameObject.FindGameObjectWithTag("Player");

        _frames.Clear();
        _timer = 0f;

        _anchorPosition = player.transform.position;

        if (_activeShadow != null)
            Destroy(_activeShadow);

        if (_activeAnchor != null)
            Destroy(_activeAnchor);

        if (playerAnchorPrefab != null)
            _activeAnchor = Instantiate(playerAnchorPrefab, _anchorPosition, Quaternion.identity);

        SwapWorld(State.Memory);
    }

    // ---------- RECORD END ----------
    private void EndRecording()
    {
        SwapWorld(State.Present);

        var player = GameObject.FindGameObjectWithTag("Player");
        player.transform.position = _anchorPosition;

        SpawnShadowReplay();
    }

    // ---------- WORLD SWAP ----------
    private void SwapWorld(State newState)
    {
        CurrentState = newState;

        if (presentWorld != null)
            presentWorld.SetActive(newState == State.Present);

        if (memoryWorld != null)
            memoryWorld.SetActive(newState == State.Memory);

        OnStateChanged?.Invoke(CurrentState);
    }

    // ---------- FRAME ADD ----------
    public void AddFrame(Vector3 worldPos, string platformName)
    {
        if (CurrentState != State.Memory) return;

        Vector3 finalPos = worldPos;

        if (!string.IsNullOrEmpty(platformName))
        {
            GameObject plat = GameObject.Find(platformName);
            if (plat != null)
                finalPos = plat.transform.InverseTransformPoint(worldPos);
        }

        _frames.Add(new FrameData
        {
            position = finalPos,
            platformName = platformName
        });
    }

    // ---------- SHADOW SPAWN ----------
    private void SpawnShadowReplay()
    {
        if (_frames.Count == 0 || shadowPrefab == null) return;

        _activeShadow = Instantiate(shadowPrefab, _frames[0].position, Quaternion.identity);

        ShadowReplay replay = _activeShadow.GetComponent<ShadowReplay>();
        replay.Init(new List<FrameData>(_frames));
    }


    // ---------- UI ----------
    public float GetProgress()
    {
        return Mathf.Clamp01(_timer / maxRecordTime);
    }

    // ---------- SAFE RESET ----------
    public void ForceResetToPresent()
    {
        SwapWorld(State.Present);

        if (_activeShadow != null)
            Destroy(_activeShadow);

        if (_activeAnchor != null)
            Destroy(_activeAnchor);
    }
}
