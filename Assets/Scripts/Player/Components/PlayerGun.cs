using UnityEngine;

public class PlayerGun : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] PlayerGlobalState.PlayerClass m_OwnerClass;

    [Header("References")]
    [SerializeField] Transform m_MuzzleLocation;

    public Vector3 MuzzleLocation() => m_MuzzleLocation.position;
    public PlayerGlobalState.PlayerClass OwnerClass() => m_OwnerClass;
}
