using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// A professional-grade world-space instruction prompt.
/// Optimized for reliability and robust player detection.
/// </summary>
public class InstructionPromptUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The root folder/Canvas of your UI. This will be enabled/disabled.")]
    [SerializeField] private GameObject visualContainer; 
    [SerializeField] private TextMeshProUGUI instructionText; 
    
    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 3.5f; 
    [SerializeField] private bool alwaysShowOnRange = false; // If true, shows editor text even if no interactable is found
    [SerializeField] private bool showDebugLogs = false;
    
    [Header("Professional Effects")]
    [SerializeField] private float fadeSpeed = 4f;
    [SerializeField] private bool useBobbing = true; 
    [SerializeField] private float bobAmount = 0.04f; // More subtle bobbing
    [SerializeField] private bool useBreathing = true; 
    [SerializeField] private float breatheSpeed = 0.6f; // Slower, more natural
    [SerializeField] private float breatheScale = 0.005f; // Ultra-subtle 0.5% pulse

    private List<IInteractable> _interactables = new List<IInteractable>();
    private CanvasGroup _canvasGroup;
    private Vector3 _authoredLocalPos;
    private Vector3 _baseScale;
    private float _currentAlpha = 0f;
    private GameObject _playerObj;
    private float _animTimer;
    private bool _shouldBeVisible = false;

    private void Start()
    {
        // 1. Gather all interactables on this object and its children/parents
        _interactables.AddRange(GetComponents<IInteractable>());
        _interactables.AddRange(GetComponentsInChildren<IInteractable>());
        _interactables.AddRange(GetComponentsInParent<IInteractable>());

        if (visualContainer != null)
        {
            _canvasGroup = visualContainer.GetComponent<CanvasGroup>();
            if (_canvasGroup == null) _canvasGroup = visualContainer.AddComponent<CanvasGroup>();
            
            _authoredLocalPos = visualContainer.transform.localPosition;
            _baseScale = visualContainer.transform.localScale;
            if (_baseScale.sqrMagnitude < 0.00001f) _baseScale = Vector3.one;

            _canvasGroup.alpha = 0;
            visualContainer.SetActive(false); 
        }

        FindPlayer();
    }

    private void FindPlayer()
    {
        _playerObj = GameObject.FindGameObjectWithTag(Tags.Player);
    }

    private void Update()
    {
        // 1. Professional Practice: Hide all world-space prompts during cutscenes
        if (CutsceneController.AnyCutsceneActive)
        {
            _shouldBeVisible = false;
            // Immediate fade out
            _currentAlpha = Mathf.MoveTowards(_currentAlpha, 0f, Time.deltaTime * fadeSpeed * 2f);
            if (_canvasGroup != null) _canvasGroup.alpha = _currentAlpha;
            if (_currentAlpha <= 0.01f && visualContainer.activeSelf) visualContainer.SetActive(false);
            return;
        }

        // 2. Dynamic Detection Check
        if (_playerObj == null) FindPlayer();

        float dist = float.MaxValue;
        bool inRange = false;

        if (_playerObj != null && _playerObj.activeInHierarchy)
        {
            dist = Vector2.Distance(transform.position, _playerObj.transform.position);
            if (dist <= detectionRadius) inRange = true;
        }

        if (!inRange)
        {
            GameObject shadow = GameObject.FindWithTag(Tags.Shadow);
            if (shadow != null)
            {
                dist = Vector2.Distance(transform.position, shadow.transform.position);
                if (dist <= detectionRadius) inRange = true;
            }
        }

        // 2. Content Check
        string prompt = "";
        if (inRange)
        {
            foreach (var interactable in _interactables)
            {
                string p = interactable.GetInteractPrompt();
                if (!string.IsNullOrEmpty(p)) { prompt = p; break; }
            }
            
            // If no script has a prompt, but we have text in the editor AND alwaysShow is true
            if (string.IsNullOrEmpty(prompt) && instructionText != null) 
            {
                if (alwaysShowOnRange || !string.IsNullOrEmpty(instructionText.text))
                {
                    prompt = instructionText.text;
                }
            }
        }

        // 3. Logic for showing
        _shouldBeVisible = inRange && !string.IsNullOrEmpty(prompt);

        if (showDebugLogs && inRange)
            Debug.Log($"[{name}] Range: {dist:F1}. Prompt: '{prompt}'. Visible: {_shouldBeVisible}");

        // 4. Visibility State Handling
        if (_shouldBeVisible && !visualContainer.activeSelf)
        {
            visualContainer.SetActive(true);
            _animTimer = 0;
        }

        float targetAlpha = _shouldBeVisible ? 1f : 0f;
        _currentAlpha = Mathf.MoveTowards(_currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);
        if (_canvasGroup != null) _canvasGroup.alpha = _currentAlpha;

        if (_currentAlpha <= 0f && !_shouldBeVisible && visualContainer.activeSelf)
        {
            visualContainer.SetActive(false);
        }

        // 5. Visual Animation
        if (visualContainer.activeSelf)
        {
            _animTimer += Time.deltaTime;

            if (instructionText != null && !string.IsNullOrEmpty(prompt))
            {
                if (instructionText.text != prompt) instructionText.text = prompt;
            }

            visualContainer.transform.rotation = Quaternion.identity;

            if (useBobbing)
            {
                Vector3 bPos = _authoredLocalPos;
                bPos.y += Mathf.Sin(_animTimer * 2f) * bobAmount;
                visualContainer.transform.localPosition = bPos;
            }

            if (useBreathing)
            {
                float pulse = 1f + (Mathf.Sin(_animTimer * breatheSpeed) * breatheScale);
                visualContainer.transform.localScale = _baseScale * pulse;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
