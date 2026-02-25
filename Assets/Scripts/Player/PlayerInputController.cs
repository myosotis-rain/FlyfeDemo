using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerInputController : MonoBehaviour
{
    private PlayerController _playerController;
    private Vector2 _moveInput;

    void Awake()
    {
        // We only need to get the reference to the "muscles" now
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
                return recordingService.ActiveShadowRb.GetComponent<PlayerController>();
            }
        }
        return _playerController;
    }

    // --- These methods are now called by the Unity Events on the PlayerInput component ---

    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }



    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            PlayerController activeController = GetActiveController();
            if (activeController != null)
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
                // If we are recording, end the recording first...
                RecordingService.Instance.EndRecording();
                // ...and then immediately play the latest recording.
                RecordingService.Instance.PlayLatestRecording();
            }
            else
            {
                // If we are not recording, just play the latest recording.
                RecordingService.Instance.PlayLatestRecording();
            }
        }
    }

    public void OnUseSkill(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // TODO: Implement skill logic
            Debug.Log("UseSkill action was triggered");
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // TODO: Implement interact logic (e.g., for reading notes or flipping switches)
            Debug.Log("Interact action was triggered");
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
}
