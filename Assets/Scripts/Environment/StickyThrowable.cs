using System;
using UnityEngine;

/// Add next to Pickable for dart objects that should stick into surfaces instead of bouncing
[RequireComponent(typeof(Pickable))]
public class StickyThrowable : MonoBehaviour
{
    [Header("Orientation")]
    [Tooltip("Local axis that points from the object towards its tip. Red gizmo shows it.")]
    public Vector3 tipAxis = Vector3.forward;
    [Tooltip("How quickly the item turns tip-first while flying.")]
    public float alignSpeed = 12f;

    [Header("Sticking")]
    [Tooltip("Impact speed (m/s) needed to stick")]
    public float minStickSpeed = 2f;
    [Tooltip("How far the item is pushed into the surface when it sticks.")]
    public float embedDepth = 0.04f;

    public bool IsStuck { get; private set; }

    //Fires when the item sticks
    public event Action<StickyThrowable, Collision> Stuck;

    Rigidbody rb;
    Pickable pickable;
    bool armed;             // true from the moment of the throw until it sticks/is picked up
    Vector3 lastVelocity;   // velocity before the collision is resolved

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        pickable = GetComponent<Pickable>();
        pickable.OnThrown += HandleThrown;
        pickable.OnPickedUp += HandleCancelled;
        pickable.OnDropped += HandleCancelled;
        pickable.OnReset += HandleCancelled;
    }

    void OnDestroy()
    {
        pickable.OnThrown -= HandleThrown;
        pickable.OnPickedUp -= HandleCancelled;
        pickable.OnDropped -= HandleCancelled;
        pickable.OnReset -= HandleCancelled;
    }

    void HandleThrown(Pickable p)
    {
        armed = true;
        IsStuck = false;
    }

    void HandleCancelled(Pickable p)
    {
        armed = false;
        IsStuck = false;
    }

    void FixedUpdate()
    {
        lastVelocity = rb.velocity;

        if (!armed || IsStuck || lastVelocity.sqrMagnitude < 1f) return;

        // Turn the tip towards the direction of travel
        Vector3 worldTip = transform.TransformDirection(tipAxis.normalized);
        Quaternion target = Quaternion.FromToRotation(worldTip, lastVelocity.normalized) * rb.rotation;
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, target, alignSpeed * Time.fixedDeltaTime));
        rb.angularVelocity = Vector3.zero; // no tumbling in flight
    }

    void OnCollisionEnter(Collision c)
    {
        if (!armed || IsStuck) return;
        if (c.relativeVelocity.magnitude < minStickSpeed) return; // too gentle-> fall

        // Direction of travel just before impact
        Vector3 dir = lastVelocity.sqrMagnitude > 0.01f ? lastVelocity.normalized : -c.GetContact(0).normal;

        // Point the tip along the impact direction and push into the surface
        Vector3 worldTip = transform.TransformDirection(tipAxis.normalized);
        rb.rotation = Quaternion.FromToRotation(worldTip, dir) * rb.rotation;
        rb.position += dir * embedDepth;
        transform.SetPositionAndRotation(rb.position, rb.rotation);

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        IsStuck = true;
        armed = false;
        Stuck?.Invoke(this, c);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 worldTip = transform.TransformDirection(tipAxis.normalized);
        Gizmos.DrawLine(transform.position, transform.position + worldTip * 0.5f);
    }
}
