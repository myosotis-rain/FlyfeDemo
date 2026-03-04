using System;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public enum WorldState { Present, Memory, Replay }
    public WorldState CurrentState { get; private set; } = WorldState.Present;
    
    public static event Action<WorldState> OnWorldChanged;

    [Header("Worlds")]
    public GameObject presentWorldFolder;
    public GameObject memoryWorldFolder;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
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
        
        // Toggle World Folders
        if (presentWorldFolder != null)
            presentWorldFolder.SetActive(state == WorldState.Present || state == WorldState.Replay);
        
        if (memoryWorldFolder != null)
            memoryWorldFolder.SetActive(state == WorldState.Memory);
        
        OnWorldChanged?.Invoke(CurrentState);
    }

    private void HandleReplayFinished()
    {
        // When the replay is done, go back to the present.
        SwapWorld(WorldState.Present);
    }
}
