using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

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
    private static string sJoincode;

    private static bool sIsHost = false;
    private static bool sAuth = false;

    // Allows external classes to see how they should interact //
    public static TransferProtocol Protocol() => sProtocol;
    public static string Joincode() => sJoincode ?? "Not connected to server";
    public static bool IsHost() => sIsHost;

    // Creates a localhost transfer protocol //
    private static void CreateLocalHostTransferProtocol()
    {
        // Sets up the transfer protocol //
        GameObject transferPrefab = Resources.Load<GameObject>("TP/Localhost");
        GameObject transferInstance = GameObject.Instantiate(transferPrefab);
        transferInstance.AddComponent<GlobalSceneObject>();
        transferInstance.name = "Localhost-ClientTransferProtocol";

        // Local host does not require any authentication so clients and hosts can be created freely //
        if (sIsHost)
        {
            // Starts the server //
            NetworkManager.Singleton.StartHost();

            // Creates the client controller //
            GameObject clientControllerPrefab = Resources.Load<GameObject>("Network/ClientController");
            GameObject clientControllerInstance = GameObject.Instantiate(clientControllerPrefab);
            clientControllerInstance.GetComponent<NetworkObject>().Spawn();

            // Creates the client for the host //
            clientControllerInstance.GetComponent<ClientController>().CreateClient(0);
        }

        // Spawning clients does not require any additional logic //
        else { NetworkManager.Singleton.StartClient(); }
    }

    // Manages sending messages to Unity to setup the relay //
    private static async Task ManageUnityRelaySetup(string joincode)
    {
        // Authenticates the user if they have not already been //
        if (sAuth == false)
        {
            // Signs into UGS as an annoymous user //
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            sAuth = true;
        }

        if (sIsHost)
        {
            // Creates a server within 3 connections (allows 3 players + host) //
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

            // Gets the code to join the server //
            sJoincode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // Controls how data is transferred over the relay //
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData
            (
                (string)allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            // Creates the host //
            NetworkManager.Singleton.StartHost();

            // Creates the client controller //
            GameObject clientControllerPrefab = Resources.Load<GameObject>("Network/ClientController");
            GameObject clientControllerInstance = GameObject.Instantiate(clientControllerPrefab);
            clientControllerInstance.GetComponent<NetworkObject>().Spawn();

            // Creates the client for the host //
            clientControllerInstance.GetComponent<ClientController>().CreateClient(0);
        }

        else
        {
            // Joins the server with the given code //
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joincode);
            sJoincode = joincode;

            // Controls how data is transferred over the relay //
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData
            (
                (string)allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                allocation.HostConnectionData
            );

            // Tells the network manager to start as a client //
            NetworkManager.Singleton.StartClient();
        }
    }

    // Creates a relay transfer protocol via Unity //
    private static async void CreateRelayTransferProtocol(string joincode)
    {
        // Sets up the transfer protocol //
        GameObject transferPrefab = Resources.Load<GameObject>("TP/Relay");
        GameObject transferInstance = GameObject.Instantiate(transferPrefab);
        transferInstance.AddComponent<GlobalSceneObject>();
        transferInstance.name = "Relay-ClientTransferProtocol";

        // It now has to send messages to servers and wait for replys,   //
        // so it is put in a co-routine to not slow down the entire game //
        await ManageUnityRelaySetup(joincode);
    }

    // Initalises the state of how the client should interact with the host/server //
    public static void Init(TransferProtocol protocol, bool isHost, string joincode = "")
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
                CreateRelayTransferProtocol(joincode);
                break;
        }
    }
}
