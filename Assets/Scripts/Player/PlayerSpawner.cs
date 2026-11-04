using UnityEngine;

/// On scene start, moves the player to the requested SpawnPoint  (SceneTransition.TargetSpawnId)
public class PlayerSpawner : MonoBehaviour
{
    private void Start()
    {
        if (string.IsNullOrEmpty(SceneTransition.TargetSpawnId)) return;

        foreach (SpawnPoint sp in FindObjectsOfType<SpawnPoint>())
        {
            if (sp.id != SceneTransition.TargetSpawnId) continue;
            CharacterController cc = GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            transform.SetPositionAndRotation(sp.transform.position, sp.transform.rotation);

            if (cc != null) cc.enabled = true;
            break;
        }

        SceneTransition.TargetSpawnId = "";
    }
}