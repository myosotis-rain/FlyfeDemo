using UnityEngine;
using Flyfe.Core;

namespace Flyfe.Gameplay
{
    /// <summary>
    /// The physical trigger the player touches to finish a level.
    /// Renamed from LevelGoal to LevelExit for better semantic clarity.
    /// This script identifies WHERE to go, while the LevelController handles HOW to get there.
    /// </summary>
    public class LevelExit : MonoBehaviour
    {
        [Header("Destination Settings")]
        [Tooltip("The name of the next scene to load.")]
        [SerializeField] private string nextSceneName;

        private bool _isActivated = false;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isActivated) return;

            // Only the real player can trigger the end-of-level exit
            if (other.CompareTag(Tags.Player))
            {
                TriggerExit();
            }
        }

        private void TriggerExit()
        {
            if (string.IsNullOrEmpty(nextSceneName))
            {
                Debug.LogWarning($"LevelExit on {gameObject.name} has no nextSceneName set!");
                return;
            }

            _isActivated = true;
            Debug.Log($"Player reached exit: {gameObject.name}. Triggering LevelController...");

            // Use the LevelController to handle the actual transition logic
            if (LevelController.Instance != null)
            {
                LevelController.Instance.LoadNextLevel(nextSceneName);
            }
            else
            {
                Debug.LogError("No LevelController found in scene! Loading scene directly as fallback.");
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
            }
        }
    }
}
