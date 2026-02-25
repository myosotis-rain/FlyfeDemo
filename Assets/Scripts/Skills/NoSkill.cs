using UnityEngine;

public class NoSkill : MonoBehaviour, ISkill
{
    public bool IsActive => false;
    public void StartSkill(Rigidbody2D rb) { }
    public void UpdateSkill(Rigidbody2D rb) { }
    public void EndSkill(Rigidbody2D rb) { }
    public void CancelSkill() { }
    public void Recharge() { }
}
