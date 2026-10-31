using UnityEngine;

/// <summary>
/// On scene start, moves the player to the SpawnPoint requested by the
/// previous scene (SceneTransition.TargetSpawnId). If none was requested
/// or none matches, the player stays where it was placed in the editor.
/// </summary>
public class PlayerSpawner : MonoBehaviour
{
    private void Start()
    {
        if (string.IsNullOrEmpty(SceneTransition.TargetSpawnId)) return;

        foreach (SpawnPoint sp in FindObjectsOfType<SpawnPoint>())
        {
            if (sp.id != SceneTransition.TargetSpawnId) continue;

            // A CharacterController overrides transform changes while enabled,
            // so switch it off for the teleport.
            CharacterController cc = GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            transform.SetPositionAndRotation(sp.transform.position, sp.transform.rotation);

            if (cc != null) cc.enabled = true;
            break;
        }

        SceneTransition.TargetSpawnId = ""; // consumed
    }
}