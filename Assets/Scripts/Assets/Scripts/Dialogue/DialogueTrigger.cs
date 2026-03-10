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
    [SerializeField] private bool persistAfterDeath = true;

    [Header("Events")]
    [Tooltip("Fired when the conversation finishes. Useful for opening doors, giving items, or starting cutscenes.")]
    public UnityEvent onDialogueFinished;

    private static System.Collections.Generic.HashSet<string> _triggeredDialogues = new System.Collections.Generic.HashSet<string>();
    private bool _hasAutoTriggered = false;
    private Transform _playerTransform;

    private void Awake()
    {
        string id = name + transform.position.ToString();
        if (persistAfterDeath && _triggeredDialogues.Contains(id))
        {
            _hasAutoTriggered = true;
        }
    }

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
            if (persistAfterDeath)
            {
                string id = name + transform.position.ToString();
                _triggeredDialogues.Add(id);
            }

            DialogueUI.Instance.StartConversation(conversation, () => {
                onDialogueFinished?.Invoke();
            });
        }
    }

    public string GetInteractPrompt()
    {
        // Don't show a prompt if it's set to auto-trigger
        return triggerMode == DialogueTriggerMode.Manual ? interactPrompt : "";
    }
}
