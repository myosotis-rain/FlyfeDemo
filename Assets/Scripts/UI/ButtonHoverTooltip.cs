using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace Flyfe.UI
{
    public class ButtonHoverTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI Reference")]
        [SerializeField] private GameObject tooltipContainer; 
        [SerializeField] private TMP_Text tooltipText;
        
        [Header("Content")]
        [SerializeField, TextArea] private string message = "<b>HOVER FAIRY</b>\n[F] to GLIDE.\n[S] or [Down] to DROP.";

        void Start()
        {
            if (tooltipContainer != null) tooltipContainer.SetActive(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (tooltipContainer != null)
            {
                tooltipContainer.SetActive(true);
                if (tooltipText != null) tooltipText.text = message;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (tooltipContainer != null) tooltipContainer.SetActive(false);
        }

        void OnDisable()
        {
            if (tooltipContainer != null) tooltipContainer.SetActive(false);
        }
    }
}
