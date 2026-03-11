using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

namespace Flyfe.Camera
{
    public class CameraManager : MonoBehaviour
    {
        public static CameraManager Instance { get; private set; }

        [Header("Cinemachine References")]
        [SerializeField] private CinemachineCamera virtualCamera;
        [SerializeField] private CinemachineConfiner2D confiner;

        private Vector3? _parallaxAnchor;
        private Transform _playerTransform;

        public Vector3? ParallaxAnchor => _parallaxAnchor;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

            if (virtualCamera == null) virtualCamera = FindFirstObjectByType<CinemachineCamera>();
            if (confiner == null) confiner = FindFirstObjectByType<CinemachineConfiner2D>();
        }

        private IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
                _parallaxAnchor = UnityEngine.Camera.main.transform.position;
            }
        }

        public void InitializeCamera(Transform target)
        {
            if (target == null || virtualCamera == null) return;
            
            _playerTransform = target;
            virtualCamera.Follow = target;

            // 1. Force the camera to move to the new Z-Depth immediately
            Vector3 targetPosition = new Vector3(target.position.x, target.position.y, virtualCamera.transform.position.z);
            
            // 2. Invalidate the confiner cache so it doesn't "pull" the camera back to the old spot
            if (confiner != null) confiner.InvalidateBoundingShapeCache();

            // 3. Teleport the camera transform directly (bypassing smoothing/damping)
            virtualCamera.transform.position = targetPosition;

            // 4. Update the global anchor so parallax starts fresh from this new spot
            _parallaxAnchor = targetPosition;
        }

        public void SetFollowTarget(Transform target, bool snap = true)
        {
            if (virtualCamera == null || target == null) return;
            
            virtualCamera.Follow = target;
            if (snap)
            {
                InitializeCamera(target);
            }
        }

        public void UpdateConfiner(PolygonCollider2D newBoundary = null)
        {
            if (confiner == null) return;
            if (newBoundary != null) confiner.BoundingShape2D = newBoundary;
            confiner.InvalidateBoundingShapeCache();
        }
    }
}
