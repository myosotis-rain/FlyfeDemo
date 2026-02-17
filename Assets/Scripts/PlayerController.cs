using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;
    public LayerMask groundLayer;

    [Header("Detection")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    void Update()
    {
        moveInput = 0;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveInput = -1;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveInput = 1;

        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            if (isGrounded && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        if (RecordManager.Instance != null && RecordManager.Instance.CurrentState == RecordManager.State.Memory)
        {
            HandleRecording();
        }
    }

    private void HandleRecording()
    {
        // Record player's world position and platform name (optional)
        string platformName = "";
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1.5f, groundLayer);
        if (hit.collider != null) platformName = hit.collider.gameObject.name;

        RecordManager.Instance.AddFrame(transform.position, platformName);
    }
}
