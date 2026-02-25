using UnityEngine;

// This interface is the template for all skills in the game.
// Any new skill (Hover, Ice Platform, etc.) must implement these methods.
public interface ISkill
{
    // Called once when the skill button is first pressed.
    void StartSkill(Rigidbody2D characterRb);

    // Called every frame the skill button is held down.
    void UpdateSkill(Rigidbody2D characterRb);

    // Called once when the skill button is released.
    void EndSkill(Rigidbody2D characterRb);

    // Called to reset or recharge the skill (e.g., on touching the ground).
    void Recharge();

    // Called to cancel the skill prematurely (e.g., by player input).
    void CancelSkill();

    // Property to check if the skill is currently active
    bool IsActive { get; }
}
