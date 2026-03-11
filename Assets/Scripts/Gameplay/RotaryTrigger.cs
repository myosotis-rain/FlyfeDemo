using UnityEngine;
using UnityEngine.EventSystems;

namespace Flyfe.Gameplay
{
    public class RotaryTrigger : MonoBehaviour, IPointerClickHandler, IInteractable
    {
        private RotarySwitchController controller;

        void Awake()
        {
            controller = GetComponentInParent<RotarySwitchController>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && controller != null)
            {
                float dist = Vector2.Distance(player.transform.position, controller.transform.position);
                if (dist <= 3.0f) 
                {
                    controller.Interact(player);
                }
            }
        }

        public void Interact(GameObject user)
        {
            if (controller != null) controller.Interact(user);
        }

        public string GetInteractPrompt() => controller != null ? controller.GetInteractPrompt() : "";
    }
}
