using Unity.Netcode;
using UnityEngine;

public class ClientController : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        // Only initalises if it is the server instance //
        if (IsServer)
        {
            // Adds a listener to be called on every client connection //
            NetworkManager.OnClientConnectedCallback += CreateClient;
        }
    }

    public void CreateClient(ulong ID)
    {
        // Creates the prefab //
        GameObject clientPrefab = Resources.Load<GameObject>("Network/Client");
        GameObject clientInstance = GameObject.Instantiate(clientPrefab);

        // Assigns it to the client's ID //
        clientInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(ID);
    }
}
