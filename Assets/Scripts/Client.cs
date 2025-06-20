using UnityEngine;
using Unity.Netcode;

public class Client : NetworkBehaviour
{
    // The local instance of the client //
    private static Client s_LocalInstance = null;

    private static bool sSpawnedServerController = false;

    public override void OnNetworkSpawn()
    {
        // Set's itself to the local instance if owned by the client //
        if (IsOwner)
        {
            Debug.Log("Local client created");
            s_LocalInstance = this;
        }

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

    // Fowards the spawn request to the server as they are the only ones able to spawn //
    public static bool SpawnNetworkGameObject(string prefab)
    {
        // Stops access of the client if it has not been created fully //
        if (s_LocalInstance == null) { return false; }

        // Calls the server and returns a sucess //
        s_LocalInstance.SpawnNetworkGameObject_ServerRPC(prefab, s_LocalInstance.OwnerClientId);
        return true;
    }

    [ServerRpc(RequireOwnership = false)] private void SpawnNetworkGameObject_ServerRPC(string prefab, ulong owner)
    {
        Debug.Log($"Client-{owner} requested {prefab} to be spawned");
    }
}
