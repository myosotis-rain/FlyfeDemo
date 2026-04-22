using UnityEngine;
using TMPro;
using Flyfe.Core;
using System.Collections;

namespace Flyfe.UI
{
    /// <summary>
    /// A trigger-based tutorial system. 
    /// Shows instructions when entering a zone and can hide them after an action is performed.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class TutorialTrigger : MonoBehaviour
    {
        [Header("Display Settings")]
        [SerializeField] private string instructionMessage = "Press [A/D] to Move";
        [SerializeField] private float fadeDuration = 0.5f;

        [Header("Completion Settings")]
        [Tooltip("If true, the message disappears only after the player performs a specific action.")]
        [SerializeField] private bool requiresAction = false;
        [SerializeField] private KeyCode requiredKey = KeyCode.None;

        private CanvasGroup _canvasGroup;
        private TextMeshProUGUI _textComponent;
        private bool _isPlayerInside = false;
        private bool _isCompleted = false;

        private void Start()
        {
            // Find a dedicated Tutorial UI in the scene (I recommend a single Canvas with a Text object)
            GameObject uiObj = GameObject.Find("TutorialPopupUI");
            if (uiObj != null)
            {
                _canvasGroup = uiObj.GetComponent<CanvasGroup>();
                _textComponent = uiObj.GetComponentInChildren<TextMeshProUGUI>();
                
                if (_canvasGroup != null) _canvasGroup.alpha = 0;
            }
            
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void Update()
        {
            if (!_isPlayerInside || _isCompleted) return;

            // If the player performs the required action, hide and mark as done
            if (requiresAction && Input.GetKeyDown(requiredKey))
            {
                _isCompleted = true;
                StartCoroutine(FadeUI(0f));
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isCompleted) return;
            
            if (other.CompareTag(Tags.Player))
            {
                _isPlayerInside = true;
                if (_textComponent != null) _textComponent.text = instructionMessage;
                StartCoroutine(FadeUI(1f));
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag(Tags.Player))
            {
                _isPlayerInside = false;
                if (!requiresAction)
                {
                    StartCoroutine(FadeUI(0f));
                }
            }
        }

        private IEnumerator FadeUI(float targetAlpha)
        {
            if (_canvasGroup == null) yield break;

            float startAlpha = _canvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
                yield return null;
            }
            _canvasGroup.alpha = targetAlpha;
        }
    }
}
