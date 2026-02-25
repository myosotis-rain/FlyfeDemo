using UnityEngine;

public class LeverController : MonoBehaviour, IInteractable
{
    [SerializeField] private DoorController door;
    [SerializeField] private bool isOn = false;

    public void Interact(GameObject user)
    {
        isOn = !isOn;
        if (door != null)
        {
            door.SetOpen(isOn);
        }
        
        // Visual feedback for lever state (optional, e.g., flipping a sprite)
        if (TryGetComponent<SpriteRenderer>(out var sr))
        {
            sr.flipY = isOn; // Simple placeholder visual feedback
        }
        
        Debug.Log("Lever interacted! State: " + (isOn ? "ON" : "OFF"));
    }

    public string GetInteractPrompt()
    {
        return isOn ? "Turn Off" : "Turn On";
    }
}
