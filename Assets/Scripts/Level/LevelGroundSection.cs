using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGroundSection : MonoBehaviour
{
    [Header("References")]
    [SerializeField] MeshCollider m_Collider;
    [SerializeField] MeshFilter m_Filter;

    public void SetMesh(Mesh mesh)
    {
        m_Collider.sharedMesh = mesh;
        m_Filter.sharedMesh = mesh;
    }
}
