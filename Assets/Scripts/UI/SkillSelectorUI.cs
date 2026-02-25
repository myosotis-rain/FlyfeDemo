using System;
using UnityEngine;
using TMPro; // Added for TextMesh Pro support

public class SkillSelectorUI : MonoBehaviour
{
    [SerializeField] private TMP_Text activeSkillText; // Changed to TMP_Text
    [SerializeField] private GameObject selectionPanel;

    private SkillManager _sm;

    void OnEnable() 
    {
        if (_sm == null) FindSkillManager();
        if (_sm != null) _sm.OnSkillChanged += RefreshUI;
    }

    void OnDisable() 
    {
        if (_sm != null) _sm.OnSkillChanged -= RefreshUI;
    }

    void Start()
    {
        if (selectionPanel != null) selectionPanel.SetActive(false);
        if (_sm == null) FindSkillManager();
        if (_sm != null && _sm.ActiveSkill != null) RefreshUI(_sm.ActiveSkill);
    }

    private void FindSkillManager()
    {
        GameObject player = GameObject.FindGameObjectWithTag(Tags.Player);
        if (player != null) _sm = player.GetComponent<SkillManager>();
    }

    private void RefreshUI(ISkill newSkill)
    {
        if (activeSkillText != null && newSkill != null)
        {
            // Changes "HoverSkill" to "Hover"
            activeSkillText.text = newSkill.GetType().Name.Replace("Skill", "");
        }
    }

    public void TogglePanel()
    {
        if (selectionPanel != null) selectionPanel.SetActive(!selectionPanel.activeSelf);
    }

    // Button Link Methods
    public void SelectHover() => SelectSkill(typeof(HoverSkill));
    public void SelectNone() => SelectSkill(typeof(NoSkill));

    private void SelectSkill(Type type)
    {
        GameObject player = GameObject.FindGameObjectWithTag(Tags.Player);
        if (player != null)
        {
            SkillManager sm = player.GetComponent<SkillManager>();
            if (sm != null)
            {
                sm.SetActiveSkill(type);
                Debug.Log("UI: Successfully switched player skill to: " + type.Name);
            }
            else
            {
                Debug.LogError("UI: Player found but it has no SkillManager component!");
            }
        }
        else
        {
            Debug.LogError("UI: Could not find any GameObject with tag 'Player'!");
        }

        if (selectionPanel != null) selectionPanel.SetActive(false);
    }
}
