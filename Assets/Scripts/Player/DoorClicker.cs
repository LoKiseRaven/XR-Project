using UnityEngine;

/// <summary>
/// Left-click raycast from the screen centre (crosshair). If it hits a Door
/// within range, activates it. Later this can be merged into the full Interactor.
/// </summary>
public class DoorClicker : MonoBehaviour
{
    public Camera playerCamera;          // drag the player's camera here
    public float interactRange = 3f;

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
        {
            Door door = hit.collider.GetComponentInParent<Door>();
            if (door != null) door.Interact();
        }
    }
}