using Unity.Netcode;
using UnityEngine;

public class ServerController : NetworkBehaviour
{
    public NetworkVariable<int> mPlayerCount = new
    (
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public void Init()
    {
        // Adds listeners to detect when players connect/disconnect //
        NetworkManager.OnClientConnectedCallback += (ulong _) => { mPlayerCount.Value++; };
        NetworkManager.OnClientDisconnectCallback += (ulong _) => { mPlayerCount.Value--; };
    }
}
