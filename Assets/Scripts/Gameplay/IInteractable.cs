using UnityEngine;

public interface IInteractable
{
    void Interact(GameObject user);
    string GetInteractPrompt();
}
