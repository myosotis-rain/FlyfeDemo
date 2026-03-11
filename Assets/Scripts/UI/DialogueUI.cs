using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Flyfe.Dialogue;

namespace Flyfe.UI
{
    public class DialogueUI : MonoBehaviour
    {
        public static DialogueUI Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TMP_Text speakerNameText;
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private Image portraitImage;
        [SerializeField] private GameObject continueIcon;
        [SerializeField] private Button fullScreenClickArea; 

        [Header("Settings")]
        [SerializeField] private float defaultTypingSpeed = 0.03f;
        [SerializeField] private bool useTypewriterEffect = true;
        [SerializeField] private float autoAdvanceDelay = 1.5f;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private float pitchVariation = 0.1f;

        private DialogueConversation _currentConversation;
        private int _currentLineIndex = 0;
        private bool _isTyping = false;
        private bool _cancelTyping = false;
        private Coroutine _typingCoroutine;
        private Coroutine _autoAdvanceCoroutine;
        private Action _onEndCallback; 
        private bool _autoAdvance = false;

        public event Action OnDialogueStarted;
        public event Action OnDialogueEnded;

        public bool IsOpen => dialoguePanel != null && dialoguePanel.activeSelf;
        public bool IsTyping => _isTyping;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                
                if (dialoguePanel != null)
                {
                    Canvas canvas = dialoguePanel.GetComponent<Canvas>();
                    if (canvas == null) canvas = dialoguePanel.AddComponent<Canvas>();
                    canvas.overrideSorting = true;
                    canvas.sortingOrder = 100;
                    
                    if (dialoguePanel.GetComponent<GraphicRaycaster>() == null)
                        dialoguePanel.AddComponent<GraphicRaycaster>();
                }

                if (continueIcon != null && continueIcon.TryGetComponent<Image>(out var img))
                {
                    img.raycastTarget = false;
                }

                if (fullScreenClickArea != null)
                {
                    fullScreenClickArea.onClick.RemoveAllListeners();
                    fullScreenClickArea.onClick.AddListener(AdvanceDialogue);
                }
            }
            else
            {
                Destroy(gameObject);
            }

            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);

            if (fullScreenClickArea != null)
                fullScreenClickArea.gameObject.SetActive(false);
        }

        public void StartConversation(DialogueConversation conversation, Action onComplete = null, bool auto = false)
        {
            if (conversation == null || conversation.lines == null || conversation.lines.Length == 0) return;

            _currentConversation = conversation;
            _currentLineIndex = 0;
            _onEndCallback = onComplete; 
            _autoAdvance = auto;
            
            if (dialoguePanel != null)
                dialoguePanel.SetActive(true);

            if (fullScreenClickArea != null)
                fullScreenClickArea.gameObject.SetActive(true);

            OnDialogueStarted?.Invoke();
            DisplayLine();
        }

        public void AdvanceDialogue()
        {
            if (!IsOpen) return;

            if (_isTyping)
            {
                _cancelTyping = true;
            }
            else
            {
                if (_autoAdvanceCoroutine != null)
                {
                    StopCoroutine(_autoAdvanceCoroutine);
                    _autoAdvanceCoroutine = null;
                }

                _currentLineIndex++;
                if (_currentLineIndex < _currentConversation.lines.Length)
                {
                    DisplayLine();
                }
                else
                {
                    EndConversation();
                }
            }
        }

        private void DisplayLine()
        {
            if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
            if (_autoAdvanceCoroutine != null)
            {
                StopCoroutine(_autoAdvanceCoroutine);
                _autoAdvanceCoroutine = null;
            }

            var line = _currentConversation.lines[_currentLineIndex];
            
            if (speakerNameText != null)
            {
                speakerNameText.text = line.speakerName;
                speakerNameText.gameObject.SetActive(!string.IsNullOrEmpty(line.speakerName));
            }

            if (portraitImage != null)
            {
                portraitImage.sprite = line.portrait;
                portraitImage.gameObject.SetActive(line.portrait != null);
            }

            if (continueIcon != null) continueIcon.SetActive(false);

            if (useTypewriterEffect)
            {
                _typingCoroutine = StartCoroutine(TypeLine(line));
            }
            else
            {
                dialogueText.text = line.text;
                dialogueText.maxVisibleCharacters = 99999;
                FinishTyping();
            }
        }

        private IEnumerator TypeLine(DialogueLine line)
        {
            _isTyping = true;
            _cancelTyping = false;
            
            dialogueText.text = line.text;
            dialogueText.maxVisibleCharacters = 0;
            dialogueText.ForceMeshUpdate(); 

            int totalChars = dialogueText.textInfo.characterCount;
            float speed = line.customTypingSpeed > 0f ? line.customTypingSpeed : defaultTypingSpeed;

            for (int i = 0; i <= totalChars; i++)
            {
                if (_cancelTyping)
                {
                    dialogueText.maxVisibleCharacters = totalChars;
                    break;
                }

                dialogueText.maxVisibleCharacters = i;

                if (i % 2 == 0 && audioSource != null && line.voiceBlip != null)
                {
                    audioSource.pitch = 1f + UnityEngine.Random.Range(-pitchVariation, pitchVariation);
                    audioSource.PlayOneShot(line.voiceBlip);
                }

                if (speed > 0)
                    yield return new WaitForSeconds(speed);
            }

            FinishTyping();
        }

        private void FinishTyping()
        {
            _isTyping = false;
            _cancelTyping = false;
            dialogueText.maxVisibleCharacters = 99999; 

            if (continueIcon != null) continueIcon.SetActive(true);

            if (_autoAdvance)
            {
                if (_autoAdvanceCoroutine != null) StopCoroutine(_autoAdvanceCoroutine);
                _autoAdvanceCoroutine = StartCoroutine(AutoAdvanceRoutine());
            }
        }

        private IEnumerator AutoAdvanceRoutine()
        {
            yield return new WaitForSeconds(autoAdvanceDelay);
            _autoAdvanceCoroutine = null;
            if (IsOpen && !_isTyping) AdvanceDialogue();
        }

        public void ForceStop()
        {
            if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
            if (_autoAdvanceCoroutine != null) StopCoroutine(_autoAdvanceCoroutine);
            EndConversation();
        }

        private void EndConversation()
        {
            _currentConversation = null;
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            if (fullScreenClickArea != null) fullScreenClickArea.gameObject.SetActive(false);
            
            OnDialogueEnded?.Invoke();
            _onEndCallback?.Invoke(); 
            _onEndCallback = null;
        }
    }
}
