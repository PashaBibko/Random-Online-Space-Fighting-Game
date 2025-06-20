using UnityEngine;

public class GlobalSceneObject : MonoBehaviour
{
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        Destroy(this);
    }
}
