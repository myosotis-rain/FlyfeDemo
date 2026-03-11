using System;
using UnityEngine;
using Flyfe.Camera;
using Flyfe.Recording;

namespace Flyfe.Core
{
    /// <summary>
    /// Manages high-level game states and world swapping.
    /// Professional Practice: Delegates technical implementation (Camera, UI) to specialized managers.
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        public enum WorldState { Present, Memory, Replay }
        public WorldState CurrentState { get; private set; } = WorldState.Present;
        
        public static event Action<WorldState> OnWorldChanged;

        [Header("World Folders")]
        public GameObject presentWorldFolder;
        public GameObject memoryWorldFolder;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void OnEnable()
        {
            ShadowReplay.OnReplayFinished += HandleReplayFinished;
        }

        private void OnDisable()
        {
            ShadowReplay.OnReplayFinished -= HandleReplayFinished;
        }

        private void Start()
        {
            // First time setup: Do NOT call SwapWorld because it forces a camera teleport
            // Just set the initial active states
            if (presentWorldFolder != null) presentWorldFolder.SetActive(true);
            if (memoryWorldFolder != null) memoryWorldFolder.SetActive(false);
            
            CurrentState = WorldState.Present;
            UpdateCameraBoundaries(CurrentState);
        }

        /// <summary>
        /// Orchestrates a world swap by toggling folders and updating technical systems.
        /// </summary>
        public void SwapWorld(WorldState state)
        {
            if (CurrentState == state) return;
            CurrentState = state;
            
            // Toggle Folders based on state
            bool isMemory = (state == WorldState.Memory);
            
            if (presentWorldFolder != null)
            {
                presentWorldFolder.SetActive(!isMemory); 
            }
            
            if (memoryWorldFolder != null)
            {
                memoryWorldFolder.SetActive(isMemory);
            }
            
            // Notify Camera System
            UpdateCameraBoundaries(state);
            
            // Critical: Resync backgrounds after the new folders are active
            ParallaxLayer.ResyncAll();
            
            OnWorldChanged?.Invoke(CurrentState);
        }

        private void UpdateCameraBoundaries(WorldState state)
        {
            if (CameraManager.Instance == null) return;

            GameObject activeFolder = (state == WorldState.Memory) ? memoryWorldFolder : presentWorldFolder;
            if (activeFolder == null) return;

            // Find the boundary collider
            PolygonCollider2D boundary = null;
            var colliders = activeFolder.GetComponentsInChildren<PolygonCollider2D>(true);
            
            foreach (var col in colliders)
            {
                if (col.isTrigger) 
                { 
                    boundary = col; 
                    break; 
                }
            }

            CameraManager.Instance.UpdateConfiner(boundary);
        }

        private void HandleReplayFinished()
        {
            SwapWorld(WorldState.Present);
        }
    }
}
