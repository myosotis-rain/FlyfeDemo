using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Image timerBarImage; 
    [SerializeField] private Text btnText;

    void OnEnable() => GameStateManager.OnWorldChanged += UpdateUI;
    void OnDisable() => GameStateManager.OnWorldChanged -= UpdateUI;

    void Update()
    {
        if (GameStateManager.Instance.CurrentState == GameStateManager.WorldState.Memory && timerBarImage != null)
        {
            timerBarImage.fillAmount = 1f - RecordingService.Instance.GetProgress();
        }
    }

    void UpdateUI(GameStateManager.WorldState state)
    {
        bool isMemory = state == GameStateManager.WorldState.Memory;
        if (timerBarImage) timerBarImage.gameObject.SetActive(isMemory);
        if (btnText) btnText.text = isMemory ? "RETURN" : "RECORD";
    }

    public void OnClickRecord() => RecordingService.Instance.ToggleRecord();
}