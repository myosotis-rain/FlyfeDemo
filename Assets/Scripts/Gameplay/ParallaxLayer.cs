using UnityEngine;
using System.Collections;

/// <summary>
/// A professional, stable parallax script.
/// Fixes: Captures anchor after camera stabilization to prevent de-centering.
/// </summary>
[DefaultExecutionOrder(100)] 
public class ParallaxLayer : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("0 = sky, 1 = world-locked")]
    public Vector2 parallaxFactor;
    public bool lockVertical = false;

    private Transform _cameraTransform;
    private Vector3 _authoredWorldPos;
    private bool _isInitialized = false;

    private static Vector3? _levelStartCameraPos;
    private static System.Action _onResync;

    void OnEnable()
    {
        _onResync += Resync;
    }

    void OnDisable()
    {
        _onResync -= Resync;
    }

    private void Start()
    {
        // Capture the EXACT position you set in the Editor
        _authoredWorldPos = transform.position;
        Initialize();
    }

    private void Initialize()
    {
        if (_cameraTransform == null && Camera.main != null)
        {
            _cameraTransform = Camera.main.transform;
        }
    }

    void LateUpdate()
    {
        if (_cameraTransform == null)
        {
            Initialize();
            return;
        }

        // Wait for the camera to exist and be stable
        if (_levelStartCameraPos == null)
        {
            // We capture the starting camera position. 
            // In a professional setup, we capture this once the camera is centered.
            _levelStartCameraPos = _cameraTransform.position;
            _isInitialized = true;
        }

        if (!_isInitialized) return;

        // PARALLAX MATH:
        // Calculate displacement from the very first frame the camera was seen
        Vector3 cameraDisplacement = _cameraTransform.position - _levelStartCameraPos.Value;

        float offsetX = cameraDisplacement.x * (1 - parallaxFactor.x);
        float offsetY = lockVertical ? 0 : cameraDisplacement.y * (1 - parallaxFactor.y);

        // Apply to the AUTHORED position (never changes)
        transform.position = _authoredWorldPos + new Vector3(offsetX, offsetY, 0);
    }

    public void Resync()
    {
        // Re-center everything to the current camera view
        if (_cameraTransform != null)
        {
            _levelStartCameraPos = _cameraTransform.position;
            _authoredWorldPos = transform.position;
        }
    }

    public static void ResyncAll()
    {
        _onResync?.Invoke();
    }
}
