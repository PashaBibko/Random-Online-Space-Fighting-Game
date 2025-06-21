using System.Collections;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    private ValleyNode m_ValleyStart;

    void Start()
    {
        StartCoroutine(SpawnLevel());
    }

    private IEnumerator SpawnLevel()
    {
        // Generates the valley nodes and calculates the bounding box //
        m_ValleyStart = new ValleyNode();
        m_ValleyStart.CalculateBoundingBox(out Vector2 min, out Vector2 max);
        Debug.Log($"{min} | {max}");

        MeshFactory.MeshGenerationSettings settings = new
        (
            VertexCountPerSide: 100,
            DistBetweenVerticies: 2
        );

        GameObject sectionPrefab = Resources.Load<GameObject>("Level/LevelSection");

        for (int x = (int)min.x; x <= (int)max.x; x++)
        {
            for (int z = (int)min.y; z <= (int)max.y; z++)
            {
                Vector3 position = new
                (
                    x * settings._VertexCountPerSide * settings._DistBetweenVertecies, 0,
                    z * settings._VertexCountPerSide * settings._DistBetweenVertecies
                );

                GameObject instance = GameObject.Instantiate(sectionPrefab, position, Quaternion.identity);

                LevelGroundSection sect = instance.GetComponent<LevelGroundSection>();
                sect.SetMesh(MeshFactory.Create(settings, new Vector2(position.x, position.z), m_ValleyStart));

                yield return new WaitForFixedUpdate();
            }
        }
    }
}
