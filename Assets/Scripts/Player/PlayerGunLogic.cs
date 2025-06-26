using Unity.Netcode;

public partial class Player : ClientControlled
{
    private void ShootGun()
    {
        // Attempts to shoot the gun on this client //
        if (m_ActiveGun.ShootGun(transform.position, m_CameraHolder.forward))
        {
            // Tells the server that the player has shot their gun //
            PlayerShot_ServerRPC();
        }
    }

    // Fowards the message to all clients //
    [ServerRpc(RequireOwnership = false)] private void PlayerShot_ServerRPC()
        => PlayerShot_ClientRPC();

    // Tells the client that this player has shot (message recived from network) //
    [ClientRpc(RequireOwnership = false)] private void PlayerShot_ClientRPC()
    {
        // If it was itself that sent the message returns early //
        if (IsOwner) { return; }

        // Else makes the gun appear like it is shooting //
        m_ActiveGun.VisualShoot();
    }
}
