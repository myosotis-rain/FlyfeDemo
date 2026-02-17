using UnityEngine;

public class SwitchController : MonoBehaviour
{
    public DoorController door; // Drag the door object here in Inspector
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