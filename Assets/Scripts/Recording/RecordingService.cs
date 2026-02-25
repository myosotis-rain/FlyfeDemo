using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class RecordingService : MonoBehaviour
{
    public static RecordingService Instance { get; private set; }

    [Header("Prefabs & Roots")]
    [SerializeField] private GameObject shadowPrefab;
    [SerializeField] private Transform actorRoot;

    [Header("Recording Settings")]
    [SerializeField] private float maxRecordTime = 6f;
    public float MaxRecordTime => maxRecordTime;

    [Header("Cinemachine")]
    [SerializeField] private CinemachineCamera cinemachineCamera;

    private GameObject _activeShadow;
    private Rigidbody2D _playerRb;
    private List<Vector3> _recordedFrames = new List<Vector3>();
    private Vector3 _playerStartPosition;
    private float _timer;
    private bool _isRecording = false;

    public Rigidbody2D ActiveShadowRb { get; private set; }
    public Transform ActiveShadowFeet { get; private set; }
    public bool IsRecordingShadow => _isRecording;
    public ShadowReplay ActiveReplay { get; private set; }

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
        
        Vector3 playerOffset = Vector3.zero;
        KinematicPlatform platformUnderPlayer = null;
        RaycastHit2D hit = Physics2D.Raycast(_playerRb.position, Vector2.down, 1.2f);
        if (hit.collider != null && hit.collider.TryGetComponent(out platformUnderPlayer))
        {
            playerOffset = _playerRb.transform.position - platformUnderPlayer.transform.position;
        }

        _playerRb.simulated = false;

        KinematicPlatform[] platforms = FindObjectsByType<KinematicPlatform>(FindObjectsSortMode.None);
        foreach (var p in platforms) p.ResetState();
        
        Vector3 snapPos = _playerRb.transform.position;
        if (platformUnderPlayer != null)
        {
            snapPos = platformUnderPlayer.transform.position + playerOffset;
            _playerRb.transform.position = snapPos;
        }

        _activeShadow = Instantiate(shadowPrefab, snapPos, Quaternion.identity, actorRoot);
        _activeShadow.name = "ACTIVE_RECORDING_SHADOW";
        _activeShadow.tag = Tags.Shadow;

        ActiveShadowRb = _activeShadow.GetComponent<Rigidbody2D>();
        ActiveShadowFeet = _activeShadow.transform.Find("ShadowGroundCheck");

        GameStateManager.Instance.SwapWorld(GameStateManager.WorldState.Memory);

        if (cinemachineCamera != null)
        {
            cinemachineCamera.Follow = _activeShadow.transform;
        }
    }

    public void EndRecording()
    {
        if (!_isRecording) return;
        _isRecording = false;

        KinematicPlatform[] platforms = FindObjectsByType<KinematicPlatform>(FindObjectsSortMode.None);
        foreach (var p in platforms) p.ResetState();

        _playerRb.simulated = true;
        _playerRb.transform.position = _playerStartPosition;

        if (_recordedFrames.Count > 10)
        {
            PlayLatestRecording();
        }
        else
        {
             if (_activeShadow)
            {
                _activeShadow.SetActive(false);
                Destroy(_activeShadow);
            }
            ActiveShadowRb = null;
            ActiveShadowFeet = null;
            GameStateManager.Instance.SwapWorld(GameStateManager.WorldState.Present);
        }
        
        if (cinemachineCamera != null)
        {
            cinemachineCamera.Follow = _playerRb.transform;
        }
    }

    public void PlayLatestRecording()
    {
        if (_recordedFrames == null || _recordedFrames.Count < 10) return;

        CleanupShadows(true);

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
        var oldGhost = GameObject.Find("REPLAY_GHOST");
        if (oldGhost) Destroy(oldGhost);

        if (!replayOnly)
        {
            var oldActive = GameObject.Find("ACTIVE_RECORDING_SHADOW");
            if (oldActive) Destroy(oldActive);
        }
    }

    public float GetProgress() => Mathf.Clamp01(_timer / MaxRecordTime);

    private void HandleReplayFinished()
    {
        ActiveReplay = null;
    }

    public void ForceResetToPresent()
    {
        if (_isRecording) EndRecording();
        else
        {
            CleanupShadows();
            GameStateManager.Instance.SwapWorld(GameStateManager.WorldState.Present);
        }
    }
}
