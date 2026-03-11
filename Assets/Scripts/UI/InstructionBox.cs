using UnityEngine;
using TMPro;

namespace Flyfe.UI
{
    public class InstructionBox : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI instructionTextComponent;
        [SerializeField, TextArea(5, 10)] private string instructionsContent; 
        [SerializeField] private bool startVisible = true;

        void Awake()
        {
            if (instructionTextComponent == null)
            {
                instructionTextComponent = GetComponent<TextMeshProUGUI>();
            }

            if (instructionTextComponent != null)
            {
                instructionTextComponent.text = FormatInstructions(instructionsContent);
                instructionTextComponent.gameObject.SetActive(startVisible);
            }
            else
            {
                Debug.LogWarning("InstructionBox: No TextMeshProUGUI component found or assigned. Please assign a Text component.");
            }
        }

        public void SetVisible(bool isVisible)
        {
            if (instructionTextComponent != null)
            {
                instructionTextComponent.gameObject.SetActive(isVisible);
            }
        }

        public void UpdateContent(string newContent)
        {
            if (instructionTextComponent != null)
            {
                instructionTextComponent.text = FormatInstructions(newContent);
            }
        }

        private string FormatInstructions(string rawContent)
        {
            string[] lines = rawContent.Split('\n');
            string formatted = "";
            foreach (string line in lines)
            {
                if (line.StartsWith("---"))
                {
                    formatted += "\n" + line + "\n"; 
                }
                else if (!string.IsNullOrWhiteSpace(line))
                {
                    formatted += line.Trim() + "\n";
                }
            }
            return formatted.Trim();
        }
    }
}
