using System.Net.NetworkInformation;
using Unity.Netcode;
using UnityEngine;

public class ServerController : NetworkBehaviour
{
    public NetworkVariable<int> mPlayerCount = new
    (
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    private void Awake()
    {
        // Adds listeners to detect when players connect/disconnect //
        NetworkManager.OnClientConnectedCallback += (ulong _) => { mPlayerCount.Value++; };
        NetworkManager.OnClientDisconnectCallback += (ulong _) => { mPlayerCount.Value--; };
    }
}
