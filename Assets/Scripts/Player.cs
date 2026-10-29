using UnityEngine;

/*
    source : https://youtu.be/6FitlbrpjlQ?si=HXDs3KcsBGKSW_Lh
*/

public class Player : MonoBehaviour
{
    // Camera Rotation
    public float mouseSensitivity = 2f;
    private float verticalRotation = 0f;
    private float horizontalRotation = 0f;
    private Transform cameraTransform;

    // Ground Movement
    private Rigidbody rb;
    public float MoveSpeed = 5f;
    private float moveHorizontal;
    private float moveForward;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        horizontalRotation = transform.eulerAngles.y;
        cameraTransform = Camera.main.transform;

        // Hides the mouse
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        moveHorizontal = Input.GetAxisRaw("Horizontal");
        moveForward = Input.GetAxisRaw("Vertical");

        ReadMouse();
    }

    void FixedUpdate()
    {
        // Body yaw follows the camera through the Rigidbody so interpolation doesn't fight it
        rb.MoveRotation(Quaternion.Euler(0, horizontalRotation, 0));
        MovePlayer();
    }

    void MovePlayer()
    {

        Quaternion yaw = Quaternion.Euler(0, horizontalRotation, 0);
        Vector3 movement = (yaw * Vector3.right * moveHorizontal + yaw * Vector3.forward * moveForward).normalized;
        Vector3 targetVelocity = movement * MoveSpeed;

        Vector3 velocity = rb.velocity;
        velocity.x = targetVelocity.x;
        velocity.z = targetVelocity.z;
        rb.velocity = velocity;
    }

    void ReadMouse()
    {
        horizontalRotation += Input.GetAxis("Mouse X") * mouseSensitivity;

        verticalRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
    }

    void LateUpdate()
    {
        // World rotation set every frame, after Rigidbody interpolation, so the view isn't capped to the physics rate
        cameraTransform.rotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0);
    }
}
