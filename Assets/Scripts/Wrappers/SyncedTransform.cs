using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

// Wrapper to allow the client to be able to control the positions of transforms //
[DisallowMultipleComponent, RequireComponent(typeof(NetworkObject))] public class SyncedTransform : NetworkTransform
{
    // Tells the script it is a NON authoritive server which allows free changing of transforms //
    protected override bool OnIsServerAuthoritative() => false;
}
