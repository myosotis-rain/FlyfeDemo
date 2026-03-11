using UnityEngine;

namespace Flyfe.Gameplay
{
    public class NoteInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField, TextArea] private string noteContent = "Hello World!";
        
        public void Interact(GameObject user)
        {
            Debug.Log("Reading Note: " + noteContent);
        }

        public string GetInteractPrompt()
        {
            return "Read Note";
        }
    }
}
