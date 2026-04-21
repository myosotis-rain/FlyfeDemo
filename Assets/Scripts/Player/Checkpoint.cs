using UnityEngine;
using System.Collections.Generic;
using Flyfe.Core;

namespace Flyfe.Player
{
    [RequireComponent(typeof(Collider2D))]
    public class Checkpoint : MonoBehaviour
    {
        [SerializeField] private bool isActive = false;
        [SerializeField] private Color activeColor = Color.green;
        [SerializeField] private Color inactiveColor = Color.red;
        [SerializeField] private SpriteRenderer feedbackRenderer;

        // Track all checkpoints in the scene for visual management
        private static List<Checkpoint> _allCheckpoints = new List<Checkpoint>();
        // Track only checkpoints the player has actually touched
        private static List<Checkpoint> _visitedCheckpoints = new List<Checkpoint>();

        private void Awake()
        {
            _allCheckpoints.Add(this);
            
            // Ensure the checkpoint is always a trigger so the player 
            // doesn't bump into it or get glued to its surface.
            if (TryGetComponent<Collider2D>(out var col))
            {
                col.isTrigger = true;
            }
        }

        private void Start()
        {
            if (feedbackRenderer == null) feedbackRenderer = GetComponent<SpriteRenderer>();
            UpdateVisuals();
        }

        private void OnDestroy()
        {
            _allCheckpoints.Remove(this);
            _visitedCheckpoints.Remove(this);
        }

        /// <summary>
        /// Call this when changing levels or restarting to prevent cross-scene data leakage.
        /// </summary>
        public static void ClearAllData()
        {
            _allCheckpoints.Clear();
            _visitedCheckpoints.Clear();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(Tags.Player))
            {
                Activate();
            }
        }

        public void Activate()
        {
            if (!isActive)
            {
                // Professional Practice: Instead of searching the whole scene, 
                // we use our cached list of checkpoints to update visuals.
                foreach (var cp in _allCheckpoints) 
                {
                    cp.SetVisualInactive();
                }

                isActive = true;
                
                if (!_visitedCheckpoints.Contains(this))
                {
                    _visitedCheckpoints.Add(this);
                }

                UpdateVisuals();

                PlayerRespawn respawnSystem = FindFirstObjectByType<PlayerRespawn>();
                if (respawnSystem != null)
                {
                    respawnSystem.SetRespawnPoint(transform);
                    Debug.Log($"<color=green>[Checkpoint]</color> {name} Activated and Saved!");
                }
            }
        }

        public void SetVisualInactive()
        {
            isActive = false;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (feedbackRenderer != null)
            {
                feedbackRenderer.color = isActive ? activeColor : inactiveColor;
            }
        }

        public static Transform GetNearestVisitedTransform(Vector3 playerPos)
        {
            if (_visitedCheckpoints.Count == 0) return null;

            Checkpoint nearest = null;
            float minDist = float.MaxValue;

            foreach (var cp in _visitedCheckpoints)
            {
                if (cp == null) continue;
                float d = Vector2.Distance(playerPos, cp.transform.position);
                if (d < minDist)
                {
                    minDist = d;
                    nearest = cp;
                }
            }

            return nearest != null ? nearest.transform : null;
        }
    }
}
