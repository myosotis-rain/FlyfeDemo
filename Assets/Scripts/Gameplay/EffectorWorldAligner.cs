using UnityEngine;

namespace Flyfe.Gameplay
{
    /// <summary>
    /// Put this on a parent object to ensure all child platforms with PlatformEffector2D
    /// always have their "one-way" direction facing world-up, regardless of rotation.
    /// Useful for rotating platforms like pendulums or circular loops.
    /// </summary>
    [ExecuteInEditMode]
    public class EffectorWorldAligner : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("If true, it will update every frame. If false, it only updates in the editor or when prompted.")]
        public bool updateContinuously = true;

        [Tooltip("The world direction we want the effector to treat as 'Up' (default is 0).")]
        public float worldUpAngle = 0f;

        private PlatformEffector2D[] _effectors;

        private void OnEnable()
        {
            RefreshEffectors();
        }

        private void LateUpdate()
        {
            if (updateContinuously || !Application.isPlaying)
            {
                AlignEffectors();
            }
        }

        public void RefreshEffectors()
        {
            _effectors = GetComponentsInChildren<PlatformEffector2D>();
        }

        [ContextMenu("Align Now")]
        public void AlignEffectors()
        {
            if (_effectors == null || _effectors.Length == 0)
            {
                RefreshEffectors();
            }

            foreach (var effector in _effectors)
            {
                if (effector == null) continue;

                // The effector's surface arc is relative to the transform's local up.
                // To make it face world-up, we subtract the transform's current world rotation.
                float currentZ = effector.transform.eulerAngles.z;
                
                // We want: LocalUp + rotationalOffset = WorldUp (worldUpAngle)
                // rotationalOffset = worldUpAngle - currentZ
                effector.rotationalOffset = worldUpAngle - currentZ;
            }
        }
    }
}
