using System;
using UnityEngine;
using TMPro;
using Flyfe.Skills;
using Flyfe.Player;

namespace Flyfe.UI
{
    public class SkillSelectorUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text activeSkillText; 
        [SerializeField] private GameObject selectionPanel;

        private SkillManager _playerSkillManager;

        void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerSkillManager = player.GetComponent<SkillManager>();
            }
            if (selectionPanel != null) selectionPanel.SetActive(false);
        }

        void Update()
        {
            if (_playerSkillManager != null && activeSkillText != null)
            {
                activeSkillText.text = _playerSkillManager.ActiveSkill?.GetType().Name ?? "None";
            }
        }
    }
}
