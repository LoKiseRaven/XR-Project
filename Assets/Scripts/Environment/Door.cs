using UnityEngine;

// A door that sends the player to another scene.
public class Door : MonoBehaviour
{
    public enum DestinationMode
    {
        FixedSpawn,            // always go to a specific target
        ReturnToLastRoomDoor   // go to the room the player came from
    }

    [Header("This door")]
    [Tooltip("Only used by Room doors. must match a SpawnPoint id in the Room scene")]
    public string doorId = "Door1";
    [Tooltip("Tick for Room doors so the corridor's base door knows where to send the player back")]
    public bool recordAsLastRoomDoor = false;

    [Header("Destination")]
    public string targetScene = "Corridor";
    public DestinationMode mode = DestinationMode.FixedSpawn;
    [Tooltip("FixedSpawn in the target scene , or the fallback for ReturnToLastRoomDoor.")]
    public string targetSpawnId = "CorridorStart";

    [Header("Activation")]
    [Tooltip("Also trigger when the player walks into this door's trigger collider (needs 'Is Trigger' ticked)")]
    public bool activateOnTouch = false;

    private bool isLoading; // stops double-activation

    // Called by PlayerInteraction when the player clicks this door.
    public void Interact()
    {
        if (isLoading) return;
        isLoading = true;

        if (recordAsLastRoomDoor)
            SceneTransition.LastRoomDoorId = doorId;

        string spawn = targetSpawnId;
        if (mode == DestinationMode.ReturnToLastRoomDoor && !string.IsNullOrEmpty(SceneTransition.LastRoomDoorId))
            spawn = SceneTransition.LastRoomDoorId; // otherwise fall back to targetSpawnId

        SceneTransition.Go(targetScene, spawn);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activateOnTouch && other.CompareTag("Player"))
            Interact();
    }
}