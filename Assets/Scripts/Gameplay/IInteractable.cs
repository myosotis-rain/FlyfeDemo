using UnityEngine;

namespace Flyfe.Gameplay
{
    public interface IInteractable
    {
        void Interact(GameObject user);
        string GetInteractPrompt();
    }
}
