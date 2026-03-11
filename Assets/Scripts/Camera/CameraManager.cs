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

        private Transform _playerTransform;
        private Vector3? _parallaxAnchor;

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
            // Wait for Cinemachine to settle
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            _parallaxAnchor = UnityEngine.Camera.main.transform.position;
            
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) _playerTransform = player.transform;
        }

        public void InitializeCamera(Transform target)
        {
            if (target == null || virtualCamera == null) return;
            
            _playerTransform = target;
            virtualCamera.Follow = target;

            Vector3 targetPosition = new Vector3(target.position.x, target.position.y, virtualCamera.transform.position.z);
            if (confiner != null) confiner.InvalidateBoundingShapeCache();

            virtualCamera.transform.position = targetPosition;
            
            // We do NOT update the parallax anchor here! 
            // It must remain at the start of the level for consistent world-swapping.
        }

        public void SetFollowTarget(Transform target, bool snap = true)
        {
            if (virtualCamera == null || target == null) return;
            virtualCamera.Follow = target;
            if (snap) InitializeCamera(target);
        }

        public void UpdateConfiner(PolygonCollider2D newBoundary = null)
        {
            if (confiner == null) return;
            if (newBoundary != null) confiner.BoundingShape2D = newBoundary;
            confiner.InvalidateBoundingShapeCache();
        }
    }
}
