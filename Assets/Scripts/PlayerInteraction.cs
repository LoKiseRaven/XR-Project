using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float playerReach = 3f;
    Interactable currentInteractable;

    // Update is called once per frame
    void Update()
    {
        CheckInteraction();
        if (Input.GetKeyDown(KeyCode.F) && currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    void CheckInteraction()
    {
        RaycastHit hit;
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        if (Physics.Raycast(ray, out hit, playerReach)
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
