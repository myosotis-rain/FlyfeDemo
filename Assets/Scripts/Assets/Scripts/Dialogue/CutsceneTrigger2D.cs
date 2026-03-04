using UnityEngine;

public enum CutsceneTriggerMode
{
    Manual,     // Player must press E
    Proximity   // Starts automatically when close
}

public class CutsceneTrigger2D : MonoBehaviour, IInteractable
{
    [SerializeField] private CutsceneController targetCutscene;
    [SerializeField] private CutsceneTriggerMode triggerMode = CutsceneTriggerMode.Proximity;
    [SerializeField] private float autoTriggerRadius = 3f; // Added for radius mode
    [SerializeField] private bool destroyAfterUse = true;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string interactPrompt = "Examine";

    private bool _hasTriggered = false;
    private Transform _playerTransform;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null) _playerTransform = player.transform;
    }

    void Update()
    {
        if (triggerMode == CutsceneTriggerMode.Proximity && !_hasTriggered)
        {
            if (_playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag(playerTag);
                if (player != null) _playerTransform = player.transform;
                return;
            }

            if (Vector2.Distance(transform.position, _playerTransform.position) <= autoTriggerRadius)
            {
                Trigger();
            }
        }
    }

    public void Interact(GameObject user)
    {
        if (triggerMode == CutsceneTriggerMode.Manual && !_hasTriggered)
        {
            Trigger();
        }
    }

    public string GetInteractPrompt()
    {
        return triggerMode == CutsceneTriggerMode.Manual ? interactPrompt : "";
    }

    private void Trigger()
    {
        if (targetCutscene != null)
        {
            _hasTriggered = true;
            targetCutscene.StartCutscene();

            if (destroyAfterUse)
            {
                if (TryGetComponent<Collider2D>(out var col)) col.enabled = false;
            }
        }
    }
}
