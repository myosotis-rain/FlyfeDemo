using UnityEngine;
using System.Collections;

namespace Flyfe.Gameplay
{
    /// <summary>
    /// A temporary platform created by the Fairy Skill.
    /// Fades out and then destroys itself.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
    public class ManifestedPlatform : MonoBehaviour
    {
        [SerializeField] private float duration = 3f;
        [SerializeField] private float fadeTime = 1f;

        private SpriteRenderer _renderer;
        private Collider2D _collider;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<Collider2D>();
            
            // Ensure it's on the Ground layer so the player can walk on it
            gameObject.layer = LayerMask.NameToLayer("Ground");
        }

        private void Start()
        {
            StartCoroutine(LifetimeSequence());
        }

        private IEnumerator LifetimeSequence()
        {
            // 1. Wait for the solid duration
            yield return new WaitForSeconds(duration);

            // 2. Fade out
            float elapsed = 0f;
            Color startColor = _renderer.color;
            Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                _renderer.color = Color.Lerp(startColor, endColor, elapsed / fadeTime);
                
                // Optional: Make the platform feel "unstable" by flickering the collider near the end
                if (elapsed > fadeTime * 0.8f) _collider.enabled = !_collider.enabled;
                
                yield return null;
            }

            // 3. Goodbye
            Destroy(gameObject);
        }
    }
}
