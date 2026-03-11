using UnityEngine;
using Flyfe.Player;

namespace Flyfe.Camera
{
    [DefaultExecutionOrder(100)]
    public class ParallaxLayer : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("0 = Sky, 1 = Gameplay Plane")]
        [SerializeField] private Vector2 parallaxFactor;
        [SerializeField] private bool lockVertical = false;

        private Transform _cameraTransform;
        private Vector3 _authoredWorldPos;
        private bool _isInitialized = false;

        private void Awake()
        {
            if (gameObject.CompareTag("Player") || name.Contains("Actors") || GetComponentInParent<PlayerController>(true) != null)
            {
                DestroyImmediate(this);
                return;
            }

            // Capture where this object belongs in the world globally
            _authoredWorldPos = transform.position;
        }

        private void Start()
        {
            if (UnityEngine.Camera.main != null) _cameraTransform = UnityEngine.Camera.main.transform;
            _isInitialized = true;
        }

        void LateUpdate()
        {
            if (!_isInitialized || _cameraTransform == null || CameraManager.Instance == null || !CameraManager.Instance.ParallaxAnchor.HasValue)
                return;

            // GLOBAL CONSISTENCY: Always calculate displacement from the very start of the level
            Vector3 anchor = CameraManager.Instance.ParallaxAnchor.Value;
            Vector3 cameraDisplacement = _cameraTransform.position - anchor;

            float offsetX = cameraDisplacement.x * (1 - parallaxFactor.x);
            float offsetY = lockVertical ? 0 : cameraDisplacement.y * (1 - parallaxFactor.y);

            // Move relative to the world position you gave it in the editor
            transform.position = _authoredWorldPos + new Vector3(offsetX, offsetY, 0);
        }

        /// <summary>
        /// Simple camera snap for respawns.
        /// </summary>
        public static void ResyncAll()
        {
            if (CameraManager.Instance == null) return;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            // Just snap the camera. The backgrounds will follow naturally in LateUpdate.
            CameraManager.Instance.InitializeCamera(player.transform);
        }
    }
}
