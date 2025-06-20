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
