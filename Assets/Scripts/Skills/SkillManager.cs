using System;
using System.Collections.Generic;
using UnityEngine;

namespace Flyfe.Skills
{
    /// <summary>
    /// Manages active skills for both the Player and their Shadows.
    /// Professional Practice: Uses component toggling to preserve Inspector data (prefabs, etc).
    /// </summary>
    public class SkillManager : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Check this ONLY on the actual Player object.")]
        [SerializeField] private bool isPlayer = true;
        [Tooltip("The name of the skill to enable on start if no choice is saved (e.g. NoSkill, HoverSkill, ManifestSkill)")]
        [SerializeField] private string startingSkillName = "NoSkill";
        
        private ISkill _activeSkill;
        private Type _activeSkillType;
        private List<ISkill> _allSkills = new List<ISkill>();

        // Static persistence: Remembers the player's choice across world resets and deaths.
        private static Type _persistedSkillType;
        public static Type PersistedSkillType => _persistedSkillType;
        
        private float _cycleCooldown = 0f;

        public ISkill ActiveSkill => _activeSkill;
        public Type ActiveSkillType => _activeSkillType;

        private void Awake()
        {
            // 1. Map all existing ISkill components on this object
            _allSkills.Clear();
            _allSkills.AddRange(GetComponents<ISkill>());

            // 2. Disable all scripts initially
            foreach (var skill in _allSkills)
            {
                if (skill is MonoBehaviour mono) mono.enabled = false;
            }

            // 3. Choice Restoration
            if (isPlayer)
            {
                // Player uses their last choice or defaults to starting skill
                if (_persistedSkillType != null) SetActiveSkill(_persistedSkillType);
                else SetSkillByName(startingSkillName);
            }
            else
            {
                // Shadows ALWAYS copy exactly what the player is currently using
                if (_persistedSkillType != null) SetActiveSkill(_persistedSkillType);
                else SetSkillByName(startingSkillName);
            }
        }

        private void Update()
        {
            if (_cycleCooldown > 0) _cycleCooldown -= Time.deltaTime;
        }

        public void SetSkillByName(string skillName)
        {
            if (string.IsNullOrEmpty(skillName)) return;
            if (!skillName.EndsWith("Skill") && skillName != "NoSkill") skillName += "Skill";

            foreach (var skill in _allSkills)
            {
                if (skill.GetType().Name == skillName)
                {
                    SetActiveSkill(skill.GetType());
                    return;
                }
            }
            Debug.LogWarning($"SkillManager: Component '{skillName}' not found on {name}. Make sure it is attached!");
        }

        public void CycleSkills()
        {
            // Only the Player should initiate cycling
            if (!isPlayer || _allSkills.Count <= 1 || _cycleCooldown > 0) return;
            
            _cycleCooldown = 0.2f; // Flicker prevention

            int currentIndex = _allSkills.FindIndex(s => s.GetType() == _activeSkillType);
            int nextIndex = (currentIndex + 1) % _allSkills.Count;
            
            SetActiveSkill(_allSkills[nextIndex].GetType());
        }

        public void SetActiveSkill(Type skillType)
        {
            if (skillType == null) return;

            // Disable current active skill
            if (_activeSkill != null && _activeSkill is MonoBehaviour oldMono)
            {
                oldMono.enabled = false;
            }

            // Find and Enable the new skill
            foreach (var skill in _allSkills)
            {
                if (skill.GetType() == skillType)
                {
                    _activeSkill = skill;
                    _activeSkillType = skillType;
                    
                    // Only the Player updates the global 'Memory'
                    if (isPlayer) _persistedSkillType = skillType;
                    
                    if (skill is MonoBehaviour newMono) newMono.enabled = true;
                    Debug.Log($"<color=magenta>[SkillManager]</color> {name} Swapped to: {skillType.Name}");
                    return;
                }
            }
        }
    }
}
