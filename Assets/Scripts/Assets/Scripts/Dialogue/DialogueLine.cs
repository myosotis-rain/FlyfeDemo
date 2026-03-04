using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    [Tooltip("The name of the character speaking.")]
    public string speakerName;

    [Tooltip("The text to display. Supports TextMeshPro rich text tags like <b>bold</b> or <color=red>red</color>.")]
    [TextArea(3, 6)]
    public string text;

    [Tooltip("Optional portrait of the character speaking.")]
    public Sprite portrait;

    [Tooltip("Custom typing speed for this line. If 0, the UI's default speed is used.")]
    public float customTypingSpeed = 0f;
    
    [Tooltip("Optional audio clip to play as a 'voice blip' while typing this line.")]
    public AudioClip voiceBlip;
}