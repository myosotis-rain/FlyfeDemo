using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecordManager : MonoBehaviour
{
    public static RecordManager Instance { get; private set; }
    public static event Action<WorldState> OnWorldChanged;

    public enum WorldState { Present, Memory }
    public WorldState CurrentState { get; private set; } = WorldState.Present;

    [Header("Worlds")]
    public GameObject presentWorldFolder;
    public GameObject memoryWorldFolder;

    [Header("Prefabs & Roots")]
    public GameObject shadowPrefab;
    public Transform actorRoot; 

    [Header("Recording Settings")]
    public float maxRecordTime = 6f;
    public Image timerBarImage;

    private GameObject _activeShadow;
    private Rigidbody2D _playerRb;
    private List<Vector3> _recordedFrames = new List<Vector3>();
    private Vector3 _anchorPos;
    private float _timer;
    private bool _isRecording = false;

    public Rigidbody2D ActiveShadowRb { get; private set; }
    public Transform ActiveShadowFeet { get; private set; }
    public bool IsRecordingShadow => _isRecording;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player) _playerRb = player.GetComponent<Rigidbody2D>();
        
        SwapWorld(WorldState.Present);
    }

    void FixedUpdate()
    {
        if (!_isRecording || ActiveShadowRb == null) return;

        _timer += Time.fixedDeltaTime;
        if (timerBarImage) timerBarImage.fillAmount = GetProgress();

        _recordedFrames.Add(ActiveShadowRb.position);

        if (_timer >= maxRecordTime) EndRecording();
    }

    public void ToggleRecord()
    {
        if (!_isRecording) StartRecording();
        else EndRecording();
    }

    public float GetProgress() => Mathf.Clamp01(_timer / maxRecordTime);

    public void ForceResetToPresent() => EndRecording();

    private void StartRecording()
    {
        if (!shadowPrefab || !_playerRb) return;

        // 1. CLEANUP DUPLICATES: Wipe the previous ghost and any active shadows
        CleanupShadows();

        _isRecording = true;
        _timer = 0f;
        _recordedFrames.Clear();
        _anchorPos = _playerRb.transform.position;

        // 2. FREEZE PLAYER
        _playerRb.simulated = false;
        _playerRb.linearVelocity = Vector2.zero;

        // 3. SPAWN ACTIVE SHADOW
        _activeShadow = Instantiate(shadowPrefab, _anchorPos, Quaternion.identity, actorRoot);
        _activeShadow.name = "ACTIVE_RECORDING_SHADOW";
        
        ActiveShadowRb = _activeShadow.GetComponent<Rigidbody2D>();
        ActiveShadowFeet = _activeShadow.transform.Find("ShadowGroundCheck");

        SwapWorld(WorldState.Memory);
    }

    private void EndRecording()
    {
        if (!_isRecording) return;
        _isRecording = false;

        // 1. RESTORE PLAYER
        _playerRb.simulated = true;
        _playerRb.transform.position = _anchorPos;

        // 2. SPAWN GHOST INTO PRESENT WORLD
        if (_recordedFrames.Count > 10)
        {
            // Parenting to presentWorldFolder keeps it visible after swap
            GameObject ghost = Instantiate(shadowPrefab, _anchorPos, Quaternion.identity, presentWorldFolder.transform);
            ghost.name = "REPLAY_GHOST";
            ghost.GetComponent<ShadowReplay>()?.Init(new List<Vector3>(_recordedFrames));
        }

        // 3. DESTROY ACTIVE RECORDING SHADOW
        if (_activeShadow) Destroy(_activeShadow);
        ActiveShadowRb = null;
        ActiveShadowFeet = null;
        
        SwapWorld(WorldState.Present);
        if (timerBarImage) timerBarImage.fillAmount = 0;
    }

    private void CleanupShadows()
    {
        // Find by name to ensure we only kill what we intend to
        GameObject oldGhost = GameObject.Find("REPLAY_GHOST");
        if (oldGhost) Destroy(oldGhost);

        GameObject oldActive = GameObject.Find("ACTIVE_RECORDING_SHADOW");
        if (oldActive) Destroy(oldActive);
    }

    private void SwapWorld(WorldState state)
    {
        CurrentState = state;
        if (presentWorldFolder) presentWorldFolder.SetActive(state == WorldState.Present);
        if (memoryWorldFolder) memoryWorldFolder.SetActive(state == WorldState.Memory);
        OnWorldChanged?.Invoke(CurrentState);
    }
}