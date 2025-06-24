using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class LevelManager : NetworkBehaviour
{
    // The instance of the level manager //
    private static LevelManager s_Instance;

    private void Start()
    {
        // Assigns itself as the instance //
        s_Instance = this;
    }

    public static bool StartSpawnLoop()
    {
        // Starts the co-routine loop on the instance //
        if (s_Instance.IsHost)
        {
            s_Instance.StartCoroutine(s_Instance.SpawnLoop());
            return true;
        }

        return false;
    }

    private IEnumerator SpawnLoop()
    {
        // Waits a random ammount of time before the next spawn //
        yield return new WaitForSeconds(UnityEngine.Random.Range(5f, 20f));

        EnemySpawner.SpawnAll();

        // Calls the next iteration of the loop //
        StartCoroutine(SpawnLoop());
    }
}
