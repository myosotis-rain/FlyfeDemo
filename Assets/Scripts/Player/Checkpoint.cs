using UnityEngine;
using System.Collections.Generic;

namespace Flyfe.Player
{
    [RequireComponent(typeof(Collider2D))]
    public class Checkpoint : MonoBehaviour
    {
        [SerializeField] private bool isActive = false;
        [SerializeField] private Color activeColor = Color.green;
        [SerializeField] private Color inactiveColor = Color.red;
        [SerializeField] private SpriteRenderer feedbackRenderer;

        // Static list to track only checkpoints the player has actually touched
        private static List<Checkpoint> _visitedCheckpoints = new List<Checkpoint>();

        private void Start()
        {
            if (feedbackRenderer == null) feedbackRenderer = GetComponent<SpriteRenderer>();
            UpdateVisuals();
        }

        private void OnDestroy()
        {
            _visitedCheckpoints.Remove(this);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                Activate();
            }
        }

        public void Activate()
        {
            if (!isActive)
            {
                // Deactivate all other checkpoints visuals
                Checkpoint[] allCheckpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
                foreach (var cp in allCheckpoints) cp.SetVisualInactive();

                isActive = true;
                
                // Add to visited list if not already there
                if (!_visitedCheckpoints.Contains(this))
                {
                    _visitedCheckpoints.Add(this);
                }

                UpdateVisuals();

                // Update the player's respawn point
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

        /// <summary>
        /// Finds the nearest checkpoint among those the player has already visited.
        /// </summary>
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
