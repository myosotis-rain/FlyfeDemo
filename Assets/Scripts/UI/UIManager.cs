using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Flyfe.Core;
using Flyfe.Recording;

namespace Flyfe.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private MeterScript timerMeter; 
        [SerializeField] private TextMeshProUGUI btnText;
        [SerializeField] private TextMeshProUGUI gemCountText;
        [SerializeField] private GameObject skillSelectionPanel;
        
        [Header("Dynamic Positioning")]
        [SerializeField] private Vector2 shadowFollowOffset; // An offset to position the meter above the shadow
        [SerializeField] private float referenceHeight = 1080f; // The height at which the offset looks perfect

        private RectTransform _meterRectTransform;
        private Vector2 _meterOriginalAnchoredPos;

        void Awake()
        {
            if (timerMeter != null)
            {
                _meterRectTransform = timerMeter.GetComponent<RectTransform>();
                _meterOriginalAnchoredPos = _meterRectTransform.anchoredPosition;
            }
        }

        void OnEnable() 
        {
            GameStateManager.OnWorldChanged += UpdateUI;
            GemManager.OnGemsChanged += UpdateGemDisplay;
        }

        void OnDisable() 
        {
            GameStateManager.OnWorldChanged -= UpdateUI;
            GemManager.OnGemsChanged -= UpdateGemDisplay;
        }

        void Start()
        {
            if (timerMeter != null && RecordingService.Instance != null)
            {
                timerMeter.SetMaxTime(RecordingService.Instance.MaxRecordTime);
            }

            // Force the panel to be hidden at the start of the game
            if (skillSelectionPanel != null)
            {
                skillSelectionPanel.SetActive(false);
            }
            
            // Initial UI State
            if (GameStateManager.Instance != null)
                UpdateUI(GameStateManager.Instance.CurrentState);

            if (GemManager.Instance != null)
                UpdateGemDisplay(GemManager.Instance.GetGemCount());
        }

        private void UpdateGemDisplay(int count)
        {
            if (gemCountText != null)
            {
                gemCountText.text = count.ToString("D2"); // Format as 01, 02, etc.
            }
        }

        void Update()
        {
            if (timerMeter == null || GameStateManager.Instance == null || RecordingService.Instance == null) return;

            var currentState = GameStateManager.Instance.CurrentState;
            
            if (currentState == GameStateManager.WorldState.Memory)
            {
                // --- Time Drain Logic (Recording) ---
                float progress = RecordingService.Instance.GetProgress();
                float remainingTime = (1f - progress) * RecordingService.Instance.MaxRecordTime;
                timerMeter.SetTime(remainingTime);

                // --- Dynamic Positioning Logic ---
                var activeShadow = RecordingService.Instance.ActiveShadowRb;
                if (activeShadow != null && UnityEngine.Camera.main != null)
                {
                    Vector2 screenPoint = UnityEngine.Camera.main.WorldToScreenPoint(activeShadow.position);
                    
                    // RESOLUTION FIX: Scale the offset based on current screen height
                    float scale = Screen.height / referenceHeight;
                    _meterRectTransform.position = (Vector3)screenPoint + (Vector3)(shadowFollowOffset * scale);
                }
            }
            else if (currentState == GameStateManager.WorldState.Replay)
            {
                // --- Time Drain Logic (Replaying) ---
                var activeReplay = RecordingService.Instance.ActiveReplay;
                if(activeReplay != null)
                {
                    // We also drain the bar during replay to keep the visual language consistent.
                    float progress = activeReplay.ReplayProgress;
                    float remainingTime = (1f - progress) * RecordingService.Instance.MaxRecordTime;
                    timerMeter.SetTime(remainingTime);
                }
            }
        }

        void UpdateUI(GameStateManager.WorldState state)
        {
            bool isMemory = state == GameStateManager.WorldState.Memory;
            bool isReplay = state == GameStateManager.WorldState.Replay;

            if (timerMeter != null)
            {
                if(isMemory)
                {
                    timerMeter.SetTime(RecordingService.Instance.MaxRecordTime);
                }
                else if (isReplay)
                {
                    _meterRectTransform.anchoredPosition = _meterOriginalAnchoredPos; // Reset to corner
                    timerMeter.SetTime(RecordingService.Instance.MaxRecordTime); // Start FULL to drain down
                }
                timerMeter.gameObject.SetActive(isMemory || isReplay);
            }

            if (btnText) btnText.text = isMemory ? "SUMMON ECHO" : "COMMUNE";
        }

        public void OnClickRecord()
        {
            if (RecordingService.Instance != null)
            {
                RecordingService.Instance.ToggleRecord();
            }
        }

        public void OpenSkillPanel()
        {
            if (skillSelectionPanel != null)
            {
                skillSelectionPanel.SetActive(!skillSelectionPanel.activeSelf);
            }
            else
            {
                Debug.LogWarning("UIManager: skillSelectionPanel is not assigned!");
            }
        }

        public void OnClickReplay()
        {
            if (RecordingService.Instance != null)
            {
                if (RecordingService.Instance.IsRecordingShadow)
                {
                    // If recording, stop the recording first, then play.
                    RecordingService.Instance.EndRecording();
                    RecordingService.Instance.PlayLatestRecording();
                }
                else
                {
                    // If not recording, just play.
                    RecordingService.Instance.PlayLatestRecording();
                }
            }
        }
    }
}
