using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

[System.Serializable]
public struct RecordedFrame
{
    public Vector3 position;
    public bool interacted;
}

[System.Serializable]
public struct ShadowMapping
{
    public string skillName;
    public GameObject prefab;
}

public class RecordingService : MonoBehaviour
{
    public static RecordingService Instance { get; private set; }

    [Header("Prefabs & Roots")]
    [SerializeField] private GameObject defaultShadowPrefab;
    [SerializeField] private List<ShadowMapping> shadowMappings;
    [SerializeField] private Transform actorRoot;

    [Header("Recording Settings")]
    [SerializeField] private float maxRecordTime = 6f;
    public float MaxRecordTime => maxRecordTime;

    [Header("Cinemachine")]
    [SerializeField] private CinemachineCamera cinemachineCamera;

    private GameObject _activeShadow;
    private Rigidbody2D _playerRb;
    private List<RecordedFrame> _recordedFrames = new List<RecordedFrame>();
    private GameObject _recordedPrefab;
    private Vector3 _playerStartPosition;
    private float _timer;
    private bool _isRecording = false;
    private bool _interactedThisFrame = false;
    private float _inputCooldown = 0f;

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

    void Update()
    {
        if (_inputCooldown > 0) _inputCooldown -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        if (!_isRecording || ActiveShadowRb == null) return;
        _timer += Time.fixedDeltaTime;

        _recordedFrames.Add(new RecordedFrame 
        { 
            position = ActiveShadowRb.position, 
            interacted = _interactedThisFrame 
        });
        _interactedThisFrame = false;

        if (_timer >= MaxRecordTime) EndRecording();
    }

    public void FlagInteraction() => _interactedThisFrame = true;

    public void ToggleRecord()
    {
        if (_inputCooldown > 0) return;

        if (!_isRecording) StartRecording();
        else EndRecording();
    }

    private void ResetWorldState()
    {
        // Find all KinematicPlatforms (even inactive ones in disabled folders)
        KinematicPlatform[] platforms = FindObjectsByType<KinematicPlatform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var p in platforms) p.ResetState();

        // Reset all IResettable interactables (Levers, Doors, Switches, Vines)
        // CRITICAL: We MUST include Inactive objects because the folders are being toggled off
        var resettables = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var r in resettables)
        {
            if (r is IResettable resettable) resettable.ResetState();
        }
    }

    public void StartRecording()
    {
        if (!_playerRb) return;

        // Determine Shadow Prefab
        GameObject prefabToSpawn = defaultShadowPrefab;
        var psm = _playerRb.GetComponent<SkillManager>();
        if (psm != null && psm.ActiveSkill != null)
        {
            string skillName = psm.ActiveSkill.GetType().Name;
            foreach (var mapping in shadowMappings)
            {
                if (mapping.skillName == skillName) { prefabToSpawn = mapping.prefab; break; }
            }
        }
        _recordedPrefab = prefabToSpawn;

        CleanupShadows(false); 

        _isRecording = true;
        _timer = 0f;
        _recordedFrames.Clear();
        _playerStartPosition = _playerRb.transform.position;

        // Platform Offset logic
        Vector3 playerOffset = Vector3.zero;
        KinematicPlatform platformUnderPlayer = null;
        RaycastHit2D hit = Physics2D.Raycast(_playerRb.position, Vector2.down, 1.2f);
        if (hit.collider != null && hit.collider.TryGetComponent(out platformUnderPlayer))
        {
            playerOffset = _playerRb.transform.position - platformUnderPlayer.transform.position;
        }

        // Darken Player
        _playerRb.simulated = false;
        foreach (var sprite in _playerRb.GetComponentsInChildren<SpriteRenderer>())
            sprite.color = new Color(0.6f, 0.6f, 0.6f, 1.0f);

        // SWAP FIRST so we can find the memory objects to reset them
        GameStateManager.Instance.SwapWorld(GameStateManager.WorldState.Memory);
        ResetWorldState();
        
        Vector3 snapPos = _playerRb.transform.position;
        if (platformUnderPlayer != null) snapPos = platformUnderPlayer.transform.position + playerOffset;

        // Spawn Shadow
        _activeShadow = Instantiate(_recordedPrefab, snapPos, Quaternion.identity, actorRoot);
        _activeShadow.name = "ACTIVE_RECORDING_SHADOW";
        _activeShadow.tag = Tags.Shadow;

        // Ignore collision with player to prevent jitter
        IgnoreCollisionWithPlayer(_activeShadow);

        ActiveShadowRb = _activeShadow.GetComponent<Rigidbody2D>();
        ActiveShadowFeet = _activeShadow.transform.Find("ShadowGroundCheck");

        // Sync Shadow
        var pc = _playerRb.GetComponent<PlayerController>();
        var sc = _activeShadow.GetComponent<PlayerController>();
        if (pc && sc) sc.SyncSettings(pc);

        var ssm = _activeShadow.GetComponent<SkillManager>();
        if (psm && ssm && psm.ActiveSkillType != null) ssm.SetActiveSkill(psm.ActiveSkillType);

        SyncCameraToActiveShadow();
    }

    public void SyncCameraToActiveShadow()
    {
        if (cinemachineCamera != null && _activeShadow != null)
        {
            // Simply tell Cinemachine to follow the new object and inform it we snapped
            cinemachineCamera.Follow = _activeShadow.transform;
            cinemachineCamera.OnTargetObjectWarped(_activeShadow.transform, Vector3.zero);
            
            ResyncConfiner();
        }
    }

    public void EndRecording()
    {
        if (!_isRecording) return;
        _isRecording = false;

        _playerRb.simulated = true;
        _playerRb.transform.position = _playerStartPosition;
        foreach (var sprite in _playerRb.GetComponentsInChildren<SpriteRenderer>()) sprite.color = Color.white;

        if (_activeShadow) Destroy(_activeShadow);
        ActiveShadowRb = null;

        // SWAP FIRST then RESET
        GameStateManager.Instance.SwapWorld(GameStateManager.WorldState.Present);
        ResetWorldState();

        if (cinemachineCamera != null && _playerRb != null)
        {
            cinemachineCamera.Follow = _playerRb.transform;
            cinemachineCamera.OnTargetObjectWarped(_playerRb.transform, Vector3.zero);
            ResyncConfiner();
        }

        PlayLatestRecording();
    }

    private void IgnoreCollisionWithPlayer(GameObject shadow)
    {
        if (_playerRb == null || shadow == null) return;
        
        Collider2D playerCol = _playerRb.GetComponent<Collider2D>();
        Collider2D shadowCol = shadow.GetComponent<Collider2D>();
        
        if (playerCol != null && shadowCol != null)
        {
            Physics2D.IgnoreCollision(playerCol, shadowCol, true);
        }
    }

    public void PlayLatestRecording()
    {
        if (_recordedFrames == null || _recordedFrames.Count < 10 || _recordedPrefab == null) return;

        if (ActiveReplay != null) Destroy(ActiveReplay.gameObject);

        // SWAP FIRST then RESET so the objects in the folder are reset correctly
        GameStateManager.Instance.SwapWorld(GameStateManager.WorldState.Replay);
        ResetWorldState();

        var presentWorld = GameStateManager.Instance.presentWorldFolder;
        if (presentWorld != null)
        {
            GameObject ghost = Instantiate(_recordedPrefab, _recordedFrames[0].position, Quaternion.identity, presentWorld.transform);
            ghost.name = "REPLAY_GHOST";
            ghost.tag = Tags.Shadow;

            // Ignore collision with player to prevent jitter
            IgnoreCollisionWithPlayer(ghost);

            var replay = ghost.GetComponent<ShadowReplay>();
            if (replay != null)
            {
                replay.Init(new List<RecordedFrame>(_recordedFrames));
                ActiveReplay = replay;

                if (cinemachineCamera != null && _playerRb != null)
                {
                    cinemachineCamera.OnTargetObjectWarped(_playerRb.transform, _playerRb.transform.position - cinemachineCamera.transform.position);
                    cinemachineCamera.Follow = _playerRb.transform;
                    ResyncConfiner();
                }
            }
        }
    }

    private void ResyncConfiner()
    {
        var confiner = cinemachineCamera.GetComponent<CinemachineConfiner2D>();
        if (confiner != null) confiner.InvalidateBoundingShapeCache();
        ParallaxLayer.ResyncAll();
    }

    private void CleanupShadows(bool replayOnly = false)
    {
        if (ActiveReplay != null) Destroy(ActiveReplay.gameObject);
        if (!replayOnly && _activeShadow != null) Destroy(_activeShadow);
    }

    public float GetProgress() => Mathf.Clamp01(_timer / MaxRecordTime);
    private void HandleReplayFinished() 
    { 
        if (cinemachineCamera != null && _playerRb != null) cinemachineCamera.Follow = _playerRb.transform; 
        _inputCooldown = 0.5f; 
    }

    public void ForceResetToPresent()
    {
        if (_isRecording) EndRecording();
        else
        {
            CleanupShadows();
            GameStateManager.Instance.SwapWorld(GameStateManager.WorldState.Present);
            ResetWorldState();
        }
    }
}
