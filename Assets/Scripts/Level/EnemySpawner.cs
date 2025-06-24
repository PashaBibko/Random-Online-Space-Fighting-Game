using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    private static List<EnemySpawner> s_Spawners;

    // Adds itself to the list of instances //
    private void Start() => s_Spawners.Add(this);

    // Resets the list of the spawners //
    public static void Init() => s_Spawners = new List<EnemySpawner>();

    public static void SpawnAll()
    {
        // Loads the enemy prefab //
        GameObject prefab = Resources.Load<GameObject>("Enemy");
        Debug.Log("SPAWNING");

        // Spawns an enemy at each spawner //
        foreach (EnemySpawner spawner in s_Spawners)
        {
            // Spawns the enemy and initalises it on the network //
            Vector3 position = spawner.transform.position + Vector3.up;
            GameObject instance = GameObject.Instantiate(prefab, position, Quaternion.identity);
            ServerController.InitaliseOnNetwork(instance);
        }
    }
}
