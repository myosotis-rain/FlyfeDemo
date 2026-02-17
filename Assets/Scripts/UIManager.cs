using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Slider progressSlider;
    [SerializeField] private GameObject memoryOverlay;

    void OnEnable()
    {
        RecordManager.OnStateChanged += HandleVisualSwap;
    }

    void OnDisable()
    {
        RecordManager.OnStateChanged -= HandleVisualSwap;
    }

    void Update()
    {
        if (RecordManager.Instance.CurrentState == RecordManager.State.Memory)
        {
            progressSlider.value = RecordManager.Instance.GetProgress();
        }
    }

    private void HandleVisualSwap(RecordManager.State newState)
    {
        bool isMemory = (newState == RecordManager.State.Memory);
        memoryOverlay.SetActive(isMemory);
        progressSlider.gameObject.SetActive(isMemory);
    }
}
