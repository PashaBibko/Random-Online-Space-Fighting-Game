using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public partial class Player : ClientControlled
{
    [Header("Player sections")]
    [SerializeField] Transform m_Orientation;
    [SerializeField] Transform m_CameraHolder;

    [Header("Internal references")]
    [SerializeField] Rigidbody m_Body;

    // Network variables //

    public NetworkVariable<Vector2> m_Rotation = new
    (
        new Vector2(0, 0),                      // <- Default value
        NetworkVariableReadPermission.Everyone, // <- Allows other clients to read
        NetworkVariableWritePermission.Owner    // <- Only the owner client is able to write
    );

    public NetworkVariable<PlayerGlobalState.PlayerClass> m_PrimaryPlayerClass = new
    (
        PlayerGlobalState.PlayerClass.GUNNER,   // <- Default value
        NetworkVariableReadPermission.Everyone, // <- Allows other clients to read
        NetworkVariableWritePermission.Owner    // <- Only the owner client is able to write
    );

    // Local variables //

    PlayerGun m_ActiveGun;

    Vector2 m_LocalRot;

    Vector3 m_MoveDir;
    Vector2 m_Input;

    private GameObject GetClassGun()
    {
        switch (m_PrimaryPlayerClass.Value)
        {
            case PlayerGlobalState.PlayerClass.GUNNER:
                return Resources.Load<GameObject>("Guns/GunnerPrimary");

            case PlayerGlobalState.PlayerClass.SCOUT:
                return Resources.Load<GameObject>("Guns/ScoutPrimary");

            case PlayerGlobalState.PlayerClass.ENGIE:
                return Resources.Load<GameObject>("Guns/EngiePrimary");

            default:
                return null;
        }
    }

    private void SpawnGun()
    {
        // Finds the correct gun prefab for the player //
        GameObject gunPrefab = GetClassGun();
        if (gunPrefab == null)
        {
            Debug.LogError("Could not find gun prefab for player");
            return;
        }

        // Spawns it in the world as a child of the camera and retrives the needed info //
        GameObject gunInstance = GameObject.Instantiate(gunPrefab, m_CameraHolder);
        m_ActiveGun = gunInstance.GetComponent<PlayerGun>();
    }

    public override void OnStart()
    {
        // Creates a camera and audio listener as they cannot be shared in the prefab //
        m_CameraHolder.AddComponent<AudioListener>();
        m_CameraHolder.AddComponent<Camera>();

        // Sets the primary class according to what the PlayerGlobalState has //
        m_PrimaryPlayerClass.Value = PlayerGlobalState.PrimaryClass;
        SpawnGun(); // Then spawns the players gun

        // Spawns the player canvas //
        GameObject canvas = Resources.Load<GameObject>("PlayerCanvas");
        GameObject.Instantiate(canvas);

        // Locks the users mouse //
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public override void OnUpdate()
    {
        // Updates user keyboard input //
        m_Input.x = Input.GetAxisRaw("Horizontal");
        m_Input.y = Input.GetAxisRaw("Vertical");

        // Updates user mouse input //
        Vector2 diff = new
        (
            Input.GetAxisRaw("Mouse X") * Time.deltaTime * 600f,
            Input.GetAxisRaw("Mouse Y") * Time.deltaTime * 400f
        );

        // Applies the change in rotation //
        // Why is it like this, I have no idea but it works ig //
        m_LocalRot.x -= diff.y;
        m_LocalRot.y += diff.x;

        // Stops the rotation values for glitching the camera //
        m_LocalRot.x = Mathf.Clamp(m_LocalRot.x, -90f, 90f);
        m_LocalRot.y = m_LocalRot.y % 360;

        // Updates the camera angle //
        m_Rotation.Value = m_LocalRot;
        m_CameraHolder.rotation = Quaternion.Euler(m_LocalRot.x, m_LocalRot.y, 0);
        m_Orientation.rotation = Quaternion.Euler(0, m_Rotation.Value.y, 0);

        // Shoots the player gun if the player is pressing left click //
        // Done here instead of fixed update to feel more responsive //
        if (Input.GetMouseButton(0)) { ShootGun(); }
    }

    public override void OnForiegnUpdate()
    {
        // Transfers the netowrk value to the client //
        m_LocalRot = m_Rotation.Value;
        
        // Sets the orientation value //
        m_CameraHolder.rotation = Quaternion.Euler(m_LocalRot.x, m_LocalRot.y, 0);
        m_Orientation.rotation = Quaternion.Euler(0, m_Rotation.Value.y, 0);

        // Creates a gun for the player if they don't have one //
        if (m_ActiveGun == null) { SpawnGun(); }

        // Else checks if they have the incorrect weapon //
        else if (m_ActiveGun.OwnerClass() != m_PrimaryPlayerClass.Value)
        {
            // Destroys the old one before creating the new one //
            Destroy(m_ActiveGun.gameObject);
            SpawnGun();

            Debug.Log("Destroyed incorrect gun");
        }
    }

    public override void OnFixedUpdate()
    {
        // Shoots a ray downwards to find what the player is standing on //
        Physics.SphereCast(transform.position, 0.2f, Vector3.down, out RaycastHit hit);
        bool grounded = hit.distance < 1.05f;

        // Calculates the move direction //
        m_MoveDir = (m_Orientation.forward * m_Input.y) + (m_Orientation.right * m_Input.x);

        // Extra logic if the player is on the ground //
        if (grounded)
        {
            // Projects the move direction onto the ground to allow the player to better move on slopes //
            m_MoveDir = Vector3.ProjectOnPlane(m_MoveDir, hit.normal).normalized;

            // Checks if the player wants to jump //
            if (Input.GetKey(KeyCode.Space))
            {
                m_Body.AddForce(Vector3.up * 10, ForceMode.Impulse);
            }

            // Disables gravity to stop sliding on slopes //
            m_Body.useGravity = false;

            // Applies a "stick-to-floor" force //
            m_Body.AddForce(-hit.normal * 30f, ForceMode.Force);
        }

        else
        {
            // When falling down applies an extra downwards force to feel more responsive //
            if (m_Body.velocity.y < 0) { m_Body.AddForce(Vector3.down * 20, ForceMode.Force); }

            // Enables gravity as it is disabled to stop sliding on slopes //
            m_Body.useGravity = true;
        }

        // Adds the base movement force to the rigidbody //
        m_Body.AddForce(m_MoveDir.normalized * 50, ForceMode.Force);
    }
}
