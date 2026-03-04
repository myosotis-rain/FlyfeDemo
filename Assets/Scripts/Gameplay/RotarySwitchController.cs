using UnityEngine;
using UnityEngine.Events; // 必须引入
using UnityEngine.InputSystem;

public class RotarySwitchController : MonoBehaviour, IInteractable
{
    [Header("Hierarchy References")]
    [SerializeField] private Transform pivotTransform; 
    [SerializeField] private Transform backplate;

    [Header("Auto-Off Settings")]
    [SerializeField] private bool autoOff = true;
    [SerializeField] private float interactRange = 3.5f;
    [SerializeField] private float bufferTime = 1.0f; // 离开后多久自动关闭

    [Header("Animation Settings")]
    [SerializeField] private float targetAngle = -90f;
    [SerializeField] private float smoothSpeed = 10f;

    [Header("Events & Effects")]
    public UnityEvent onSwitchOn;
    public UnityEvent onSwitchOff;

    private bool _isOn = false;
    private Quaternion _startRot;
    private Quaternion _endRot;
    private float _outOfRangeTimer = 0f;

    void Awake()
    {
        if (pivotTransform != null)
        {
            _startRot = pivotTransform.localRotation;
            _endRot = Quaternion.Euler(0, 0, targetAngle);
        }
    }

    public void Interact(GameObject actor)
    {
        Toggle();
    }

    public string GetInteractPrompt()
    {
        return _isOn ? "Close" : "Open";
    }

    void Update()
    {
        if (pivotTransform == null) return;
        
        // Handle Auto-Off Logic
        if (autoOff && _isOn)
        {
            if (IsAnyPlayerInRange())
            {
                _outOfRangeTimer = 0f; // Reset timer if someone is nearby
            }
            else
            {
                _outOfRangeTimer += Time.deltaTime;
                if (_outOfRangeTimer >= bufferTime)
                {
                    Toggle(); // Auto-toggle off
                }
            }
        }

        Quaternion target = _isOn ? _endRot : _startRot;
        pivotTransform.localRotation = Quaternion.Slerp(pivotTransform.localRotation, target, Time.deltaTime * smoothSpeed);
    }

    private bool IsAnyPlayerInRange()
    {
        // Check for the main player
        GameObject player = GameObject.FindGameObjectWithTag(Tags.Player);
        if (player != null && Vector2.Distance(transform.position, player.transform.position) <= interactRange)
            return true;

        // Check for the recording shadow
        if (RecordingService.Instance != null && RecordingService.Instance.IsRecordingShadow)
        {
            if (RecordingService.Instance.ActiveShadowRb != null)
            {
                if (Vector2.Distance(transform.position, RecordingService.Instance.ActiveShadowRb.position) <= interactRange)
                    return true;
            }
        }

        // Check for the replay ghost
        GameObject ghost = GameObject.FindWithTag(Tags.Shadow); // Replay ghosts also have this tag
        if (ghost != null && Vector2.Distance(transform.position, ghost.transform.position) <= interactRange)
            return true;

        return false;
    }

    public void Toggle()
    {
        _isOn = !_isOn;
        _outOfRangeTimer = 0f; // Reset timer on manual toggle

        if (_isOn)
        {
            onSwitchOn.Invoke();
        }
        else
        {
            onSwitchOff.Invoke();
        }
    }
}