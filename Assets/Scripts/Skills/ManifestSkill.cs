using UnityEngine;
using Flyfe.Gameplay;
using Flyfe.Recording;

namespace Flyfe.Skills
{
    public class ManifestSkill : MonoBehaviour, ISkill
    {
        [Header("Settings")]
        [SerializeField] private GameObject platformPrefab;
        [SerializeField] private Vector2 platformOffset = new Vector2(0, -0.8f);
        [SerializeField] private bool canUseInAirOnly = true;

        private bool _hasUsedInAir = false;
        private bool _isActive = false;

        public bool IsActive => _isActive;

        public void StartSkill(Rigidbody2D characterRb)
        {
            if (_hasUsedInAir && canUseInAirOnly) return;

            SpawnPlatform(characterRb);
            _isActive = true;
        }

        public void UpdateSkill(Rigidbody2D characterRb)
        {
            // This skill is a 'one-shot' trigger, but we could add logic here
            // if we wanted the platform to follow the player before release.
        }

        public void EndSkill(Rigidbody2D characterRb)
        {
            _isActive = false;
        }

        public void Recharge()
        {
            _hasUsedInAir = false;
        }

        public void CancelSkill()
        {
            _isActive = false;
        }

        private void SpawnPlatform(Rigidbody2D characterRb)
        {
            if (platformPrefab == null)
            {
                Debug.LogWarning("ManifestSkill: No Platform Prefab assigned!");
                return;
            }

            // Calculate spawn position relative to player
            Vector3 spawnPos = characterRb.transform.position + (Vector3)platformOffset;
            
            // Professional Practice: Parent to the RecordingService actorRoot
            // This ensures the platform survives world-swaps and shows up in the Present world.
            Transform parent = (RecordingService.Instance != null) ? RecordingService.Instance.transform : null;
            
            // Spawn the platform
            GameObject platform = Instantiate(platformPrefab, spawnPos, Quaternion.identity, parent);
            
            // Mark as used for air-recovery balancing
            _hasUsedInAir = true;

            Debug.Log("<color=cyan>[ManifestSkill]</color> Platform Manifested!");
        }
    }
}
