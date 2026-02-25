using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // Reference the MeterScript from the package instead of a raw Image
    [SerializeField] private MeterScript timerMeter; 
    [SerializeField] private Text btnText;

    void OnEnable() => GameStateManager.OnWorldChanged += UpdateUI;
    void OnDisable() => GameStateManager.OnWorldChanged -= UpdateUI;

    void Update()
    {
        if (GameStateManager.Instance.CurrentState == GameStateManager.WorldState.Memory && timerMeter != null)
        {
            // Use the package's SetHealth logic to update the radial fill and gradient
            // We use (1 - progress) because RecordingService likely counts up, 
            // and we want the bar to drain down.
            float remainingTime = (1f - RecordingService.Instance.GetProgress()) * 8f; // 8 matches the package max
            timerMeter.SetHealth(remainingTime);
        }
    }

    void UpdateUI(GameStateManager.WorldState state)
    {
        bool isMemory = state == GameStateManager.WorldState.Memory;
        if (timerMeter) timerMeter.gameObject.SetActive(isMemory);
        if (btnText) btnText.text = isMemory ? "SUMMON ECHO" : "COMMUNE"; // Using your world-setting names
    }

    public void OnClickRecord() => RecordingService.Instance.ToggleRecord();
}