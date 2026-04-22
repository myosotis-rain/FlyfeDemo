using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using Flyfe.Gameplay;

namespace Flyfe.UI
{
    /// <summary>
    /// Professional Journal Controller.
    /// Uses the 'Modern Dashboard' approach and New Input System.
    /// </summary>
    public class JournalController : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject journalRoot;
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        [Header("Input (New Input System)")]
        [Tooltip("Map this to TAB or a Gamepad button in the Inspector.")]
        [SerializeField] private InputAction toggleJournalAction;

        private bool _isOpen = false;

        private void OnEnable()
        {
            toggleJournalAction.Enable();
            toggleJournalAction.performed += _ => ToggleJournal();
        }

        private void OnDisable()
        {
            toggleJournalAction.Disable();
            toggleJournalAction.performed -= _ => ToggleJournal();
        }

        private void Start()
        {
            if (journalRoot != null) journalRoot.SetActive(false);
        }

        public void ToggleJournal()
        {
            if (_isOpen) CloseJournal();
            else OpenJournal();
        }

        public void OpenJournal()
        {
            _isOpen = true;
            Time.timeScale = 0f; 
            if (journalRoot != null) journalRoot.SetActive(true);
            
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        public void CloseJournal()
        {
            _isOpen = false;
            Time.timeScale = 1f; 
            if (journalRoot != null) journalRoot.SetActive(false);
        }

        public void ReturnToMenu()
        {
            Time.timeScale = 1f;
            if (LevelController.Instance != null)
                LevelController.Instance.LoadNextLevel(mainMenuSceneName);
            else
                SceneManager.LoadScene(mainMenuSceneName);
        }

        public void QuitGame() => Application.Quit();
    }
}
