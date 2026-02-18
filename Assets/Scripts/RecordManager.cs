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

    public void ForceResetToPresent()
    {
        if (_isRecording) EndRecording();
        else {
            CleanupShadows();
            SwapWorld(WorldState.Present);
        }
    }

    private void StartRecording()
    {
        if (!shadowPrefab || !_playerRb) return;
        CleanupShadows();

        _isRecording = true;
        _timer = 0f;
        _recordedFrames.Clear();
        _anchorPos = _playerRb.transform.position;

        _playerRb.simulated = false;
        _playerRb.linearVelocity = Vector2.zero;

        _activeShadow = Instantiate(shadowPrefab, _playerRb.transform.position, Quaternion.identity, actorRoot);
        _activeShadow.name = "ACTIVE_RECORDING_SHADOW";
        _activeShadow.tag = "Shadow"; 
        _activeShadow.layer = LayerMask.NameToLayer("Shadow");
        
        ActiveShadowRb = _activeShadow.GetComponent<Rigidbody2D>();
        ActiveShadowFeet = _activeShadow.transform.Find("ShadowGroundCheck");

        if (_playerRb.transform.parent != null)
            _activeShadow.transform.SetParent(_playerRb.transform.parent);

        KinematicPlatform[] platforms = FindObjectsByType<KinematicPlatform>(FindObjectsSortMode.None);
        foreach (var p in platforms) p.ResetState();

        SwapWorld(WorldState.Memory);
    }

    private void EndRecording()
    {
        if (!_isRecording) return;
        _isRecording = false;

        // --- THE CRITICAL FIX: MANUAL DETACH BEFORE SWAP ---
        KinematicPlatform[] platforms = FindObjectsByType<KinematicPlatform>(FindObjectsSortMode.None);
        foreach (var p in platforms) 
        {
            p.ManualReleaseChildren(); // Force platforms to let go BEFORE deactivating
            p.ResetState();
        }

        // Double safety for the player and active shadow
        if (_playerRb.transform.parent != null) _playerRb.transform.SetParent(null);
        if (_activeShadow && _activeShadow.transform.parent != null) _activeShadow.transform.SetParent(null);

        _playerRb.simulated = true;
        _playerRb.transform.position = _anchorPos;

        if (_recordedFrames.Count > 10)
        {
            GameObject ghost = Instantiate(shadowPrefab, _recordedFrames[0], Quaternion.identity, presentWorldFolder.transform);
            ghost.name = "REPLAY_GHOST";
            ghost.tag = "Shadow";
            ghost.layer = LayerMask.NameToLayer("Shadow");
            ghost.GetComponent<ShadowReplay>()?.Init(new List<Vector3>(_recordedFrames));
        }

        if (_activeShadow) 
        {
            _activeShadow.SetActive(false); 
            Destroy(_activeShadow);
        }
        
        ActiveShadowRb = null;
        ActiveShadowFeet = null;
        SwapWorld(WorldState.Present);
        if (timerBarImage) timerBarImage.fillAmount = 0;
    }

    private void CleanupShadows()
    {
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