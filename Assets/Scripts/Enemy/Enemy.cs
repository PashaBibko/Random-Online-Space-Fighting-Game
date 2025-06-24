using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : NetworkBehaviour
{
    NavMeshAgent m_NavAgent;
    GameObject m_PlayerTarget;

    private void Start()
    {
        // Stops non-host objects controlling the enemy //
        if (IsHost)
        {
            // Makes sure only the host has the nav mesh agent //
            m_NavAgent = transform.parent.AddComponent<NavMeshAgent>();
            m_PlayerTarget = GameObject.FindGameObjectWithTag("Player");
        }
    }

    private void Update()
    {
        // Stops non-host objects controlling the enemy //
        if (IsHost)
        {
            m_NavAgent.SetDestination(m_PlayerTarget.transform.position);
        }
    }

    public static void KillEnemy(GameObject enemy)
    {
        // Gets the script of the enemy //
        Enemy component = enemy.GetComponentInChildren<Enemy>();
        if (component == null)
        {
            Debug.Log($"Object: {enemy.name} is not an enemy");
            return;
        }

        // Triggers the ServerRPC to kill it //
        component.KillEnemy_ServerRPC();
    }

    [ServerRpc(RequireOwnership = false)] private void KillEnemy_ServerRPC()
    {
        // Stops killing already dead objects //
        if (NetworkObject == null || !NetworkObject.IsSpawned) { return; }

        // Despawns the object on the network and destroys the game object (true param) //
        NetworkObject.Despawn(true);
    }
}
