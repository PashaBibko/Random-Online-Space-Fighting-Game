using UnityEngine;

public class PlayerGun : MonoBehaviour
{
    [SerializeField] Transform m_MuzzleLocation;

    public Vector3 MuzzleLocation() => m_MuzzleLocation.position;
}
