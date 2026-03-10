using System;
using UnityEngine;
using Unity.Cinemachine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public enum WorldState { Present, Memory, Replay }
    public WorldState CurrentState { get; private set; } = WorldState.Present;
    
    public static event Action<WorldState> OnWorldChanged;

    [Header("Worlds")]
    public GameObject presentWorldFolder;
    public GameObject memoryWorldFolder;

    [Header("Cinemachine")]
    [SerializeField] private CinemachineCamera cinemachineCamera;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void OnEnable()
    {
        ShadowReplay.OnReplayFinished += HandleReplayFinished;
    }

    void OnDisable()
    {
        ShadowReplay.OnReplayFinished -= HandleReplayFinished;
    }

    void Start()
    {
        SwapWorld(WorldState.Present);
    }

    public void SwapWorld(WorldState state)
    {
        CurrentState = state;
        
        // Deactivate Present when in Memory to ensure background shows.
        if (presentWorldFolder != null)
            presentWorldFolder.SetActive(state == WorldState.Present || state == WorldState.Replay);
        
        if (memoryWorldFolder != null)
            memoryWorldFolder.SetActive(state == WorldState.Memory);
        
        OnWorldChanged?.Invoke(CurrentState);

        // Update Camera Confiner to match the new world's boundaries
        UpdateConfinerShape(state);
        ParallaxLayer.ResyncAll();
    }

    private void UpdateConfinerShape(WorldState state)
    {
        if (cinemachineCamera == null) return;
        
        var confiner = cinemachineCamera.GetComponent<CinemachineConfiner2D>();
        if (confiner == null) return;

        // Find the boundary specifically for the active world
        GameObject activeFolder = (state == WorldState.Memory) ? memoryWorldFolder : presentWorldFolder;
        if (activeFolder == null) return;

        // CRITICAL: Must find inactive colliders too because we just toggled the folder!
        PolygonCollider2D boundaryShape = null;
        var colliders = activeFolder.GetComponentsInChildren<PolygonCollider2D>(true);
        
        foreach (var col in colliders)
        {
            if (col.isTrigger) 
            {
                boundaryShape = col;
                break;
            }
        }

        if (boundaryShape == null && colliders.Length > 0)
        {
            boundaryShape = colliders[0];
        }

        if (boundaryShape != null)
        {
            confiner.BoundingShape2D = boundaryShape;
            confiner.InvalidateBoundingShapeCache();
        }
        else
        {
            // If NO boundary is found, it's safer to clear the confiner than to have it stuck
            confiner.BoundingShape2D = null;
        }
    }

    private void HandleReplayFinished()
    {
        SwapWorld(WorldState.Present);
    }
}
