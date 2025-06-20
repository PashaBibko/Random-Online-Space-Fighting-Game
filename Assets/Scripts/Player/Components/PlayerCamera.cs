using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] Camera m_Camera;
    [SerializeField] AudioListener m_AudioListener;

    [Header("Settings")]
    [SerializeField] Vector2 m_Sensitivity;

    // The local instance of the camera //
    private static PlayerCamera s_Instance = null;

    // How much the user has rotated the camera //
    private Vector2 m_Rotation = Vector2.zero;
    public Vector2 Rotation() => m_Rotation;

    // Gets/Creates the instance //
    public static PlayerCamera Instance()
    {
        // Returns the instance if it exists //
        if (s_Instance != null) { return s_Instance; }

        // Locks the players mouse //
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Else it will have to create it //
        GameObject prefab = Resources.Load<GameObject>("PlayerCamera");
        GameObject instance = GameObject.Instantiate(prefab);

        // Assigns the instance and returns it //
        s_Instance = instance.GetComponent<PlayerCamera>();
        return s_Instance;
    }

    private void Update()
    {
        // Gets the mouse input from the player //
        Vector2 diff = new
        (
            Input.GetAxisRaw("Mouse X") * Time.deltaTime * m_Sensitivity.x,
            Input.GetAxisRaw("Mouse Y") * Time.deltaTime * m_Sensitivity.y
        );

        // Applies the difference to the rotation //
        // Yes these lines are correct even if they don't look like it //
        m_Rotation.x -= diff.y;
        m_Rotation.y += diff.x;

        // Clamps the X angle to stop it looping around //
        m_Rotation.x = Mathf.Clamp(m_Rotation.x, -90f, 90f);

        // Applies some of the rotation to itself //
        transform.rotation = Quaternion.Euler(m_Rotation.x, m_Rotation.y, 0);
    }
}
