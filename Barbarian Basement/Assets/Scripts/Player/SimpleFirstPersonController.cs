using UnityEngine;

public class SimpleFirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Mouse Look")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private Transform playerCamera;
    private float xRotation = 0f;

    private void Update()
    {
        HandleMovement();
        HandleMouseLook();
    }

    private void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Build movement vector
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        move.y = 0;  // Lock Y position

        transform.position += move * moveSpeed * Time.deltaTime;
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate the player horizontally (yaw)
        transform.Rotate(Vector3.up * mouseX);

        // Rotate the camera vertically (pitch)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
