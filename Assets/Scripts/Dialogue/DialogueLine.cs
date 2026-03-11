using UnityEngine;

namespace Flyfe.Dialogue
{
    [System.Serializable]
    public class DialogueLine
    {
        public string speakerName;
        [TextArea(3, 10)]
        public string text;
        public Sprite portrait;
        public AudioClip voiceBlip;
        public float customTypingSpeed = 0f;
    }
}
