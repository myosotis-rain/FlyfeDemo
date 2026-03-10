using UnityEngine;

public class LeverController : MonoBehaviour, IInteractable, IResettable
{
    [SerializeField] private DoorController door;
    [SerializeField] private bool isOn = false;

    private bool _initialState;

    void Awake()
    {
        _initialState = isOn;
    }

    public void ResetState()
    {
        isOn = _initialState;
        if (door != null) door.SetOpen(isOn);
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (TryGetComponent<SpriteRenderer>(out var sr))
        {
            sr.flipY = isOn;
        }
    }

    public void Interact(GameObject user)
    {
        isOn = !isOn;
        if (door != null)
        {
            door.SetOpen(isOn);
        }
        
        UpdateVisuals();
        
        Debug.Log("Lever interacted! State: " + (isOn ? "ON" : "OFF"));
    }

    public string GetInteractPrompt()
    {
        return isOn ? "Turn Off" : "Turn On";
    }
}
