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
}
