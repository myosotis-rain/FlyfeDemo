using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float jumpForce = 12f;
    public LayerMask groundLayer;
    public Transform playerGroundCheck;

    private Rigidbody2D _playerRb;

    void Awake() => _playerRb = GetComponent<Rigidbody2D>();

    void Update()
    {
        float moveInput = 0;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveInput = -1;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveInput = 1;

        Rigidbody2D targetRb = _playerRb;
        Transform targetFeet = playerGroundCheck;

        // Redirect input if recording
        if (RecordManager.Instance != null && RecordManager.Instance.IsRecordingShadow)
        {
            if (RecordManager.Instance.ActiveShadowRb != null)
            {
                targetRb = RecordManager.Instance.ActiveShadowRb;
                targetFeet = RecordManager.Instance.ActiveShadowFeet;
            }
        }

        if (targetRb != null)
        {
            targetRb.WakeUp();
            targetRb.linearVelocity = new Vector2(moveInput * moveSpeed, targetRb.linearVelocity.y);

            if (targetFeet != null)
            {
                bool isGrounded = Physics2D.OverlapCircle(targetFeet.position, 0.25f, groundLayer);
                if (isGrounded && Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    targetRb.linearVelocity = new Vector2(targetRb.linearVelocity.x, jumpForce);
                }
            }
        }
    }
}