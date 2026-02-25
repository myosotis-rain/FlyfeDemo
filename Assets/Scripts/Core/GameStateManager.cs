using System;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public enum WorldState { Present, Memory }
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

    void Start()
    {
        SwapWorld(WorldState.Present);
    }

    public void SwapWorld(WorldState state)
    {
        CurrentState = state;
        presentWorldFolder?.SetActive(state == WorldState.Present);
        memoryWorldFolder?.SetActive(state == WorldState.Memory);
        OnWorldChanged?.Invoke(CurrentState);
    }
}
