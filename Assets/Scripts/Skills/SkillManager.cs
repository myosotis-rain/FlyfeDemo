using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    private List<ISkill> _skills;
    public ISkill ActiveSkill { get; private set; }
    public Type ActiveSkillType => ActiveSkill?.GetType();

    // Event for UI to listen to
    public event Action<ISkill> OnSkillChanged;

    void Awake()
    {
        _skills = GetComponents<ISkill>().ToList();
        if (_skills.Count > 0) SetActiveSkill(0); 
    }

    public void SetActiveSkill(int index)
    {
        if (_skills == null || index < 0 || index >= _skills.Count) return;

        for (int i = 0; i < _skills.Count; i++)
        {
            var mono = _skills[i] as MonoBehaviour;
            if (mono != null) mono.enabled = (i == index);
        }

        ActiveSkill = _skills[index];
        OnSkillChanged?.Invoke(ActiveSkill); // Trigger the update
    }
    
    public void SetActiveSkill(Type type)
    {
        if (_skills == null) _skills = GetComponents<ISkill>().ToList();
        if (_skills == null) return;

        int index = _skills.FindIndex(s => s.GetType() == type);
        if (index != -1) SetActiveSkill(index);
    }

    public void CycleSkills(float direction)
    {
        if (_skills.Count <= 1) return;
        int currentIndex = _skills.IndexOf(ActiveSkill);
        int nextIndex = (direction > 0) 
            ? (currentIndex + 1) % _skills.Count 
            : (currentIndex - 1 + _skills.Count) % _skills.Count;
        SetActiveSkill(nextIndex);
    }
}
