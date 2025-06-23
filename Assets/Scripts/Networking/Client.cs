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
            GameObject controllerPrefab = Resources.Load<GameObject>("Network/ServerController");
            GameObject controllerInstance = GameObject.Instantiate(controllerPrefab);

            // Initalises it with the network correctly //
            controllerInstance.GetComponent<NetworkObject>().Spawn();
            DontDestroyOnLoad(controllerInstance);

            // Runs the init() function of the ServerController to finish initalistion //
            controllerInstance.GetComponent<ServerController>().Init();
        }
    }

    // Fowards the spawn request to the server as they are the only ones able to spawn //
    public static bool SpawnNetworkGameObject(string prefab, Vector3 location)
    {
        // Stops access of the client if it has not been created fully //
        if (s_LocalInstance == null) { return false; }

        // Calls the server and returns a sucess //
        s_LocalInstance.SpawnNetworkGameObject_ServerRPC(prefab, s_LocalInstance.OwnerClientId, location);
        return true;
    }

    [ServerRpc(RequireOwnership = false)] private void SpawnNetworkGameObject_ServerRPC(string name, ulong owner, Vector3 location)
    {
        // Loads the prefab and makes sure it is valid //
        GameObject prefab = Resources.Load<GameObject>(name);
        if (prefab == null)
        {
            Debug.LogError($"Invalid prefab [{name}] passed to Client.SpawnNetworkGameObject - prefab does not exist");
        }

        // Creates the prefab and locates the NetworkObject //
        GameObject instance = GameObject.Instantiate(prefab, location, Quaternion.identity);
        if (!instance.TryGetComponent<NetworkObject>(out var net))
        {
            Debug.LogError($"Invalid prefab [{name}] passed to Client.SpawnNetworkGameObject - prefab does not have a [Network Object] component");
        }

        // Initalises the network object as owned by the client that called for it's creation //
        net.SpawnAsPlayerObject(owner, true);
    }
}
