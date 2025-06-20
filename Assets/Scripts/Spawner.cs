using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] string m_PrefabName;

    // Calls the client to create the object //
    private IEnumerator AttemptSpawn()
    {
        // False returns means that the client has not been fully created yet so the function waits until it is ready //
        while (Client.SpawnNetworkGameObject(m_PrefabName) == false) { yield return new WaitForFixedUpdate(); }
    }

    // Spawns the prefab provided //
    private void Start() => StartCoroutine(AttemptSpawn());
}
