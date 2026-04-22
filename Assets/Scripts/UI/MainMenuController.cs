using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Flyfe.Gameplay;

namespace Flyfe.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Scene Configuration")]
        [SerializeField] private string firstLevelName = "Flyfe_Level_1";

        public void PlayGame()
        {
            // Use LevelController if it exists for a smooth fade
            if (LevelController.Instance != null)
            {
                LevelController.Instance.LoadNextLevel(firstLevelName);
            }
            else
            {
                // Fallback to direct loading
                SceneManager.LoadScene(firstLevelName);
            }
        }

        public void QuitGame()
        {
            Debug.Log("Quitting Game...");
            Application.Quit();
        }
    }
}
