using UnityEngine;

[CreateAssetMenu(fileName = "New Conversation", menuName = "Dialogue System/Conversation")]
public class DialogueConversation : ScriptableObject
{
    [Tooltip("A conversation consists of a series of lines executed in order.")]
    public DialogueLine[] lines;
}