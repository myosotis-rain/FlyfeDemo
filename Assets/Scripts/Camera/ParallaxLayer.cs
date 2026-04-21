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

        [Header("Clamping (Optional)")]
        [Tooltip("If enabled, the layer will try to stay within these bounds relative to the camera.")]
        [SerializeField] private bool clampToBounds = false;
        [SerializeField] private SpriteRenderer boundsRenderer;

        private Transform _cameraTransform;
        private Vector3 _authoredWorldPos;
        private bool _isInitialized = false;
        private UnityEngine.Camera _mainCam;

        private void Awake()
        {
            if (gameObject.CompareTag("Player") || name.Contains("Actors") || GetComponentInParent<PlayerController>(true) != null)
            {
                DestroyImmediate(this);
                return;
            }

            // Capture where this object belongs in the world globally
            _authoredWorldPos = transform.position;
            
            if (boundsRenderer == null) boundsRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            _mainCam = UnityEngine.Camera.main;
            if (_mainCam != null) _cameraTransform = _mainCam.transform;
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

            Vector3 targetPos = _authoredWorldPos + new Vector3(offsetX, offsetY, 0);

            if (clampToBounds && boundsRenderer != null)
            {
                targetPos = ApplyClamping(targetPos);
            }

            // Move relative to the world position you gave it in the editor
            transform.position = targetPos;
        }

        private Vector3 ApplyClamping(Vector3 pos)
        {
            if (_mainCam == null || boundsRenderer == null) return pos;

            // Calculate camera viewport dimensions in world space
            float camHeight = _mainCam.orthographicSize * 2f;
            float camWidth = camHeight * _mainCam.aspect;
            
            // Get the current bounds of the sprite
            // We calculate where the bounds WOULD be if we moved the object to 'pos'
            Vector3 currentPos = transform.position;
            Bounds currentBounds = boundsRenderer.bounds;
            Vector3 centerOffset = currentBounds.center - currentPos;
            Vector3 extents = currentBounds.extents;

            Vector3 predictedCenter = pos + centerOffset;

            // Camera View Edges
            float camLeft = _cameraTransform.position.x - camWidth / 2f;
            float camRight = _cameraTransform.position.x + camWidth / 2f;
            float camTop = _cameraTransform.position.y + camHeight / 2f;
            float camBottom = _cameraTransform.position.y - camHeight / 2f;

            // Predicted Background Edges
            float bgLeft = predictedCenter.x - extents.x;
            float bgRight = predictedCenter.x + extents.x;
            float bgTop = predictedCenter.y + extents.y;
            float bgBottom = predictedCenter.y - extents.y;

            // Adjust X: If the camera is past the background's left edge, shift the background right
            if (extents.x * 2f >= camWidth)
            {
                if (bgLeft > camLeft) pos.x -= (bgLeft - camLeft);
                else if (bgRight < camRight) pos.x += (camRight - bgRight);
            }

            // Adjust Y: If the camera is past the background's top/bottom edge, shift the background up/down
            if (!lockVertical && extents.y * 2f >= camHeight)
            {
                if (bgTop < camTop) pos.y += (camTop - bgTop);
                else if (bgBottom > camBottom) pos.y -= (bgBottom - camBottom);
            }

            return pos;
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
