using UnityEngine;

public class CameraFollow : MonoBehaviour {
    public Transform player; 
    public float smoothTime = 0.15f;
    private Vector3 velocity = Vector3.zero;

    void LateUpdate() {
        if (player != null) {
            // We lock Y and Z, only following the Player's X progress
            Vector3 targetPosition = new Vector3(player.position.x + 4, transform.position.y, -10);
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
    }
}