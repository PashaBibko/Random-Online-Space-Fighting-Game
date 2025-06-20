using UnityEngine;

public class Player : ClientControlled
{
    [Header("Internal references")]
    [SerializeField] Rigidbody m_Body;
    [SerializeField] Transform m_Orientation;

    PlayerCamera m_Camera = null;

    Vector3 m_MoveDir;
    Vector2 m_Input;

    public override void OnStart()
    {
        // Sets the camera to be it's child //
        m_Camera = PlayerCamera.Instance();
        m_Camera.transform.SetParent(transform, false);
    }

    public override void OnUpdate()
    {
        // Updates user input //
        m_Input.x = Input.GetAxisRaw("Horizontal");
        m_Input.y = Input.GetAxisRaw("Vertical");

        // Updates the camera angle //
        Vector2 rot = m_Camera.Rotation();
        m_Orientation.rotation = Quaternion.Euler(0, rot.y, 0);
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
