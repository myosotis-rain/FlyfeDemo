using UnityEngine;
using UnityEngine.InputSystem;
using Flyfe.UI;
using Flyfe.Dialogue;
using Flyfe.Recording;
using Flyfe.Gameplay;
using Flyfe.Skills;

namespace Flyfe.Player
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputController : MonoBehaviour
    {
        private PlayerController _playerController;
        private bool _inputLocked = false;

        void Awake()
        {
            _playerController = GetComponent<PlayerController>();
        }

        public void SetInputLocked(bool locked)
        {
            _inputLocked = locked;
        }

        private PlayerController GetActiveController()
        {
            if (RecordingService.Instance != null && RecordingService.Instance.IsRecordingShadow)
            {
                if (RecordingService.Instance.ActiveShadowRb != null)
                {
                    return RecordingService.Instance.ActiveShadowRb.GetComponent<PlayerController>();
                }
            }
            return _playerController;
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            if (_inputLocked || CutsceneController.AnyCutsceneActive) return;
            
            PlayerController activeController = GetActiveController();
            if (activeController != null)
            {
                activeController.Move(context.ReadValue<Vector2>());
            }
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
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

            if (_inputLocked || CutsceneController.AnyCutsceneActive || (DialogueUI.Instance != null && DialogueUI.Instance.IsOpen)) return;

            PlayerController activeController = GetActiveController();
            if (activeController != null && context.performed)
            {
                activeController.Jump();
            }
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
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

                if (_inputLocked) return;

                PlayerController activeController = GetActiveController();
                if (activeController != null)
                {
                    float interactRadius = 1.5f;
                    Collider2D[] colliders = Physics2D.OverlapCircleAll(activeController.transform.position, interactRadius);
                    
                    foreach (var collider in colliders)
                    {
                        var interactables = collider.GetComponents<IInteractable>();
                        if (interactables.Length > 0)
                        {
                            foreach (var interactable in interactables)
                            {
                                interactable.Interact(activeController.gameObject);
                            }
                            
                            if (RecordingService.Instance != null && RecordingService.Instance.IsRecordingShadow)
                            {
                                RecordingService.Instance.FlagInteraction();
                            }
                            break;
                        }
                    }
                }
            }
        }

        public void OnRecord(InputAction.CallbackContext context)
        {
            if (_inputLocked || CutsceneController.AnyCutsceneActive || (DialogueUI.Instance != null && DialogueUI.Instance.IsOpen)) return;

            if (context.performed && RecordingService.Instance != null)
            {
                RecordingService.Instance.ToggleRecord();
            }
        }

        public void OnReplay(InputAction.CallbackContext context)
        {
            if (_inputLocked || CutsceneController.AnyCutsceneActive || (DialogueUI.Instance != null && DialogueUI.Instance.IsOpen)) return;

            if (context.performed && RecordingService.Instance != null)
            {
                RecordingService.Instance.PlayLatestRecording();
            }
        }

        public void OnUseSkill(InputAction.CallbackContext context)
        {
            if (_inputLocked || CutsceneController.AnyCutsceneActive || (DialogueUI.Instance != null && DialogueUI.Instance.IsOpen)) return;

            PlayerController activeController = GetActiveController();
            if (activeController != null)
            {
                if (context.performed) activeController.StartSkill();
                else if (context.canceled) activeController.DeactivateSkill();
            }
        }

        public void OnCycleSkill(InputAction.CallbackContext context)
        {
            if (_inputLocked || CutsceneController.AnyCutsceneActive || (DialogueUI.Instance != null && DialogueUI.Instance.IsOpen)) return;

            if (context.performed)
            {
                var skillManager = GetComponent<SkillManager>();
                if (skillManager != null)
                {
                    skillManager.CycleSkills();
                }
            }
        }

        public void OnClick(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
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
    }
}
