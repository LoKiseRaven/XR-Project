using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float playerReach = 3f;
    Interactable currentInteractable;
    Door currentDoor;

    // Update is called once per frame
    void Update()
    {
        CheckInteraction();
        if (Input.GetMouseButtonDown(0))
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
        RaycastHit hit;
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        bool hasHit = Physics.Raycast(ray, out hit, playerReach);

        // The Door script can sit on a parent of the collider that was hit
        currentDoor = hasHit ? hit.collider.GetComponentInParent<Door>() : null;

        if (hasHit
            && hit.collider.TryGetComponent(out Interactable newInteractable)
            && newInteractable.enabled)
        {
            if (newInteractable != currentInteractable)
            {
                DisableCurrentInteractable();
                SetNewCurrentInteractable(newInteractable);
            }
        }
        else
        {
            DisableCurrentInteractable();
        }
    }

    void SetNewCurrentInteractable(in Interactable newInteractable)
    {
        currentInteractable = newInteractable;
    }

    void DisableCurrentInteractable()
    {
        if (currentInteractable)
        {
            currentInteractable = null;
        }
    }
}
