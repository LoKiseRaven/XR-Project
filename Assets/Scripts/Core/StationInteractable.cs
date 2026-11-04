using UnityEngine;
using UnityEngine.SceneManagement;

public class StationInteractable : MonoBehaviour
{
    [Tooltip("Identifier for this station")]
    public string stationName;

    [Tooltip("Override room identifier. If left empty, uses current active Scene name.")]
    public string roomOverrideName;

    public void OnPlayerInteracted()
    {
        string roomName = string.IsNullOrEmpty(roomOverrideName)
            ? SceneManager.GetActiveScene().name
            : roomOverrideName;

        if (string.IsNullOrEmpty(stationName))
        {
            stationName = gameObject.name; // Fallback to GameObject name if empty
        }

        StationTracker.RecordInteraction(stationName, roomName);
    }
}