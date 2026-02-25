using UnityEngine;

public class DoorController : MonoBehaviour
{
    [SerializeField] private Vector3 positionOffsetWhenOpen = new Vector3(0, 3, 0); // Direction and distance to move
    [SerializeField] private float slideSpeed = 5f;
    
    private Vector3 closedPos;
    private Vector3 targetPos;

    void Awake()
    {
        closedPos = transform.position;
        targetPos = closedPos;
    }

    void Update()
    {
        // Smoothly slides the door/bridge to the target position
        transform.position = Vector3.MoveTowards(transform.position, targetPos, slideSpeed * Time.deltaTime);
    }

    public void SetOpen(bool isOpen)
    {
        targetPos = isOpen ? closedPos + positionOffsetWhenOpen : closedPos;
    }
}