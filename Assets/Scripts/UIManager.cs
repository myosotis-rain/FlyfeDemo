using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image timerBarImage;
    [SerializeField] private Button recordButton;

    private TextMeshProUGUI buttonText;

    private void Awake()
    {
        if (recordButton != null)
            buttonText = recordButton.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        RecordManager.OnStateChanged += HandleVisualSwap;
    }

    private void OnDisable()
    {
        RecordManager.OnStateChanged -= HandleVisualSwap;
    }

    private void Update()
    {
        if (RecordManager.Instance != null && timerBarImage != null &&
            RecordManager.Instance.CurrentState == RecordManager.State.Memory)
        {
            timerBarImage.fillAmount = RecordManager.Instance.GetProgress();
        }
    }

    private void HandleVisualSwap(RecordManager.State newState)
    {
        if (buttonText != null)
            buttonText.text = newState == RecordManager.State.Memory ? "Stop" : "Record";
    }
}
