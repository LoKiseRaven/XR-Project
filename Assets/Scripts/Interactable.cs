using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum InteractionType
{
    Test,
    Button
}

public class Interactable : MonoBehaviour
{
    public InteractionType type;

    [Header("Button")]
    [SerializeField] private float pressDepth = 0.0045f;
    [SerializeField] private float pressSpeed = 10f;
    [SerializeField] private float pressTime = 0.15f;

    // What the button triggers, wired in the inspector (e.g. ColorScreen.SubmitAnswer)
    public UnityEvent onPressed;

    private Vector3 originalPosition;
    private Vector3 targetPosition;

    void Start()
    {
        originalPosition = transform.position;

        targetPosition = originalPosition;
    }

    public void Interact()
    {
        switch (type)
        {
            case InteractionType.Test:
                Debug.Log("Test interaction on " + name);
                break;

            case InteractionType.Button:
                ButtonAction();
                break;
        }
    }

    public void ButtonAction()
    {
        Debug.Log("Button pressed " + name);
        targetPosition = originalPosition + new Vector3(0, -pressDepth, 0);

        CancelInvoke(nameof(ReleaseButton));
        Invoke(nameof(ReleaseButton), pressTime);

        onPressed.Invoke();
    }

    private void ReleaseButton()
    {
        targetPosition = originalPosition;
    }

    private void Update()
    {
        if (type == InteractionType.Button)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * pressSpeed);
        }
    }
}
