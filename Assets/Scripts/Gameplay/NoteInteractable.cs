using UnityEngine;

public class NoteInteractable : MonoBehaviour, IInteractable
{
    [SerializeField, TextArea] private string noteContent = "Hello World!";
    
    public void Interact(GameObject user)
    {
        // For now, we just log it. In a real game, this might open a UI window.
        Debug.Log("Reading Note: " + noteContent);
        
        // Show in UI if UIManager has a way to show notes
        // Note: You could expand UIManager to handle this.
    }

    public string GetInteractPrompt()
    {
        return "Read Note";
    }
}
