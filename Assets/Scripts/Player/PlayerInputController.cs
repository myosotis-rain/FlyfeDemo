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

    void Update()
    {
        // The _moveInput is updated by OnMove, so we apply it here
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
        if (context.performed && RecordingService.Instance != null)
        {
            RecordingService.Instance.ToggleRecord();
        }
    }

    public void OnReplay(InputAction.CallbackContext context)
    {
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
            PlayerController activeController = GetActiveController();
            if (activeController != null)
            {
                // Search for interactables near the active character
                float interactRadius = 1.5f;
                Collider2D[] colliders = Physics2D.OverlapCircleAll(activeController.transform.position, interactRadius);
                
                foreach (var collider in colliders)
                {
                    if (collider.TryGetComponent<IInteractable>(out var interactable))
                    {
                        interactable.Interact(activeController.gameObject);
                        
                        // If we are currently recording, flag this interaction so the replay ghost can do it too.
                        if (RecordingService.Instance != null && RecordingService.Instance.IsRecordingShadow)
                        {
                            RecordingService.Instance.FlagInteraction();
                        }

                        Debug.Log("Interacted with: " + collider.name);
                        break; // Interact with the first one found
                    }
                }
            }
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
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

    public void OnPoint(InputAction.CallbackContext context)
    {
        // Not used for character control; this is for UI interaction.
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        // Not used for character control; this is for UI interaction.
    }

    public void OnCycleSkill(InputAction.CallbackContext context)
    {
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
