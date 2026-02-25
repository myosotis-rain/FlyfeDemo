using UnityEngine;

public class SwitchController : MonoBehaviour
{
    [SerializeField] private DoorController door; // Drag the door object here in Inspector
    private int pressCount = 0;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(Tags.Player) || other.CompareTag(Tags.Shadow))
        {
            pressCount++;
            if (door != null) door.SetOpen(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(Tags.Player) || other.CompareTag(Tags.Shadow))
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