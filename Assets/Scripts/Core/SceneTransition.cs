using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Static helper that loads scenes and carries state across the load.
/// Static fields survive scene changes, so no DontDestroyOnLoad object is needed.
/// </summary>
public static class SceneTransition
{
    /// <summary>ID of the SpawnPoint the player should appear at in the next scene.</summary>
    public static string TargetSpawnId = "";

    /// <summary>ID of the last Room door the player walked through (used by the corridor's "back" door).</summary>
    public static string LastRoomDoorId = "";

    /// <summary>Remember the destination spawn point, then load the scene.</summary>
    public static void Go(string sceneName, string spawnId)
    {
        TargetSpawnId = spawnId;
        SceneManager.LoadScene(sceneName);
    }
}