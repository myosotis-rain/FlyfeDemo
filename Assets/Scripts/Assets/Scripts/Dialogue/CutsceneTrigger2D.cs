using UnityEngine;

public enum CutsceneTriggerMode
{
    Manual,     // Player must press E
    Proximity   // Starts automatically when close
}

public class CutsceneTrigger2D : MonoBehaviour, IInteractable
{
    [SerializeField] private CutsceneController targetCutscene;
    [SerializeField] private MemorySwitch linkedSwitch;
    [SerializeField] private CutsceneTriggerMode triggerMode = CutsceneTriggerMode.Proximity;
    [SerializeField] private float autoTriggerRadius = 3f;
    [SerializeField] private bool persistAfterDeath = true;
    [SerializeField] private bool destroyAfterUse = true;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string interactPrompt = "Examine";

    [Header("Positioning")]
    [SerializeField] private bool snapToCenter = true;
    [SerializeField] private float snapSpeed = 5f;

    private static System.Collections.Generic.HashSet<string> _triggeredCutscenes = new System.Collections.Generic.HashSet<string>();
    private bool _hasTriggered = false;
    private Transform _playerTransform;

    private void Awake()
    {
        string id = name + transform.position.ToString();
        if (persistAfterDeath && _triggeredCutscenes.Contains(id))
        {
            _hasTriggered = true;
        }
    }

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
        if (targetCutscene != null && !_hasTriggered)
        {
            _hasTriggered = true;

            if (persistAfterDeath)
            {
                string id = name + transform.position.ToString();
                _triggeredCutscenes.Add(id);
            }

            StartCoroutine(PreparePlayerAndStart());
        }
    }

    private System.Collections.IEnumerator PreparePlayerAndStart()
    {
        // 1. Lock player immediately
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null && player.TryGetComponent<PlayerInputController>(out var input))
        {
            input.SetInputLocked(true);
        }

        // 2. Turn on the switch and wait for animation
        if (linkedSwitch != null)
        {
            linkedSwitch.TurnOn();
            yield return new WaitUntil(() => linkedSwitch.GetCurrentState() == MemorySwitch.SwitchState.On);
        }

        // 3. Optional snapping
        if (player != null && snapToCenter)
        {
            Vector3 targetPos = new Vector3(transform.position.x, player.transform.position.y, player.transform.position.z);
            float timeout = 0.5f;
            float elapsed = 0;

            while (Vector2.Distance(new Vector2(player.transform.position.x, 0), new Vector2(targetPos.x, 0)) > 0.1f && elapsed < timeout)
            {
                player.transform.position = Vector3.MoveTowards(player.transform.position, targetPos, snapSpeed * Time.deltaTime);
                elapsed += Time.deltaTime;
                yield return null;
            }
            player.transform.position = targetPos;
        }

        // 4. Finally start the cutscene
        if (targetCutscene != null)
        {
            targetCutscene.StartCutscene();
            
            // Wait for the entire cutscene sequence to finish
            yield return new WaitUntil(() => !targetCutscene.IsActive);

            // 5. Turn off the switch only after we return to gameplay
            if (linkedSwitch != null)
            {
                linkedSwitch.TurnOff();
            }
        }

        if (destroyAfterUse)
        {
            if (TryGetComponent<Collider2D>(out var col)) col.enabled = false;
        }
    }
}
