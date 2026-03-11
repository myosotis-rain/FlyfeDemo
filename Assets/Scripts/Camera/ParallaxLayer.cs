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
        private Vector3 _initialWorldPos;

        private void Awake()
        {
            if (gameObject.CompareTag("Player") || name.Contains("Actors") || GetComponentInParent<PlayerController>(true) != null)
            {
                DestroyImmediate(this);
                return;
            }
            _initialWorldPos = transform.position;
        }

        private void Start()
        {
            if (UnityEngine.Camera.main != null) _cameraTransform = UnityEngine.Camera.main.transform;
        }

        public void ResetToBase()
        {
            // Reset the object to its starting position for the current resync
            transform.position = _initialWorldPos;
        }

        void Update()
        {
            if (_cameraTransform == null || CameraManager.Instance == null || !CameraManager.Instance.ParallaxAnchor.HasValue)
                return;

            Vector3 anchor = CameraManager.Instance.ParallaxAnchor.Value;
            Vector3 cameraDisplacement = _cameraTransform.position - anchor;

            float offsetX = cameraDisplacement.x * (1 - parallaxFactor.x);
            float offsetY = lockVertical ? 0 : cameraDisplacement.y * (1 - parallaxFactor.y);

            // Calculate movement relative to the initialauthored position
            transform.position = _initialWorldPos + new Vector3(offsetX, offsetY, 0);
        }

        public static void ResyncAll()
        {
            if (CameraManager.Instance == null) return;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            // 1. Tell all layers to return to base to calculate from the new anchor
            ParallaxLayer[] layers = FindObjectsByType<ParallaxLayer>(FindObjectsSortMode.None);
            foreach (var layer in layers) layer.ResetToBase();

            // 2. Snap the camera to the player and set the new anchor
            CameraManager.Instance.InitializeCamera(player.transform);
        }
    }
}
