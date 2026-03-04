using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image portraitImage;
    [SerializeField] private GameObject continueIcon;
    [SerializeField] private Button fullScreenClickArea; // New: A large invisible button that covers the screen

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
    private Action _onEndCallback; // Directly track the trigger that started this
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

            // Ensure the continue icon doesn't block clicks
            if (continueIcon != null && continueIcon.TryGetComponent<Image>(out var img))
            {
                img.raycastTarget = false;
            }

            // Hook up the full-screen button to advance dialogue
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
        _onEndCallback = onComplete; // Save the callback
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
            // If currently typing, skip the effect and show all text immediately
            _cancelTyping = true;
        }
        else
        {
            // If done typing, move to the next line or end conversation
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

        var line = _currentConversation.lines[_currentLineIndex];
        
        // Update Name UI
        if (speakerNameText != null)
        {
            speakerNameText.text = line.speakerName;
            speakerNameText.gameObject.SetActive(!string.IsNullOrEmpty(line.speakerName));
        }

        // Update Portrait UI
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
        
        // Set text and FORCE TMP to parse it immediately to get character counts
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

            // Voice blip logic
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

        // If Auto-Advance is on, wait a short moment and then go to the next line
        if (_autoAdvance)
        {
            StartCoroutine(AutoAdvanceRoutine());
        }
    }

    private IEnumerator AutoAdvanceRoutine()
    {
        yield return new WaitForSeconds(autoAdvanceDelay); // Custom pause for reading
        if (IsOpen && !_isTyping) AdvanceDialogue();
    }

    private void EndConversation()
    {
        _currentConversation = null;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (fullScreenClickArea != null) fullScreenClickArea.gameObject.SetActive(false);
        
        OnDialogueEnded?.Invoke();
        _onEndCallback?.Invoke(); // Execute the specific callback for this session
        _onEndCallback = null;
    }
}
