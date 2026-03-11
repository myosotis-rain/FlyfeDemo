using UnityEngine;
using Flyfe.Player;

namespace Flyfe.Camera
{
    /// <summary>
    /// Professional Parallax Layer.
    /// Logic: Each layer captures the camera position when it first becomes active.
    /// This ensures perfect alignment regardless of when the layer is enabled (e.g. world swapping).
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class ParallaxLayer : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("0 = Sky, 1 = Gameplay Plane")]
        [SerializeField] private Vector2 parallaxFactor;
        [SerializeField] private bool lockVertical = false;

        private Transform _cameraTransform;
        private Vector3 _initialWorldPos;
        private Vector3 _initialCameraPos;
        private bool _isInitialized = false;

        private void Awake()
        {
            // Hierarchy Safety Check
            if (gameObject.CompareTag("Player") || name.Contains("Actors") || GetComponentInParent<PlayerController>(true) != null)
            {
                DestroyImmediate(this);
                return;
            }
        }

        private void OnEnable()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (UnityEngine.Camera.main == null) return;
            
            _cameraTransform = UnityEngine.Camera.main.transform;
            _initialWorldPos = transform.position;
            _initialCameraPos = _cameraTransform.position;
            _isInitialized = true;
        }

        void LateUpdate()
        {
            if (!_isInitialized || _cameraTransform == null) return;

            // Calculate how much the camera has moved since this layer was initialized
            Vector3 cameraDelta = _cameraTransform.position - _initialCameraPos;

            float offsetX = cameraDelta.x * (1 - parallaxFactor.x);
            float offsetY = lockVertical ? 0 : cameraDelta.y * (1 - parallaxFactor.y);

            // Apply movement relative to the position we had when we woke up
            transform.position = _initialWorldPos + new Vector3(offsetX, offsetY, 0);
        }

        /// <summary>
        /// Called during teleports (respawns) to prevent the background from drifting.
        /// </summary>
        public static void ResyncAll()
        {
            if (CameraManager.Instance == null) return;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            // 1. SNAP the camera first so the layers capture the correct destination
            CameraManager.Instance.InitializeCamera(player.transform);

            // 2. Refresh all layers
            ParallaxLayer[] layers = FindObjectsByType<ParallaxLayer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var layer in layers)
            {
                layer.Initialize();
            }
        }
    }
}
