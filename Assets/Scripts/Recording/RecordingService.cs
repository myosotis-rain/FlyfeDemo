using System.Collections.Generic;
using UnityEngine;
using Flyfe.Core;
using Flyfe.Camera;
using Flyfe.Skills;
using Flyfe.Gameplay;
using Flyfe.Player;

namespace Flyfe.Recording
{
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
        public bool IsRecordingShadow => _isRecording;
        public ShadowReplay ActiveReplay { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player) _playerRb = player.GetComponent<Rigidbody2D>();
        }

        private void OnEnable() => ShadowReplay.OnReplayFinished += HandleReplayFinished;
        private void OnDisable() => ShadowReplay.OnReplayFinished -= HandleReplayFinished;

        private void Update()
        {
            if (_inputCooldown > 0) _inputCooldown -= Time.deltaTime;
        }

        private void FixedUpdate()
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

        public void StartRecording()
        {
            // Determine Shadow Prefab
            GameObject prefabToSpawn = defaultShadowPrefab;
            var psm = _playerRb.GetComponent<SkillManager>();
            if (psm != null && psm.ActiveSkill != null)
            {
                string skillName = psm.ActiveSkill.GetType().Name;
                foreach (var mapping in shadowMappings)
                {
                    if (mapping.skillName == skillName) 
                    { 
                        prefabToSpawn = mapping.prefab; 
                        break; 
                    }
                }
            }
            _recordedPrefab = prefabToSpawn;
            CleanupShadows(false); 

            _isRecording = true;
            _timer = 0f;
            _recordedFrames.Clear();
            _playerStartPosition = _playerRb.transform.position;

            // Darken Player
            _playerRb.simulated = false;
            foreach (var sprite in _playerRb.GetComponentsInChildren<SpriteRenderer>())
                sprite.color = new Color(0.6f, 0.6f, 0.6f, 1.0f);

            // Swap World & Reset State
            GameStateManager.Instance.SwapWorld(GameStateManager.WorldState.Memory);
            ResetWorldState();
            
            // Spawn Shadow
            _activeShadow = Instantiate(_recordedPrefab, _playerRb.transform.position, Quaternion.identity, actorRoot);
            _activeShadow.name = "ACTIVE_RECORDING_SHADOW";
            _activeShadow.tag = "Shadow";

            Physics2D.IgnoreCollision(_playerRb.GetComponent<Collider2D>(), _activeShadow.GetComponent<Collider2D>(), true);

            ActiveShadowRb = _activeShadow.GetComponent<Rigidbody2D>();

            // Sync Camera
            if (CameraManager.Instance != null)
                CameraManager.Instance.SetFollowTarget(_activeShadow.transform, true);
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

            GameStateManager.Instance.SwapWorld(GameStateManager.WorldState.Present);
            ResetWorldState();

            if (CameraManager.Instance != null)
                CameraManager.Instance.SetFollowTarget(_playerRb.transform, true);

            PlayLatestRecording();
        }

        public void PlayLatestRecording()
        {
            if (_recordedFrames == null || _recordedFrames.Count < 10 || _recordedPrefab == null) return;

            if (ActiveReplay != null) Destroy(ActiveReplay.gameObject);

            GameStateManager.Instance.SwapWorld(GameStateManager.WorldState.Replay);
            ResetWorldState();

            var presentWorld = GameStateManager.Instance.presentWorldFolder;
            if (presentWorld != null)
            {
                GameObject ghost = Instantiate(_recordedPrefab, _recordedFrames[0].position, Quaternion.identity, presentWorld.transform);
                ghost.name = "REPLAY_GHOST";
                ghost.tag = "Shadow";

                Physics2D.IgnoreCollision(_playerRb.GetComponent<Collider2D>(), ghost.GetComponent<Collider2D>(), true);

                var replay = ghost.GetComponent<ShadowReplay>();
                if (replay != null)
                {
                    replay.Init(new List<RecordedFrame>(_recordedFrames));
                    ActiveReplay = replay;

                    if (CameraManager.Instance != null)
                        CameraManager.Instance.SetFollowTarget(_playerRb.transform, true);
                }
            }
        }

        private void ResetWorldState()
        {
            var resettables = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var r in resettables)
            {
                if (r is IResettable resettable) resettable.ResetState();
            }
        }

        private void CleanupShadows(bool replayOnly = false)
        {
            if (ActiveReplay != null) Destroy(ActiveReplay.gameObject);
            if (!replayOnly && _activeShadow != null) Destroy(_activeShadow);
        }

        public float GetProgress() => Mathf.Clamp01(_timer / MaxRecordTime);

        private void HandleReplayFinished() 
        { 
            if (CameraManager.Instance != null && _playerRb != null)
                CameraManager.Instance.SetFollowTarget(_playerRb.transform, false);
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
}
