using UnityEngine;
using Flyfe.Player;

namespace Flyfe.Camera
{
    /// <summary>
    /// Professional Parallax Layer (Local-Space Anchored).
    /// Logic: Uses the authored localPosition as the absolute 'Zero Point'.
    /// When synced, it captures the current camera position and calculates all future movement relative to that moment.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class ParallaxLayer : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("0 = Sky, 1 = Gameplay Plane")]
        [SerializeField] private Vector2 parallaxFactor;
        [SerializeField] private bool lockVertical = false;

        private Transform _cameraTransform;
        private Vector3 _anchorCameraPos;
        private Vector3 _authoredLocalPos;
        private bool _isInitialized = false;

        private void Awake()
        {
            // Hierarchy Safety Check
            if (gameObject.CompareTag("Player") || name.Contains("Actors") || GetComponentInParent<PlayerController>(true) != null)
            {
                DestroyImmediate(this);
                return;
            }

            // Capture the authored position immediately as our "Absolute Zero"
            _authoredLocalPos = transform.localPosition;
        }

        private void OnEnable()
        {
            InitializeAnchor();
        }

        public void InitializeAnchor()
        {
            if (UnityEngine.Camera.main == null) return;
            
            _cameraTransform = UnityEngine.Camera.main.transform;
            _anchorCameraPos = _cameraTransform.position;
            
            // Force reset to the authored position so we start fresh
            transform.localPosition = _authoredLocalPos;
            
            _isInitialized = true;
        }

        void LateUpdate()
        {
            if (!_isInitialized || _cameraTransform == null) return;

            // Calculate how much the camera has moved since the last synchronization
            Vector3 cameraDelta = _cameraTransform.position - _anchorCameraPos;

            float offsetX = cameraDelta.x * (1 - parallaxFactor.x);
            float offsetY = lockVertical ? 0 : cameraDelta.y * (1 - parallaxFactor.y);

            // Apply movement in LOCAL space relative to the authored Zero Point
            transform.localPosition = _authoredLocalPos + new Vector3(offsetX, offsetY, 0);
        }

        /// <summary>
        /// Called during teleports or world swaps to perfectly align all backgrounds.
        /// </summary>
        public static void ResyncAll()
        {
            if (CameraManager.Instance == null) return;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            // 1. SNAP the camera first so the layers capture the correct destination
            CameraManager.Instance.InitializeCamera(player.transform);

            // 2. Refresh all layers (active and inactive)
            ParallaxLayer[] layers = FindObjectsByType<ParallaxLayer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var layer in layers)
            {
                layer.InitializeAnchor();
            }
        }
    }
}
