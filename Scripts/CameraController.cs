using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player; // Reference to the player transform
    public Transform leftWall; // Left boundary
    public Transform rightWall; // Right boundary
    public Transform topWall; // Top boundary
    public Transform bottomWall; // Bottom boundary

    private Camera cam; // Reference to the camera component

    void Start ()
    {
        // Get the camera component attached to this GameObject
        cam = GetComponent<Camera>();
    }

    void LateUpdate ()
    {
        // Calculate the camera's half-size in world units
        float halfHeight = cam.orthographicSize; // For orthographic cameras
        float halfWidth = halfHeight * cam.aspect; // Based on aspect ratio

        // Get the player's position
        Vector3 targetPosition = player.position;

        // Clamp the camera's position to ensure its edges stay within the walls
        float clampedX = Mathf.Clamp(targetPosition.x,leftWall.position.x + halfWidth,rightWall.position.x - halfWidth);
        float clampedY = Mathf.Clamp(targetPosition.y,bottomWall.position.y + halfHeight,topWall.position.y - halfHeight);

        // Set the camera's position with the clamped values, keeping Z-axis unchanged
        transform.position = new Vector3(clampedX,clampedY,transform.position.z);
    }
}
