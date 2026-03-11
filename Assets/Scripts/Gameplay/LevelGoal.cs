using UnityEngine;
using UnityEngine.SceneManagement;
using Flyfe.Recording;

namespace Flyfe.Gameplay
{
    public class LevelGoal : MonoBehaviour
    {
        [SerializeField] private string nextSceneName;
        [SerializeField] private float transitionDelay = 1.5f;

        private bool _isLevelComplete = false;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (_isLevelComplete) return;

            if (other.CompareTag("Player"))
            {
                CompleteLevel();
            }
        }

        private void CompleteLevel()
        {
            _isLevelComplete = true;
            Debug.Log("Level Complete!");

            if (RecordingService.Instance != null)
            {
                RecordingService.Instance.ForceResetToPresent();
            }

            if (!string.IsNullOrEmpty(nextSceneName))
            {
                Invoke(nameof(LoadNextScene), transitionDelay);
            }
        }

        private void LoadNextScene()
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
