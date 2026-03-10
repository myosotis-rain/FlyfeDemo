using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A specialized switch for the Memory World.
/// Uses Animator Parameters for stable state transitions.
/// </summary>
public class MemorySwitch : MonoBehaviour, IInteractable, IResettable
{
    public enum SwitchState { Off, TurningOn, On, TurningOff }

    [Header("Settings")]
    [SerializeField] private bool startsOn = false;
    [SerializeField] private bool allowManualTrigger = true;
    [SerializeField] private bool allowProximityTrigger = false;
    [SerializeField] private bool triggerByEventOnly = false; // If true, ignore internal E and proximity logic
    [SerializeField] private float proximityRadius = 2f;
    [SerializeField] private string interactPrompt = "Activate";

    [Header("Events")]
    public UnityEvent onActivated;
    public UnityEvent onDeactivated;

    private Animator _animator;
    private SwitchState _currentState;
    private bool _isPlayerInRange = false;

    // Animator Parameters
    private static readonly int IsOnHash = Animator.StringToHash("IsOn");
    private static readonly int TriggerHash = Animator.StringToHash("SwitchTrigger");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        ResetState();
    }

    public void ResetState()
    {
        _currentState = startsOn ? SwitchState.On : SwitchState.Off;
        
        if (_animator != null)
        {
            // Only set parameters if they exist to avoid console errors
            if (HasParameter("IsOn")) _animator.SetBool(IsOnHash, startsOn);
            
            // Snap the animator to the correct IDLE state immediately
            _animator.Play(startsOn ? "IdleOn" : "IdleOff", 0, 0f);
        }
    }

    private bool HasParameter(string paramName)
    {
        if (_animator == null) return false;
        foreach (AnimatorControllerParameter param in _animator.parameters)
        {
            if (param.name == paramName) return true;
        }
        return false;
    }

    private void Update()
    {
        if (triggerByEventOnly) return;

        if (allowProximityTrigger && _currentState == SwitchState.Off)
        {
            CheckProximity();
        }
    }

    private void CheckProximity()
    {
        GameObject player = GameObject.FindGameObjectWithTag(Tags.Player);
        if (player != null)
        {
            float dist = Vector2.Distance(transform.position, player.transform.position);
            if (dist <= proximityRadius && !_isPlayerInRange)
            {
                _isPlayerInRange = true;
                TurnOn();
            }
            else if (dist > proximityRadius)
            {
                _isPlayerInRange = false;
            }
        }
    }

    // --- IInteractable Implementation ---
    public void Interact(GameObject actor)
    {
        if (triggerByEventOnly || !allowManualTrigger) return;
        
        // Block interaction if we are already in the middle of a transition
        if (_currentState == SwitchState.TurningOn || _currentState == SwitchState.TurningOff) return;

        if (_currentState == SwitchState.Off) TurnOn();
        else if (_currentState == SwitchState.On) TurnOff();
    }

    public string GetInteractPrompt()
    {
        if (triggerByEventOnly) return "";
        if (_currentState == SwitchState.TurningOn || _currentState == SwitchState.TurningOff) return "...";
        return _currentState == SwitchState.Off ? interactPrompt : "Deactivate";
    }

    // --- Switch Logic ---

    public void TurnOn()
    {
        if (_currentState != SwitchState.Off) return;
        
        _currentState = SwitchState.TurningOn;
        if (HasParameter("IsOn")) _animator.SetBool(IsOnHash, true);
        if (HasParameter("SwitchTrigger")) _animator.SetTrigger(TriggerHash);
        else _animator.Play("TurnOn"); // Fallback
        
        onActivated.Invoke();
    }

    public void TurnOff()
    {
        if (_currentState != SwitchState.On) return;

        _currentState = SwitchState.TurningOff;
        if (HasParameter("IsOn")) _animator.SetBool(IsOnHash, false);
        if (HasParameter("SwitchTrigger")) _animator.SetTrigger(TriggerHash);
        else _animator.Play("TurnOff"); // Fallback
        
        onDeactivated.Invoke();
    }

    // --- Animation Events (Required for State Management) ---

    public void SetStateOn()
    {
        _currentState = SwitchState.On;
    }

    public void SetStateOff()
    {
        _currentState = SwitchState.Off;
    }

    public SwitchState GetCurrentState() => _currentState;
}
