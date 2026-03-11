using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.Events;
using UnityEngine.UI;
using Flyfe.UI;
using Flyfe.Recording;
using Flyfe.Player;
using Flyfe.Core;

namespace Flyfe.Dialogue
{
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
        Manual,     
        Auto        
    }

    [System.Serializable]
    public class CutsceneStep
    {
        public CutsceneStepType type;
        public AdvanceMode advanceMode; 
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
        [SerializeField] private Image cgDisplayImage; 
        [SerializeField] private bool preserveAspect = true;
        [SerializeField] private bool playOnStart = false;

        private static int _activeCutscenesCount = 0;
        public static bool AnyCutsceneActive => _activeCutscenesCount > 0;

        private Transform _originalCameraFollow;
        private bool _isCutsceneActive = false;
        private bool _waitingForInput = false;
        private float _inputCooldown = 0f;

        public bool IsActive => _isCutsceneActive;

        public void AdvanceCutscene()
        {
            if (_isCutsceneActive && _inputCooldown <= 0)
            {
                _waitingForInput = false;
                _inputCooldown = 0.3f;
            }
        }

        private void Update()
        {
            if (_inputCooldown > 0) _inputCooldown -= Time.deltaTime;
        }

        private void Awake()
        {
            if (cgDisplayImage != null)
                cgDisplayImage.gameObject.SetActive(false);
        }

        private void Start()
        {
            if (playOnStart) StartCutscene();
        }

        public void StartCutscene()
        {
            if (_isCutsceneActive) return;
            _isCutsceneActive = true;
            _activeCutscenesCount++;
            StartCoroutine(PlaySequence());
        }

        private IEnumerator PlaySequence()
        {
            if (virtualCamera != null)
                _originalCameraFollow = virtualCamera.Follow;

            CanvasGroup cgGroup = null;
            if (cgDisplayImage != null)
            {
                cgDisplayImage.preserveAspect = preserveAspect;
                cgDisplayImage.raycastTarget = false; 
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
                        if (DialogueUI.Instance != null && DialogueUI.Instance.IsOpen) DialogueUI.Instance.ForceStop();
                        yield return ScreenFader.Instance.FadeOutCoroutine(step.duration);
                        continue; 

                    case CutsceneStepType.FadeIn:
                        if (DialogueUI.Instance != null && DialogueUI.Instance.IsOpen) DialogueUI.Instance.ForceStop();
                        yield return ScreenFader.Instance.FadeInCoroutine(step.duration);
                        continue; 

                    case CutsceneStepType.Wait:
                        if (step.advanceMode == AdvanceMode.Auto)
                        {
                            yield return new WaitForSeconds(step.duration);
                        }
                        break;

                    case CutsceneStepType.UnityEvent:
                        step.customEvent?.Invoke();
                        break;
                }

                if (step.advanceMode == AdvanceMode.Manual && step.type != CutsceneStepType.Dialogue)
                {
                    _waitingForInput = true;
                    yield return new WaitUntil(() => !_waitingForInput);
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
            if (!_isCutsceneActive) return;

            GameObject player = GameObject.FindGameObjectWithTag(Tags.Player);
            if (player != null && player.TryGetComponent<PlayerInputController>(out var input))
            {
                input.SetInputLocked(false);
            }

            if (virtualCamera != null)
            {
                bool isRecording = RecordingService.Instance != null && RecordingService.Instance.IsRecordingShadow;
                if (!isRecording)
                {
                    virtualCamera.Follow = _originalCameraFollow;
                }
            }

            _isCutsceneActive = false;
            _activeCutscenesCount--;
        }
    }
}
