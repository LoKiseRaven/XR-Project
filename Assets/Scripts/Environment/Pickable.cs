using System;
using UnityEngine;

// Put on any object the player can pick up, carry and throw
// Needs a Rigidbody and at least one Collider.
// Scale tip: keep every parent of a Pickable at scale (1,1,1), otherwise the physics
// (and the size of the ball) behave strangely.
[RequireComponent(typeof(Rigidbody))]
public class Pickable : MonoBehaviour
{
    [Header("Rigidbody")]
    [Tooltip("Mass in kg")]
    public float mass = 0.6f;

    [Header("Holding")]
    [Tooltip("How strongly the object is pulled toward the hold point while carried.")]
    public float followStrength = 15f;
    [Tooltip("Auto-drop at this distance")]
    public float maxHoldDistance = 1.5f;

    [Header("Physics feel")]
    public bool overrideMaterial = true;
    [Range(0f, 1f)] public float bounciness = 0.75f;
    public float friction = 0.5f;

    [Header("Safety")]
    [Tooltip("If the object falls below this world height sende back to its starting point.")]
    public float killY = -5f;

    public bool IsHeld { get; private set; }

    // Events so other systems can react without polling.
    public event Action<Pickable> OnPickedUp;
    public event Action<Pickable> OnThrown;
    public event Action<Pickable> OnDropped;
    public event Action<Pickable> OnReset;

    Rigidbody rb;
    Transform holdPoint;
    Vector3 startPos;
    Quaternion startRot;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = mass;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        startPos = transform.position;
        startRot = transform.rotation;

        var colliders = GetComponentsInChildren<Collider>();

        // Runtime physic material: "Maximum" bounce combine means the ball still bounces on floors that dont haev their own no material
        if (overrideMaterial)
        {
            var mat = new PhysicMaterial(name + "_runtime")
            {
                bounciness = bounciness,
                dynamicFriction = friction,
                staticFriction = friction,
                bounceCombine = PhysicMaterialCombine.Maximum,
                frictionCombine = PhysicMaterialCombine.Average
            };
            foreach (var c in colliders) c.sharedMaterial = mat;
        }

        // The player walks through throwables instead of being blocked by them
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && player.TryGetComponent(out CharacterController cc))
            foreach (var c in colliders) Physics.IgnoreCollision(c, cc, true);
    }

    void FixedUpdate()
    {
        if (IsHeld)
        {
            Vector3 toTarget = holdPoint.position - rb.position;

            // Stuck behind geometry: let go rather than fight the wall.
            if (toTarget.magnitude > maxHoldDistance)
            {
                Drop();
                return;
            }

            rb.velocity = toTarget * followStrength;
            rb.angularVelocity = Vector3.zero;
        }
        else if (transform.position.y < killY)
        {
            ResetToStart(); // fell into void
        }
    }

    public void Grab(Transform hold)
    {
        holdPoint = hold;
        IsHeld = true;
        rb.isKinematic = false; // an embedded/frozen item is freed when picked up
        rb.useGravity = false;
        OnPickedUp?.Invoke(this);
    }

    public void Drop()
    {
        if (!IsHeld) return;
        Release();
        OnDropped?.Invoke(this);
    }

    //launch with the given linear velocity (m/s) and spin (rad/s)
    public void Throw(Vector3 velocity, Vector3 spin)
    {
        if (!IsHeld) return;
        Release();
        rb.velocity = velocity;
        rb.angularVelocity = spin;
        OnThrown?.Invoke(this);
    }

    void Release()
    {
        IsHeld = false;
        holdPoint = null;
        rb.useGravity = true;
    }

    //Put the object back where it started.
    public void ResetToStart()
    {
        if (IsHeld) Release();
        rb.isKinematic = false; // velocity cannot be set on a kinematic body
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.position = startPos;
        rb.rotation = startRot;
        transform.SetPositionAndRotation(startPos, startRot);
        OnReset?.Invoke(this);
    }
}