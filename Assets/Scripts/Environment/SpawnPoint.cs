using UnityEngine;

/// <summary>
/// Marks a place where the player can appear. Put one in front of each door
/// (outside the door's trigger, if it has one), with the blue arrow (forward)
/// pointing the way the player should face.
/// </summary>
public class SpawnPoint : MonoBehaviour
{
    [Tooltip("Unique name within the scene, e.g. Door1, Door2, CorridorStart")]
    public string id = "Door1";

    // Draws a marker in the Scene view so spawn points are easy to see and aim.
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.9f, 0.3f);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position + Vector3.up * 0.9f, transform.forward);
    }
}