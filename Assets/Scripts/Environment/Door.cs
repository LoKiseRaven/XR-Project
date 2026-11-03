using UnityEngine;

/// <summary>
/// A door that sends the player to another scene.
/// Can be triggered by clicking it (via DoorClicker) and/or by walking into it.
/// </summary>
public class Door : MonoBehaviour
{
    public enum DestinationMode
    {
        FixedSpawn,            // always go to Target Spawn Id
        ReturnToLastRoomDoor   // go to the room door the player originally left through
    }

    [Header("This door")]
    [Tooltip("Only used by Room doors, e.g. Door1 / Door2. Must match a SpawnPoint id in the Room scene.")]
    public string doorId = "Door1";
    [Tooltip("Tick for Room doors so the corridor's base door knows where to send the player back.")]
    public bool recordAsLastRoomDoor = false;

    [Header("Destination")]
    public string targetScene = "Corridor";
    public DestinationMode mode = DestinationMode.FixedSpawn;
    [Tooltip("SpawnPoint id in the target scene (FixedSpawn), or the fallback for ReturnToLastRoomDoor.")]
    public string targetSpawnId = "CorridorStart";

    [Header("Activation")]
    [Tooltip("Also trigger when the player walks into this door's trigger collider (needs 'Is Trigger' ticked, and the Player tagged 'Player').")]
    public bool activateOnTouch = false;

    private bool isLoading; // guards against double-activation

    /// <summary>Called by DoorClicker when the player clicks this door.</summary>
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