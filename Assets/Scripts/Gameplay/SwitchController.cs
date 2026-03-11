using UnityEngine;
using Flyfe.Core;

namespace Flyfe.Gameplay
{
    public class SwitchController : MonoBehaviour
    {
        [SerializeField] private DoorController door; 
        private int pressCount = 0;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") || other.CompareTag("Shadow"))
            {
                pressCount++;
                if (door != null) door.SetOpen(true);
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player") || other.CompareTag("Shadow"))
            {
                pressCount--;
                if (pressCount <= 0)
                {
                    pressCount = 0;
                    if (door != null) door.SetOpen(false);
                }
            }
        }
    }
}
