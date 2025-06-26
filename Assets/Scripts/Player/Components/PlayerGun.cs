using UnityEngine;

public class PlayerGun : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] PlayerGlobalState.PlayerClass m_OwnerClass;
    [SerializeField] float m_Firerate;
    [SerializeField, Range(0.05f, 0.3f)] float m_MuzzleFlashDuration;

    [Header("References")]
    [SerializeField] GameObject m_MuzzleFlash;

    // The last time a shot from the gun was fired //
    float m_LastShot = Mathf.NegativeInfinity;
    public PlayerGlobalState.PlayerClass OwnerClass() => m_OwnerClass;

    public void ShootGun(Vector3 playerPos, Vector3 rayDir)
    {
        // Checks if the player can fire the gun yet //
        float minDif = 1f / m_Firerate;
        if (minDif < Time.time - m_LastShot)
        {
            m_LastShot = Time.time;

            // Performs a raycast to see what they are looking at //
            if (Physics.Raycast(playerPos, rayDir, out RaycastHit info, Mathf.Infinity))
            {
                // Checks if it hit an enemy //
                if (info.collider.CompareTag("Enemy"))
                {
                    Enemy.KillEnemy(info.collider.transform.parent.gameObject);
                }
            }

            // Enables the muzzle flash //
            m_MuzzleFlash.SetActive(true);
        }
    }

    private void LateUpdate()
    {
        // Disables the muzzle flash if it has been long enough since it was fired //
        if (m_MuzzleFlash.activeInHierarchy)
        {
            m_MuzzleFlash.SetActive(Time.time < m_LastShot + m_MuzzleFlashDuration);
        }
    }
}
