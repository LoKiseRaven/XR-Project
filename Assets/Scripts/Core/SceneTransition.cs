using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneTransition
{
    public static string TargetSpawnId = "";

    public static string LastRoomDoorId = "";

    public static void Go(string sceneName, string spawnId)
    {
        TargetSpawnId = spawnId;
        SceneManager.LoadScene(sceneName);
    }
}