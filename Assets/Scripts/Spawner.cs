using System.Collections;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] string m_PrefabName;

    // Calls the client to create the object //
    private IEnumerator AttemptSpawn(Vector3 location)
    {
        // False returns means that the client has not been fully created yet so the function waits until it is ready //
        while (Client.SpawnNetworkGameObject(m_PrefabName, location) == false) { yield return new WaitForFixedUpdate(); }
    }

    // Runs the corountine to spawn the object //
    public void RequestSpawn() => StartCoroutine(AttemptSpawn(Vector3.zero));

    public void RequestSpawn(Vector3 spawnLocation) => StartCoroutine(AttemptSpawn(spawnLocation));
}
