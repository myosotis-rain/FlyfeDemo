using UnityEngine;
using UnityEngine.EventSystems; // Required for professional UI/Pointer events

public class RotaryTrigger : MonoBehaviour, IPointerClickHandler, IInteractable
{
    private RotarySwitchController controller;

    void Awake() => controller = GetComponentInParent<RotarySwitchController>();

    public void Interact(GameObject user)
    {
        if (controller != null) controller.Interact(user);
    }

    public string GetInteractPrompt() => controller != null ? controller.GetInteractPrompt() : "";

    // This is the professional way to handle clicks. 
    // It works with the Event System and respects UI blocking.
    public void OnPointerClick(PointerEventData eventData)
    {
        if (controller != null)
        {
            // Check distance to the player manually since the switch is a world object
            GameObject player = GameObject.FindGameObjectWithTag(Tags.Player);
            if (player != null)
            {
                float dist = Vector2.Distance(player.transform.position, controller.transform.position);
                if (dist <= 3.0f) // Standard interaction range
                {
                    controller.Interact(player);
                }
            }
        }
    }
}