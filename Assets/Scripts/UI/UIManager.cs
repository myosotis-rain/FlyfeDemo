using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private MeterScript timerMeter; 
    [SerializeField] private Text btnText;
    
    [Header("Dynamic Positioning")]
    [SerializeField] private Vector2 shadowFollowOffset; // An offset to position the meter above the shadow (e.g., X: 0, Y: 80)

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

    void OnEnable() => GameStateManager.OnWorldChanged += UpdateUI;
    void OnDisable() => GameStateManager.OnWorldChanged -= UpdateUI;

    void Start()
    {
        if (timerMeter != null && RecordingService.Instance != null)
        {
            timerMeter.SetMaxTime(RecordingService.Instance.MaxRecordTime);
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
            if (activeShadow != null && Camera.main != null)
            {
                Vector2 screenPoint = Camera.main.WorldToScreenPoint(activeShadow.position);
                _meterRectTransform.position = screenPoint + shadowFollowOffset;
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

    public void OnClickRecord() => RecordingService.Instance.ToggleRecord();
}