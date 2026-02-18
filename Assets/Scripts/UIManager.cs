using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Image timerBarImage; 
    public Text btnText;

    void OnEnable() => RecordManager.OnWorldChanged += UpdateUI;
    void OnDisable() => RecordManager.OnWorldChanged -= UpdateUI;

    void Update()
    {
        if (RecordManager.Instance.CurrentState == RecordManager.WorldState.Memory && timerBarImage != null)
        {
            timerBarImage.fillAmount = 1f - RecordManager.Instance.GetProgress();
        }
    }

    void UpdateUI(RecordManager.WorldState state)
    {
        bool isMemory = state == RecordManager.WorldState.Memory;
        if (timerBarImage) timerBarImage.gameObject.SetActive(isMemory);
        if (btnText) btnText.text = isMemory ? "RETURN" : "RECORD";
    }

    public void OnClickRecord() => RecordManager.Instance.ToggleRecord();
}