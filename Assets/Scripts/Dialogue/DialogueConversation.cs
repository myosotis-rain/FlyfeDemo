using UnityEngine;

namespace Flyfe.Dialogue
{
    [CreateAssetMenu(fileName = "NewConversation", menuName = "Dialogue/Conversation")]
    public class DialogueConversation : ScriptableObject
    {
        public DialogueLine[] lines;
    }
}
