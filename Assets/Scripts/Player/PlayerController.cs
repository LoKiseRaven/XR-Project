using UnityEngine;

/// <summary>
/// First-person movement: WASD to walk, mouse to look, with gravity.
/// Horizontal look rotates the Player body; vertical look tilts only the camera.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public Transform cameraTransform;   // the child camera
    public float walkSpeed = 4f;
    public float mouseSensitivity = 2f;
    public float gravity = -20f;

    private CharacterController controller;
    private float pitch;                // camera up/down angle
    private float verticalVelocity;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked; // hide and lock the cursor
        Cursor.visible = false;
    }

    private void Update()
    {
        // --- Look ---
        // Yaw is applied straight to the transform (no stored angle), so
        // PlayerSpawner's rotation is never overwritten.
        float mx = Input.GetAxis("Mouse X") * mouseSensitivity;
        float my = Input.GetAxis("Mouse Y") * mouseSensitivity;
        transform.Rotate(0f, mx, 0f);
        pitch = Mathf.Clamp(pitch - my, -85f, 85f);
        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        // --- Move ---
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        Vector3 move = transform.TransformDirection(Vector3.ClampMagnitude(input, 1f)) * walkSpeed;

        // Gravity: small downward push keeps the controller grounded.
        if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);

        // Press Esc to free the cursor while testing in the editor.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}