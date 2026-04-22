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
        [Tooltip("Extra distance buffer to prevent flickering when exiting range.")]
        [SerializeField] private float exitHysteresis = 0.5f;
        [SerializeField] private bool alwaysShowOnRange = false; 
        [SerializeField] private bool onlyShowInMemoryWorld = false;
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
        private GameObject _shadowObj;
        private float _animTimer;
        private bool _shouldBeVisible = false;
        private bool _isCurrentlyInRange = false;
        private float _flickerPreventionTimer = 0f;
        private const float MIN_VISIBLE_TIME = 0.15f;

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

        private void FindShadow()
        {
            _shadowObj = GameObject.FindGameObjectWithTag("Shadow");
        }

        private void Update()
        {
            if (visualContainer == null) return;
            if (_flickerPreventionTimer > 0) _flickerPreventionTimer -= Time.deltaTime;

            // 1. Professional Practice: Hide prompts during cutscenes
            if (CutsceneController.AnyCutsceneActive)
            {
                if (visualContainer.activeSelf) HidePrompt();
                return;
            }

            // World State Check: Hide if we are restricted to Memory world but currently in Present/Replay
            if (onlyShowInMemoryWorld && GameStateManager.Instance != null)
            {
                if (GameStateManager.Instance.CurrentState != GameStateManager.WorldState.Memory)
                {
                    if (visualContainer.activeSelf || _currentAlpha > 0) 
                    {
                        _shouldBeVisible = false;
                    }
                    else return; 
                }
            }

            if (_playerObj == null) FindPlayer();
            // Re-find shadow if it's gone or dead
            if (_shadowObj == null || !_shadowObj.activeInHierarchy) FindShadow();

            // 2. Detection with Hysteresis
            float distToPlayer = (_playerObj != null && _playerObj.activeInHierarchy) ? Vector2.Distance(transform.position, _playerObj.transform.position) : float.MaxValue;
            float distToShadow = (_shadowObj != null && _shadowObj.activeInHierarchy) ? Vector2.Distance(transform.position, _shadowObj.transform.position) : float.MaxValue;
            
            float minDist = Mathf.Min(distToPlayer, distToShadow);

            float currentLimit = _isCurrentlyInRange ? (detectionRadius + exitHysteresis) : detectionRadius;
            bool nowInRange = (minDist <= currentLimit);

            string prompt = "";
            if (nowInRange)
            {
                foreach (var interactable in _interactables)
                {
                    if (interactable == null) continue;
                    string p = interactable.GetInteractPrompt();
                    if (!string.IsNullOrEmpty(p)) { prompt = p; break; }
                }
                
                if (string.IsNullOrEmpty(prompt) && instructionText != null) 
                {
                    if (alwaysShowOnRange || !string.IsNullOrEmpty(instructionText.text))
                        prompt = instructionText.text;
                }
            }

            bool wantVisible = nowInRange && !string.IsNullOrEmpty(prompt);

            // Flicker Prevention: If we just turned on, stay on for at least MIN_VISIBLE_TIME
            if (wantVisible && !_shouldBeVisible)
            {
                _flickerPreventionTimer = MIN_VISIBLE_TIME;
            }

            if (!wantVisible && _flickerPreventionTimer > 0)
            {
                wantVisible = true;
            }

            _shouldBeVisible = wantVisible;
            _isCurrentlyInRange = nowInRange;

            // Log if requested and in range
            if (showDebugLogs && _isCurrentlyInRange)
            {
                Debug.Log($"[{name}] Range: {minDist:F1}. Visible: {_shouldBeVisible}. Prompt: '{prompt}'");
            }

            // 3. Handle Visual State
            if (_shouldBeVisible)
            {
                if (!visualContainer.activeSelf) visualContainer.SetActive(true);
                
                _animTimer += Time.deltaTime;

                if (instructionText != null && instructionText.text != prompt)
                {
                    instructionText.text = prompt;
                }

                visualContainer.transform.rotation = Quaternion.identity;

                if (useBobbing)
                {
                    Vector3 bPos = _authoredLocalPos;
                    bPos.y += Mathf.Sin(_animTimer * bobSpeed) * bobAmount;
                    visualContainer.transform.localPosition = bPos;
                }

                if (useBreathing)
                {
                    float pulse = 1f + (Mathf.Sin(_animTimer * breatheSpeed) * breatheScale);
                    visualContainer.transform.localScale = _baseScale * pulse;
                }
            }

            // 4. Handle Fading
            float targetAlpha = _shouldBeVisible ? 1f : 0f;
            _currentAlpha = Mathf.MoveTowards(_currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);
            if (_canvasGroup != null) _canvasGroup.alpha = _currentAlpha;

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
