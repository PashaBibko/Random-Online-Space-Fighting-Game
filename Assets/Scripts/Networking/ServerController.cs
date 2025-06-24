using Unity.Netcode;
using UnityEngine;

public class ServerController : NetworkBehaviour
{
    // The local instance of the server controller //
    private static ServerController sInstance = null;

    // Allows external scripts to read variables contained within //

    public static int Playercount()
    {
        // If no instance is found yet returns 0 //
        if (sInstance == null) { return 0; }

        // Else returns the actual player count //
        return sInstance.mPlayerCount.Value;
    }

    public static bool HostRequestSpawn(string prefabName, Vector3 location)
    {
        // Returns false if there is no instance of the server controller //
        if (sInstance == null) { return false; }

        // Returns false if the client is not the host //
        if (sInstance.IsHost == false) { return false; }

        // Loads the prefab and makes sure it is valid //
        GameObject prefab = Resources.Load<GameObject>(prefabName);
        if (prefab == null)
        {
            Debug.LogError($"Invalid prefab [{prefabName}] passed to ServerController.HostRequestSpawn - prefab does not exist");
            return false;
        }

        // Creates the prefab and locates the NetworkObject //
        GameObject instance = GameObject.Instantiate(prefab, location, Quaternion.identity);
        if (!instance.TryGetComponent<NetworkObject>(out var net))
        {
            Debug.LogError($"Invalid prefab [{prefabName}] passed to ServerController.HostRequestSpawn - prefab does not have a [Network Object] component");
            return false;
        }

        // Spawns the object on the network and returns true //
        net.Spawn();
        return true;
    }

    public static void InitaliseOnNetwork(GameObject obj)
    {
        // Finds the NetworkObject if it has one //
        if (!obj.TryGetComponent<NetworkObject>(out var net))
        {
            Debug.LogError($"GameObject [{obj.name}] passed to ServerController.InitaliseOnNetwork does not havea Network Object component");
            return;
        }

        // Creates it on the network //
        net.Spawn();
    }

    // Tracks how many players are connected (shared between clients) //
    public NetworkVariable<int> mPlayerCount = new
    (
        0,                                      // <- Default value
        NetworkVariableReadPermission.Everyone, // <- Allows all clients to read
        NetworkVariableWritePermission.Server   // <- Only server is allowed to write
    );

    public void Awake()
    {
        // Assigns itself to the local instance //
        sInstance = this;
    }

    public void Init()
    {
        // The player instance that creates the server controller does not trigger the listeners so has to be manually called //
        mPlayerCount.Value = 1;

        // Adds listeners to detect when players connect/disconnect //
        NetworkManager.OnClientConnectedCallback += (ulong _) => { mPlayerCount.Value++; };
        NetworkManager.OnClientDisconnectCallback += (ulong _) => { mPlayerCount.Value--; };
    }
}
