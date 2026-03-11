using System;
using System.Collections.Generic;
using UnityEngine;

namespace Flyfe.Skills
{
    public class SkillManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private string startingSkillName = "NoSkill";

        [Header("Available Skills")]
        [SerializeField] private List<GameObject> skillPrefabs;
        
        private ISkill _activeSkill;
        private Type _activeSkillType;

        public ISkill ActiveSkill => _activeSkill;
        public Type ActiveSkillType => _activeSkillType;

        private void Awake()
        {
            // Initialize with the starting skill
            if (_activeSkill == null)
            {
                Type t = Type.GetType("Flyfe.Skills." + startingSkillName);
                if (t == null) t = typeof(NoSkill);
                
                SetActiveSkill(t);
            }
        }

        public void SetActiveSkill(Type skillType)
        {
            if (skillType == null) return;

            // Remove existing skill component
            if (_activeSkill != null && _activeSkill is MonoBehaviour mono)
            {
                Destroy(mono);
            }

            // Add new skill component
            _activeSkill = gameObject.AddComponent(skillType) as ISkill;
            _activeSkillType = skillType;
            
            Debug.Log($"[{name}] Skill swapped to: {skillType.Name}");
        }

        public void CycleSkills()
        {
            // Simple cycle between NoSkill and HoverSkill for now
            if (_activeSkillType == typeof(NoSkill))
            {
                SetActiveSkill(typeof(HoverSkill));
            }
            else
            {
                SetActiveSkill(typeof(NoSkill));
            }
        }
    }
}
