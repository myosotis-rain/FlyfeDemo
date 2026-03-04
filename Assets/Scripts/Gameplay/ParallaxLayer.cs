using UnityEngine;

/// <summary>
/// A stable parallax script designed for Unity 6 Cinemachine.
/// This script calculates position based on an absolute offset from the starting camera position,
/// which prevents jitter and cumulative drift.
/// </summary>
[DefaultExecutionOrder(100)] // Ensure this runs after Cinemachine/Camera updates
public class ParallaxLayer : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("0 = locked to camera (sky), 1 = follows camera 1:1 (foreground), 0.1-0.5 = background depth")]
    public Vector2 parallaxFactor;
    
    [Tooltip("If true, the layer will only move on the X axis.")]
    public bool lockVertical = false;

    private Transform _cameraTransform;
    private Vector3 _startCameraPosition;
    private Vector3 _startLayerPosition;
    private bool _isInitialized = false;

    private static System.Action _onResync;

    void OnEnable()
    {
        _onResync += Resync;
        
        // Only initialize if we haven't already. 
        // This prevents the background from "jumping" when switching worlds.
        if (!_isInitialized)
        {
            InitializePositions();
        }
    }

    void OnDisable()
    {
        _onResync -= Resync;
    }

    void Start()
    {
        if (!_isInitialized)
        {
            InitializePositions();
        }
    }

    private void InitializePositions()
    {
        if (_cameraTransform == null && Camera.main != null)
        {
            _cameraTransform = Camera.main.transform;
        }

        if (_cameraTransform != null)
        {
            _startCameraPosition = _cameraTransform.position;
            _startLayerPosition = transform.position;
            _isInitialized = true;
        }
    }

    void LateUpdate()
    {
        if (!_isInitialized || _cameraTransform == null) return;

        // Calculate total camera displacement since start/resync
        Vector3 cameraDisplacement = _cameraTransform.position - _startCameraPosition;

        // Calculate parallax displacement
        // Using (1 - factor) to match: 0 = camera-locked (sky), 1 = world-locked (gameplay)
        float offsetX = cameraDisplacement.x * (1 - parallaxFactor.x);
        float offsetY = lockVertical ? 0 : cameraDisplacement.y * (1 - parallaxFactor.y);

        // Set the absolute position
        transform.position = _startLayerPosition + new Vector3(offsetX, offsetY, 0);
    }

    /// <summary>
    /// Call this if the camera is teleported (warped) to prevent a massive delta jump.
    /// </summary>
    public void Resync()
    {
        _isInitialized = false;
        InitializePositions();
    }

    /// <summary>
    /// Globally triggers a resync for all active ParallaxLayer instances.
    /// Use this after a camera teleport/warp.
    /// </summary>
    public static void ResyncAll()
    {
        _onResync?.Invoke();
    }
}
