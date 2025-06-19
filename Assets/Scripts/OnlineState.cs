using Unity.Netcode;

using UnityEngine;

public class GlobalSceneObject : MonoBehaviour
{
    private void Start() => DontDestroyOnLoad(gameObject);
}

public static class OnlineState
{
    // How data is transferred between clients //
    public enum TransferProtocol
    {
        // Uses Unity game services for servers //
        RELAY,

        // Transfers data over the local network //
        LOCALHOST
    }

    // The online state of the client //
    private static TransferProtocol sProtocol;
    private static bool sIsHost;

    // Allows external classes to see how they should interact //
    public static TransferProtocol Protocol() => sProtocol;
    public static bool IsHost() => sIsHost;

    // Creates a localhost transfer protocol //
    private static void CreateLocalHostTransferProtocol()
    {
        // Sets up the transfer protocol //
        GameObject transferPrefab = Resources.Load<GameObject>("TP/Localhost");
        GameObject transferInstance = GameObject.Instantiate(transferPrefab);
        transferInstance.AddComponent<GlobalSceneObject>();

        // Local host does not require any authentication so clients and hosts can be created freely //
        if (sIsHost)
            { NetworkManager.Singleton.StartHost(); }

        else
            { NetworkManager.Singleton.StartClient(); }
    }

    // Creates a relay transfer protocol via Unity //
    private static void CreateRelayTransferProtocol()
    {
        // I CBA atm to set this up AGAIN //
        throw new System.NotImplementedException();
    }

    // Initalises the state of how the client should interact with the host/server //
    public static void Init(TransferProtocol protocol, bool isHost)
    {
        // Sets the classes members //
        sProtocol = protocol;
        sIsHost = isHost;

        // Initalises the Transfer Protocol //
        switch (sProtocol) // <- Switch to support more modes later on
        {
            case TransferProtocol.LOCALHOST:
                CreateLocalHostTransferProtocol();
                break;

            case TransferProtocol.RELAY:
                CreateRelayTransferProtocol();
                break;
        }
    }
}
