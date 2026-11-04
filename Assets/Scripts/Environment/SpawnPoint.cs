using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Tooltip("Unique name within a scene")]
    public string id = "Door1";

    // Draws a marker in the scene view so spawn points are easy to see
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.9f, 0.3f);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position + Vector3.up * 0.9f, transform.forward);
    }
}