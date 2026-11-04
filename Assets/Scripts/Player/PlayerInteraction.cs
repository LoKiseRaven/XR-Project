using UnityEngine;
using UnityEngine.UIElements;

// Crosshair interaction for the Player 
// Controls
//   Empty-handed:  Left click          -> press an Interactable / click a Door / pick up a Pickable
//                  E                   -> pick up a Pickable
//   Holding:       Hold left click     -> charge the power bar, release to throw
//                  E                   -> drop gently
public class PlayerInteraction : MonoBehaviour
{
    [Header("Reach")]
    public float playerReach = 3f;

    [Header("Holding")]
    [Tooltip("How far in front of the camera the held object floats.")]
    public float holdDistance = 0.9f;
    [Tooltip("Vertical offset of the held object relative to the camera")]
    public float holdHeight = -0.15f;

    [Header("Throwing")]
    public float minThrowSpeed = 4f;
    public float maxThrowSpeed = 11f;
    [Tooltip("Seconds of holding the button to reach full power.")]
    public float chargeTime = 1.5f;
    [Tooltip("Extra upward tilt (degrees) added to the throw so a level aim still arcs.")]
    public float throwUpAngle = 8f;
    [Tooltip("Backspin in rad/s, makes the ball look and roll more natural.")]
    public float backspin = 8f;

    // What the crosshair currently points at (refreshed every frame while empty-handed)
    Interactable currentInteractable;
    Door currentDoor;
    Pickable currentPickable;

    // Throwing state
    Camera cam;
    Transform holdPoint;
    Pickable held;
    bool charging;
    float chargeStart;

    void Awake()
    {
        cam = Camera.main;

        // The held object floats toward this point, child of the camera.
        var go = new GameObject("HoldPoint");
        holdPoint = go.transform;
        holdPoint.SetParent(cam.transform, false);
        holdPoint.localPosition = new Vector3(0f, holdHeight, holdDistance);
    }

    void Update()
    {
        if (UnityEngine.Cursor.lockState != CursorLockMode.Locked) return;

        if (held != null && !held.IsHeld)
        {
            held = null;
            charging = false;
        }

        if (held != null)
        {
            UpdateHolding();
            return;
        }

        CheckInteraction();

        bool click = Input.GetMouseButtonDown(0);
        bool useKey = Input.GetKeyDown(KeyCode.E);

        if (currentPickable != null && (click || useKey))
        {
            PickUp(currentPickable);
        }
        else if (click)
        {
            if (currentInteractable != null)
            {
                currentInteractable.Interact();
            }
            else if (currentDoor != null)
            {
                currentDoor.Interact();
            }
        }
    }

    void CheckInteraction()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        bool hasHit = Physics.Raycast(ray, out RaycastHit hit, playerReach);

        currentDoor = hasHit ? hit.collider.GetComponentInParent<Door>() : null;
        currentPickable = hasHit ? hit.collider.GetComponentInParent<Pickable>() : null;

        if (hasHit
            && hit.collider.TryGetComponent(out Interactable newInteractable)
            && newInteractable.enabled)
        {
            currentInteractable = newInteractable;
        }
        else
        {
            currentInteractable = null;
        }
    }

    void PickUp(Pickable p)
    {
        held = p;
        held.Grab(holdPoint);
        charging = false; // charging only starts on the NEXT mouse-down, so the 1st click does not instantly throw it
        currentInteractable = null;
        currentDoor = null;
        currentPickable = null;
    }

    void UpdateHolding()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            held.Drop();
            held = null;
            charging = false;
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            charging = true;
            chargeStart = Time.time;
        }

        if (charging && Input.GetMouseButtonUp(0))
        {
            ThrowHeld();
        }
    }

    void ThrowHeld()
    {
        float speed = Mathf.Lerp(minThrowSpeed, maxThrowSpeed, ChargeFraction);

        // Tilt the aim direction upward around the camera's right axis.
        Vector3 dir = Quaternion.AngleAxis(-throwUpAngle, cam.transform.right) * cam.transform.forward;
        Vector3 spin = -cam.transform.right * backspin;

        held.Throw(dir * speed, spin);
        held = null;
        charging = false;
    }

    float ChargeFraction => charging ? Mathf.Clamp01((Time.time - chargeStart) / chargeTime) : 0f;

    // Temporary crosshair + power bar
    void OnGUI()
    {
        float cx = Screen.width * 0.5f, cy = Screen.height * 0.5f;
        GUI.DrawTexture(new Rect(cx - 2, cy - 2, 4, 4), Texture2D.whiteTexture);

        if (charging)
        {
            float w = 200f, h = 10f;
            Rect bg = new Rect(cx - w / 2, Screen.height - 80, w, h);
            GUI.color = new Color(0, 0, 0, 0.6f);
            GUI.DrawTexture(bg, Texture2D.whiteTexture);
            GUI.color = Color.Lerp(Color.green, Color.red, ChargeFraction);
            GUI.DrawTexture(new Rect(bg.x, bg.y, w * ChargeFraction, h), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
    }
}