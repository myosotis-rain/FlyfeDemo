using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerInputController : MonoBehaviour
{
    private PlayerController _playerController;
    // private PlayerInputActions _inputActions; // Removed: No longer needed for SetCallbacks
    private Vector2 _moveInput;

    void Awake()
    {
        _playerController = GetComponent<PlayerController>();
    }

    private bool _inputLocked = false;

    public void SetInputLocked(bool locked)
    {
        _inputLocked = locked;
        if (locked) _moveInput = Vector2.zero;
    }

    void Update()
    {
        // Force stop input if dialogue is open or input is locked or cutscene is active
        if (_inputLocked || (DialogueUI.Instance != null && DialogueUI.Instance.IsOpen) || CutsceneController.AnyCutsceneActive)
        {
            _moveInput = Vector2.zero;
        }
    }

    void FixedUpdate()
    {
        // The _moveInput is updated by OnMove, and we apply it here in the physics loop
        PlayerController activeController = GetActiveController();
        if (activeController != null)
        {
            activeController.Move(_moveInput);
        }
    }

    private PlayerController GetActiveController()
    {
        RecordingService recordingService = RecordingService.Instance;
        if (recordingService != null && recordingService.IsRecordingShadow)
        {
            if (recordingService.ActiveShadowRb != null)
            {
                // Assuming the active shadow also has a PlayerController attached
                return recordingService.ActiveShadowRb.GetComponent<PlayerController>();
            }
        }
        return _playerController;
    }

    // --- These methods are now called by the Unity Events on the PlayerInput component ---

    public void OnUseSkill(InputAction.CallbackContext context)
    {
        // Only allow skill usage if we are currently in the recording/memory phase.
        if (RecordingService.Instance == null || !RecordingService.Instance.IsRecordingShadow)
        {
            return;
        }

        if (CutsceneController.AnyCutsceneActive || (DialogueUI.Instance != null && DialogueUI.Instance.IsOpen))
        {
            return;
        }

        PlayerController activeController = GetActiveController();
        if (activeController != null)
        {
            if (context.performed) // Skill is activated on a single press
            {
                activeController.StartSkill();
            }
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Allow Space (Jump action) to advance dialogue or cutscene
            if (DialogueUI.Instance != null && DialogueUI.Instance.IsOpen)
            {
                DialogueUI.Instance.AdvanceDialogue();
                return;
            }

            if (CutsceneController.AnyCutsceneActive)
            {
                CutsceneController[] controllers = FindObjectsByType<CutsceneController>(FindObjectsSortMode.None);
                foreach (var c in controllers) if (c.IsActive) c.AdvanceCutscene();
            }
        }

        if (CutsceneController.AnyCutsceneActive || (DialogueUI.Instance != null && DialogueUI.Instance.IsOpen)) return;

        PlayerController activeController = GetActiveController();
        if (activeController != null)
        {
            if (context.performed)
            {
                activeController.Jump();
            }
        }
    }

    public void OnRecord(InputAction.CallbackContext context)
    {
        if (CutsceneController.AnyCutsceneActive || (DialogueUI.Instance != null && DialogueUI.Instance.IsOpen)) return;

        if (context.performed && RecordingService.Instance != null)
        {
            RecordingService.Instance.ToggleRecord();
        }
    }

    public void OnReplay(InputAction.CallbackContext context)
    {
        if (CutsceneController.AnyCutsceneActive || (DialogueUI.Instance != null && DialogueUI.Instance.IsOpen)) return;

        if (context.performed && RecordingService.Instance != null)
        {
            if (RecordingService.Instance.IsRecordingShadow)
            {
                RecordingService.Instance.EndRecording();
            }
            RecordingService.Instance.PlayLatestRecording();
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // If we are currently in a conversation, advance the dialogue instead of interacting with the world
            if (DialogueUI.Instance != null && DialogueUI.Instance.IsOpen)
            {
                DialogueUI.Instance.AdvanceDialogue();
                return;
            }

            if (CutsceneController.AnyCutsceneActive)
            {
                CutsceneController[] controllers = FindObjectsByType<CutsceneController>(FindObjectsSortMode.None);
                foreach (var c in controllers) if (c.IsActive) c.AdvanceCutscene();
                return;
            }

            PlayerController activeController = GetActiveController();
            if (activeController != null)
            {
                // Search for interactables near the active character
                float interactRadius = 1.5f;
                Collider2D[] colliders = Physics2D.OverlapCircleAll(activeController.transform.position, interactRadius);
                
                foreach (var collider in colliders)
                {
                    // Use GetComponents to find ALL interactable scripts on this object
                    var interactables = collider.GetComponents<IInteractable>();
                    if (interactables.Length > 0)
                    {
                        foreach (var interactable in interactables)
                        {
                            interactable.Interact(activeController.gameObject);
                        }
                        
                        // If we are currently recording, flag this interaction
                        if (RecordingService.Instance != null && RecordingService.Instance.IsRecordingShadow)
                        {
                            RecordingService.Instance.FlagInteraction();
                        }

                        Debug.Log("Interacted with: " + collider.name + " (" + interactables.Length + " components)");
                        break; // Move to the next collider if needed, but usually we stop at the first object
                    }
                }
            }
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (CutsceneController.AnyCutsceneActive || (DialogueUI.Instance != null && DialogueUI.Instance.IsOpen))
        {
            _moveInput = Vector2.zero; // Stop movement while talking
            return;
        }

        _moveInput = context.ReadValue<Vector2>();

        // Check for early skill cancel if 'S' or 'Down Arrow' is pressed
        if (context.performed && _moveInput.y < 0) // Detect pressing 'S' or 'Down Arrow'
        {
            PlayerController activeController = GetActiveController();
            if (activeController != null && activeController.IsSkillActive)
            {
                activeController.CancelSkill();
            }
        }
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Allow clicking to advance dialogue or cutscene
            if (DialogueUI.Instance != null && DialogueUI.Instance.IsOpen)
            {
                DialogueUI.Instance.AdvanceDialogue();
                return;
            }

            if (CutsceneController.AnyCutsceneActive)
            {
                CutsceneController[] controllers = FindObjectsByType<CutsceneController>(FindObjectsSortMode.None);
                foreach (var c in controllers) if (c.IsActive) c.AdvanceCutscene();
            }
        }
    }

    public void OnCycleSkill(InputAction.CallbackContext context)
    {
        if (CutsceneController.AnyCutsceneActive || (DialogueUI.Instance != null && DialogueUI.Instance.IsOpen)) return;

        if (context.performed)
        {
            float direction = context.ReadValue<float>();
            // If it's a key press like 'Q', value is 1. If it's scroll wheel, it's 120 or -120.
            if (_playerController != null)
            {
                SkillManager sm = _playerController.GetComponent<SkillManager>();
                if (sm != null)
                {
                    sm.CycleSkills(direction);
                }
            }
        }
    }
}
