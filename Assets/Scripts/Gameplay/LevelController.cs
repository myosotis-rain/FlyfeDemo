using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Flyfe.Recording;
using Flyfe.UI;

namespace Flyfe.Gameplay
{
    /// <summary>
    /// The 'Brain' of the level management.
    /// Handles the technical side of scene transitions (Async loading, Fading).
    /// Typically resides on a global Manager object that persists across scenes.
    /// </summary>
    public class LevelController : MonoBehaviour
    {
        public static LevelController Instance { get; private set; }

        [Header("Transition Configuration")]
        [SerializeField] private float fadeDuration = 1.0f;
        [SerializeField] private float bufferDelay = 0.5f;

        private bool _isTransitioning = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                // LevelController is often a global manager that persists between scenes
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Public entry point for LevelExit scripts or other game events.
        /// </summary>
        public void LoadNextLevel(string sceneName)
        {
            if (_isTransitioning) return;
            StartCoroutine(TransitionSequence(sceneName));
        }

        private IEnumerator TransitionSequence(string sceneName)
        {
            _isTransitioning = true;

            // 1. Inform gameplay systems to clean up (Stop recording, reset states)
            if (RecordingService.Instance != null)
            {
                RecordingService.Instance.ForceResetToPresent();
            }

            // 2. Trigger the Fade Out via ScreenFader singleton
            if (ScreenFader.Instance != null)
            {
                yield return ScreenFader.Instance.FadeOutCoroutine(fadeDuration);
            }
            else
            {
                Debug.LogWarning("LevelController: No ScreenFader instance found. Transitioning instantly.");
            }

            // 3. Optional buffer to allow audio/visuals to settle in black
            yield return new WaitForSeconds(bufferDelay);

            // 4. Industry Standard: Asynchronous Scene Loading
            // This loads the next level in the background without freezing the game.
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

            // Wait until the scene is fully loaded
            while (!asyncLoad.isDone)
            {
                // This loop is where you'd update a loading progress bar
                // (e.g. loadingBar.value = asyncLoad.progress;)
                yield return null;
            }

            // 5. Fade In to the new scene
            // Note: Since ScreenFader is a singleton, it should survive or be recreated.
            // If it survives, we can fade back in.
            if (ScreenFader.Instance != null)
            {
                yield return ScreenFader.Instance.FadeInCoroutine(fadeDuration);
            }

            _isTransitioning = false;
        }
    }
}
