using UnityEngine;
using Unity.Netcode;

public class Client : NetworkBehaviour
{
    private static bool sSpawnedServerController = false;

    public override void OnNetworkSpawn()
    {
        // Sets it's name to have it's client ID (for easier recognition) //
        transform.name = "Client-" + OwnerClientId;

        // If it is the host it needs to spawn the server controller so everything functions correctly //
        if (IsHost && sSpawnedServerController == false)
        {
            // Sets the updated state of wether it has been spawned //
            sSpawnedServerController = true;

            // Spawns the server controller within the world //
            GameObject controllerPrefab = Resources.Load<GameObject>("ServerController");
            GameObject controllerInstance = GameObject.Instantiate(controllerPrefab);

            // Initalises it with the network correctly //
            controllerInstance.GetComponent<NetworkObject>().Spawn();
            DontDestroyOnLoad(controllerInstance);

            // Runs the init() function of the ServerController to finish initalistion //
            controllerInstance.GetComponent<ServerController>().Init();
        }
    }
}
