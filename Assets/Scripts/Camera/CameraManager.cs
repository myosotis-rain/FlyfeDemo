using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

namespace Flyfe.Camera
{
    /// <summary>
    /// Centralized manager for camera operations.
    /// Simplified: Parallax layers now handle their own initialization, so we just focus on tracking.
    /// </summary>
    public class CameraManager : MonoBehaviour
    {
        public static CameraManager Instance { get; private set; }

        [Header("Cinemachine References")]
        [SerializeField] private CinemachineCamera virtualCamera;
        [SerializeField] private CinemachineConfiner2D confiner;

        private Transform _playerTransform;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

            if (virtualCamera == null) virtualCamera = FindFirstObjectByType<CinemachineCamera>();
            if (confiner == null) confiner = FindFirstObjectByType<CinemachineConfiner2D>();
        }

        public void InitializeCamera(Transform target)
        {
            if (target == null || virtualCamera == null) return;
            
            _playerTransform = target;
            virtualCamera.Follow = target;

            // Instant Teleport
            Vector3 targetPosition = new Vector3(target.position.x, target.position.y, virtualCamera.transform.position.z);
            if (confiner != null) confiner.InvalidateBoundingShapeCache();

            virtualCamera.transform.position = targetPosition;
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
