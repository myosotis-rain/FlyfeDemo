using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Flyfe.Core;
using Flyfe.Recording;
using Flyfe.Dialogue;
using Flyfe.Gameplay;

namespace Flyfe.UI
{
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
        [SerializeField] private bool alwaysShowOnRange = false; 
        [SerializeField] private bool showDebugLogs = false;
        
        [Header("Professional Effects")]
        [SerializeField] private float fadeSpeed = 5f;
        [SerializeField] private bool useBobbing = false; 
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private float bobAmount = 0.1f;
        [SerializeField] private bool useBreathing = false; 
        [SerializeField] private float breatheSpeed = 1.5f; 
        [SerializeField] private float breatheScale = 0.05f;

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
            // Gather all interactables on this object and children/parents
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
            _playerObj = GameObject.FindGameObjectWithTag("Player");
        }

        private void Update()
        {
            // 1. Professional Practice: Hide prompts during cutscenes
            if (CutsceneController.AnyCutsceneActive)
            {
                if (visualContainer.activeSelf) HidePrompt();
                return;
            }

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
                GameObject shadow = GameObject.FindWithTag("Shadow");
                if (shadow != null)
                {
                    dist = Vector2.Distance(transform.position, shadow.transform.position);
                    if (dist <= detectionRadius) inRange = true;
                }
            }

            string prompt = "";
            if (inRange)
            {
                foreach (var interactable in _interactables)
                {
                    string p = interactable.GetInteractPrompt();
                    if (!string.IsNullOrEmpty(p)) { prompt = p; break; }
                }
                
                // Fallback to text in TMP component
                if (string.IsNullOrEmpty(prompt) && instructionText != null) 
                {
                    if (alwaysShowOnRange || !string.IsNullOrEmpty(instructionText.text))
                        prompt = instructionText.text;
                }
            }

            _shouldBeVisible = inRange && !string.IsNullOrEmpty(prompt);

            // Log if requested and in range
            if (showDebugLogs && inRange)
            {
                Debug.Log($"[{name}] Range: {dist:F1}. Visible: {_shouldBeVisible}. Prompt: '{prompt}'");
            }

            // 2. Handle Visual State
            if (_shouldBeVisible)
            {
                if (!visualContainer.activeSelf) visualContainer.SetActive(true);
                
                _animTimer += Time.deltaTime;

                // Sync text content
                if (instructionText != null && instructionText.text != prompt)
                {
                    instructionText.text = prompt;
                }

                // Billboard
                visualContainer.transform.rotation = Quaternion.identity;

                // Bobbing
                if (useBobbing)
                {
                    Vector3 bPos = _authoredLocalPos;
                    bPos.y += Mathf.Sin(_animTimer * bobSpeed) * bobAmount;
                    visualContainer.transform.localPosition = bPos;
                }

                // Breathing
                if (useBreathing)
                {
                    float pulse = 1f + (Mathf.Sin(_animTimer * breatheSpeed) * breatheScale);
                    visualContainer.transform.localScale = _baseScale * pulse;
                }
            }

            // 3. Handle Fading
            float targetAlpha = _shouldBeVisible ? 1f : 0f;
            _currentAlpha = Mathf.MoveTowards(_currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);
            if (_canvasGroup != null) _canvasGroup.alpha = _currentAlpha;

            // Deactivate when invisible
            if (_currentAlpha <= 0f && !_shouldBeVisible && visualContainer.activeSelf)
            {
                visualContainer.SetActive(false);
            }
        }

        private void HidePrompt()
        {
            _shouldBeVisible = false;
            _currentAlpha = 0;
            if (_canvasGroup != null) _canvasGroup.alpha = 0;
            visualContainer.SetActive(false);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
}
