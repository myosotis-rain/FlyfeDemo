using UnityEngine;
using UnityEngine.Events;

public enum DialogueTriggerMode
{
    Manual,     // Player must press E
    Proximity   // Starts automatically when close
}

public class DialogueTrigger : MonoBehaviour, IInteractable
{
    [Header("Dialogue Content")]
    [SerializeField] private DialogueConversation conversation;
    [SerializeField] private string interactPrompt = "Talk";

    [Header("Trigger Settings")]
    [SerializeField] private DialogueTriggerMode triggerMode = DialogueTriggerMode.Manual;
    [SerializeField] private float autoTriggerRadius = 3f;

    [Header("Events")]
    [Tooltip("Fired when the conversation finishes. Useful for opening doors, giving items, or starting cutscenes.")]
    public UnityEvent onDialogueFinished;

    private bool _hasAutoTriggered = false;
    private Transform _playerTransform;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag(Tags.Player);
        if (player != null) _playerTransform = player.transform;
    }

    void Update()
    {
        if (triggerMode == DialogueTriggerMode.Proximity && !_hasAutoTriggered)
        {
            if (_playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag(Tags.Player);
                if (player != null) _playerTransform = player.transform;
                return;
            }

            if (Vector2.Distance(transform.position, _playerTransform.position) <= autoTriggerRadius)
            {
                if (DialogueUI.Instance != null && !DialogueUI.Instance.IsOpen)
                {
                    _hasAutoTriggered = true;
                    StartTalking();
                }
            }
        }
    }

    public void Interact(GameObject user)
    {
        if (triggerMode == DialogueTriggerMode.Manual)
        {
            StartTalking();
        }
    }

    private void StartTalking()
    {
        if (conversation == null) return;

        if (DialogueUI.Instance != null && !DialogueUI.Instance.IsOpen)
        {
            DialogueUI.Instance.StartConversation(conversation, () => {
                onDialogueFinished?.Invoke();
                
                // Optional: If you want proximity triggers to reset after a while, 
                // you could reset _hasAutoTriggered here. For now, it's one-shot.
            });
        }
    }

    public string GetInteractPrompt()
    {
        // Don't show a prompt if it's set to auto-trigger
        return triggerMode == DialogueTriggerMode.Manual ? interactPrompt : "";
    }
}
