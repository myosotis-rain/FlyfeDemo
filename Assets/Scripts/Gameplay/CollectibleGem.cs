using UnityEngine;
using Flyfe.Core;

namespace Flyfe.Gameplay
{
    /// <summary>
    /// A collectible gem that integrates with the Resettable system.
    /// Professional Practice: If collected during a Recording that is then cancelled, 
    /// it will reappear in the world.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class CollectibleGem : MonoBehaviour, IResettable
    {
        [Header("Visual Settings")]
        [SerializeField] private float floatSpeed = 2f;
        [SerializeField] private float floatAmplitude = 0.2f;
        [SerializeField] private GameObject collectEffectPrefab;

        private Vector3 _startPos;
        private bool _isCollected = false;
        private SpriteRenderer _renderer;
        private Collider2D _collider;

        private void Awake()
        {
            _startPos = transform.position;
            _renderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<Collider2D>();
            
            // Ensure it's a trigger
            _collider.isTrigger = true;
        }

        private void Update()
        {
            if (_isCollected) return;

            // Simple "juice" - floating effect
            float newY = _startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            transform.position = new Vector3(_startPos.x, newY, _startPos.z);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isCollected) return;

            // Professional: Can be collected by Player OR Shadow
            if (other.CompareTag(Tags.Player) || other.CompareTag(Tags.Shadow))
            {
                Collect();
            }
        }

        private void Collect()
        {
            _isCollected = true;
            _renderer.enabled = false;
            _collider.enabled = false;

            if (GemManager.Instance != null)
            {
                GemManager.Instance.AddGem();
            }

            if (collectEffectPrefab != null)
            {
                Instantiate(collectEffectPrefab, transform.position, Quaternion.identity);
            }
        }

        public void ResetState()
        {
            // If the world resets (e.g. recording ended/cancelled), the gem reappears
            // Note: If you want gems to be PERMANENT once collected, remove the GemManager 
            // logic from ResetState and only track it there.
            _isCollected = false;
            _renderer.enabled = true;
            _collider.enabled = true;
            transform.position = _startPos;
        }
    }
}
