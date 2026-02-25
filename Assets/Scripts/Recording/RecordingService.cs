using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine; // Updated for Unity 6+ Cinemachine

public class RecordingService : MonoBehaviour
{
    public static RecordingService Instance { get; private set; }

    [Header("Prefabs & Roots")]
    [SerializeField] private GameObject shadowPrefab;
    [SerializeField] private Transform actorRoot;

    [Header("Recording Settings")]
    [SerializeField] private float maxRecordTime = 6f;
    public float MaxRecordTime => maxRecordTime; // Public getter for other scripts to access

    [Header("Cinemachine")]
    [SerializeField] private CinemachineCamera cinemachineCamera; // Corrected to CinemachineCamera for Unity 6+

    private GameObject _activeShadow;
    private Rigidbody2D _playerRb;
    private List<Vector3> _recordedFrames = new List<Vector3>();
    private Vector3 _playerStartPosition;
    private float _timer;
    private bool _isRecording = false;

    public Rigidbody2D ActiveShadowRb { get; private set; }
    public Transform ActiveShadowFeet { get; private set; }
    public bool IsRecordingShadow => _isRecording;
    public ShadowReplay ActiveReplay { get; private set; } // Tracks the currently active replay ghost

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        var player = GameObject.FindGameObjectWithTag(Tags.Player);
        if (player) _playerRb = player.GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        ShadowReplay.OnReplayFinished += HandleReplayFinished;
    }

    void OnDisable()
    {
        ShadowReplay.OnReplayFinished -= HandleReplayFinished;
    }

    void FixedUpdate()
    {
        if (!_isRecording || ActiveShadowRb == null) return;
        _timer += Time.fixedDeltaTime;
        _recordedFrames.Add(ActiveShadowRb.position);
        if (_timer >= MaxRecordTime) EndRecording();
    }

    public void ToggleRecord()
    {
        if (!_isRecording) StartRecording();
        else EndRecording();
    }

    private void StartRecording()
    {
        if (!shadowPrefab || !_playerRb) return;
        
        // Clean up any previous ghosts or active replays
        CleanupShadows(false); 
        if (ActiveReplay != null)
        {
            Destroy(ActiveReplay.gameObject);
            ActiveReplay = null;
        }

        _isRecording = true;
        _timer = 0f;
        _recordedFrames.Clear();
        _playerStartPosition = _playerRb.transform.position;
        
        // Calculate Offset from platform manually
        Vector3 playerOffset = Vector3.zero;
        KinematicPlatform platformUnderPlayer = null;
        RaycastHit2D hit = Physics2D.Raycast(_playerRb.position, Vector2.down, 1.2f);
        if (hit.collider != null && hit.collider.TryGetComponent(out platformUnderPlayer))
        {
            playerOffset = _playerRb.transform.position - platformUnderPlayer.transform.position;
        }

        // Freeze Player and gray out
        _playerRb.simulated = false;
        if (_playerRb.TryGetComponent<SpriteRenderer>(out var playerSprite))
        {
            playerSprite.color = Color.gray; // Gray out the inactive player
        }

        // Reset Platforms
        KinematicPlatform[] platforms = FindObjectsByType<KinematicPlatform>(FindObjectsSortMode.None);
        foreach (var p in platforms) p.ResetState();
        
        // Snap Player to Platform Start
        Vector3 snapPos = _playerRb.transform.position;
        if (platformUnderPlayer != null)
        {
            snapPos = platformUnderPlayer.transform.position + playerOffset;
            _playerRb.transform.position = snapPos;
        }

        // Spawn Shadow (Parented to actorRoot to keep scale at 1,1,1)
        _activeShadow = Instantiate(shadowPrefab, snapPos, Quaternion.identity, actorRoot);
        _activeShadow.name = "ACTIVE_RECORDING_SHADOW";
        _activeShadow.tag = Tags.Shadow;

        ActiveShadowRb = _activeShadow.GetComponent<Rigidbody2D>();
        ActiveShadowFeet = _activeShadow.transform.Find("ShadowGroundCheck");

        GameStateManager.Instance.SwapWorld(GameStateManager.WorldState.Memory);

        // Cinemachine: Switch camera to follow the active shadow
        if (cinemachineCamera != null)
        {
            cinemachineCamera.Follow = _activeShadow.transform;
        }
    }

            public void EndRecording()
            {
                if (!_isRecording) return;
                _isRecording = false;
        
                // Restore player control and color
                _playerRb.simulated = true;
                _playerRb.transform.position = _playerStartPosition;
                if (_playerRb.TryGetComponent<SpriteRenderer>(out var playerSprite))
                {
                    playerSprite.color = Color.white;
                }
        
                // Clean up the active shadow immediately
                if (_activeShadow)
                {
                    Destroy(_activeShadow);
                    _activeShadow = null;
                }
                ActiveShadowRb = null;
                ActiveShadowFeet = null;
        
                // Reset platforms
                KinematicPlatform[] platforms = FindObjectsByType<KinematicPlatform>(FindObjectsSortMode.None);
                foreach (var p in platforms) p.ResetState();
        
                // Always return to the Present state when recording is finished.
                GameStateManager.Instance.SwapWorld(GameStateManager.WorldState.Present);
                
                // Switch camera back to the player
                if (cinemachineCamera != null)
                {
                    cinemachineCamera.Follow = _playerRb.transform;
                }
            }    
        public void PlayLatestRecording()
        {
            if (_recordedFrames == null || _recordedFrames.Count < 10) return;
    
            // If a replay is already active, destroy it before creating a new one.
            if (ActiveReplay != null)
            {
                Destroy(ActiveReplay.gameObject);
                ActiveReplay = null;
            }
    
            var presentWorld = GameStateManager.Instance.presentWorldFolder;
            if (presentWorld != null)
            {
                GameObject ghost = Instantiate(shadowPrefab, _recordedFrames[0], Quaternion.identity, presentWorld.transform);
                ghost.name = "REPLAY_GHOST";
                ghost.tag = Tags.Shadow;
                var replayComponent = ghost.GetComponent<ShadowReplay>();
                if (replayComponent != null)
                {
                    replayComponent.Init(new List<Vector3>(_recordedFrames));
                    ActiveReplay = replayComponent;
                    GameStateManager.Instance.SwapWorld(GameStateManager.WorldState.Replay);
                }
            }
        }
    
        private void CleanupShadows(bool replayOnly = false)
        {
            // This is now a general cleanup. Let's use our direct references.
            if (ActiveReplay != null)
            {
                Destroy(ActiveReplay.gameObject);
                ActiveReplay = null;
            }
    
            if (!replayOnly)
            {
                if (_activeShadow != null)
                {
                    Destroy(_activeShadow);
                    _activeShadow = null;
                }
            }
        }
    public float GetProgress() => Mathf.Clamp01(_timer / MaxRecordTime);

    private void HandleReplayFinished()
    {
        ActiveReplay = null; // Clear the reference to the finished replay
    }

    public void ForceResetToPresent()
    {
        if (_isRecording) EndRecording(); // If recording, ending it will handle reset
        else
        {
            CleanupShadows();
            GameStateManager.Instance.SwapWorld(GameStateManager.WorldState.Present);
        }
    }
}