using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public enum CutsceneStepType
{
    Dialogue,
    CameraFocus,
    FadeOut,
    FadeIn,
    Wait,
    UnityEvent,
    ShowCG,
    HideCG
}

public enum AdvanceMode
{
    Manual,     // Wait for player input (Press E)
    Auto        // Continue as soon as text finishes
}

[System.Serializable]
public class CutsceneStep
{
    public CutsceneStepType type;
    public AdvanceMode advanceMode; // New
    public DialogueConversation conversation;
    public Transform cameraTarget;
    public Sprite cgImage; 
    public float duration = 1.0f;
    public UnityEvent customEvent;
}

public class CutsceneController : MonoBehaviour
{
    [SerializeField] private List<CutsceneStep> steps;
    [SerializeField] private CinemachineCamera virtualCamera;
    
    [Header("CG Support")]
    [SerializeField] private Image cgDisplayImage; // The UI Image component to show the CG
    [SerializeField] private bool playOnStart = false;

    private Transform _originalCameraFollow;
    private bool _isCutsceneActive = false;

    void Awake()
    {
        if (cgDisplayImage != null)
            cgDisplayImage.gameObject.SetActive(false);
    }

    void Start()
    {
        if (playOnStart) StartCutscene();
    }

    public void StartCutscene()
    {
        if (_isCutsceneActive) return;
        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        _isCutsceneActive = true;
        
        if (virtualCamera != null)
            _originalCameraFollow = virtualCamera.Follow;

        // Ensure CG display is set up for fading
        CanvasGroup cgGroup = null;
        if (cgDisplayImage != null)
        {
            cgDisplayImage.raycastTarget = false; // Important: Never block dialogue clicks!
            cgGroup = cgDisplayImage.GetComponent<CanvasGroup>();
            if (cgGroup == null) cgGroup = cgDisplayImage.gameObject.AddComponent<CanvasGroup>();
            cgGroup.alpha = 0;
            cgDisplayImage.gameObject.SetActive(false);
        }

        foreach (var step in steps)
        {
            switch (step.type)
            {
                case CutsceneStepType.Dialogue:
                    // Show CG simultaneously if provided
                    if (cgDisplayImage != null && step.cgImage != null)
                    {
                        cgDisplayImage.sprite = step.cgImage;
                        cgDisplayImage.gameObject.SetActive(true);
                        StartCoroutine(FadeCanvasGroup(cgGroup, 1, 0.5f));
                    }

                    bool dialogueFinished = false;
                    bool isAuto = (step.advanceMode == AdvanceMode.Auto);
                    DialogueUI.Instance.StartConversation(step.conversation, () => dialogueFinished = true, isAuto);
                    yield return new WaitUntil(() => dialogueFinished);
                    break;

                case CutsceneStepType.CameraFocus:
                    if (virtualCamera != null && step.cameraTarget != null)
                        virtualCamera.Follow = step.cameraTarget;
                    // For camera focus, we don't wait if duration is 0
                    if (step.duration > 0) yield return new WaitForSeconds(step.duration);
                    break;

                case CutsceneStepType.ShowCG:
                    if (cgDisplayImage != null && step.cgImage != null)
                    {
                        cgDisplayImage.sprite = step.cgImage;
                        cgDisplayImage.gameObject.SetActive(true);
                        yield return StartCoroutine(FadeCanvasGroup(cgGroup, 1, step.duration));
                    }
                    break;

                case CutsceneStepType.HideCG:
                    if (cgGroup != null)
                        yield return StartCoroutine(FadeCanvasGroup(cgGroup, 0, step.duration));
                    if (cgDisplayImage != null) cgDisplayImage.gameObject.SetActive(false);
                    break;

                case CutsceneStepType.FadeOut:
                    yield return ScreenFader.Instance.FadeOutCoroutine(step.duration);
                    break;

                case CutsceneStepType.FadeIn:
                    yield return ScreenFader.Instance.FadeInCoroutine(step.duration);
                    break;

                case CutsceneStepType.Wait:
                    yield return new WaitForSeconds(step.duration);
                    break;

                case CutsceneStepType.UnityEvent:
                    step.customEvent?.Invoke();
                    break;
            }
        }

        EndCutscene();
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float targetAlpha, float duration)
    {
        if (group == null) yield break;
        float startAlpha = group.alpha;
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }
        group.alpha = targetAlpha;
    }

    private void EndCutscene()
    {
        // Only reset the camera if we are NOT currently recording a shadow.
        // If we ARE recording, the RecordingService should stay in control.
        if (virtualCamera != null)
        {
            bool isRecording = RecordingService.Instance != null && RecordingService.Instance.IsRecordingShadow;
            if (!isRecording)
            {
                virtualCamera.Follow = _originalCameraFollow;
            }
        }

        _isCutsceneActive = false;
    }
}
