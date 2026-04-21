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

            // Professional Jitter Fix: Ensure the camera updates in sync 
            // with the physics engine (FixedUpdate) because the player 
            // is a Rigidbody2D.
            if (virtualCamera != null)
            {
                // Cinemachine 3.x uses different update methods. We force it to stay in sync with physics.
                var brain = UnityEngine.Camera.main.GetComponent<CinemachineBrain>();
                if (brain != null) brain.UpdateMethod = CinemachineBrain.UpdateMethods.FixedUpdate;
            }
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

            // Snap the virtual camera position
            Vector3 targetPosition = new Vector3(target.position.x, target.position.y, virtualCamera.transform.position.z);
            virtualCamera.ForceCameraPosition(targetPosition, Quaternion.identity);

            if (confiner != null) confiner.InvalidateBoundingShapeCache();
        }

        public void SetFollowTarget(Transform target, bool snap = true)
        {
            if (virtualCamera == null || target == null) return;
            
            // If snapping, we need to calculate the warp delta for Cinemachine 3
            if (snap && virtualCamera.Follow != null)
            {
                virtualCamera.OnTargetObjectWarped(target, target.position - virtualCamera.Follow.position);
            }

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
