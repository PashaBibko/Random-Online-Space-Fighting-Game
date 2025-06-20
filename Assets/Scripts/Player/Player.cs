using System.Collections;
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
        // Calculates the move direction //
        m_MoveDir = (m_Orientation.forward * m_Input.y) + (m_Orientation.right * m_Input.x);

        // Adds the force to the rigidbody //
        m_Body.AddForce(m_MoveDir.normalized * 50, ForceMode.Force);
    }
}
