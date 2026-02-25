using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

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

    // Runs when mouse enters the button area
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipContainer != null)
        {
            tooltipContainer.SetActive(true);
            if (tooltipText != null) tooltipText.text = message;
        }
    }

    // Runs when mouse leaves the button area
    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipContainer != null) tooltipContainer.SetActive(false);
    }

    // Ensure it closes if the button is clicked and the panel it's on disappears
    void OnDisable()
    {
        if (tooltipContainer != null) tooltipContainer.SetActive(false);
    }
}
