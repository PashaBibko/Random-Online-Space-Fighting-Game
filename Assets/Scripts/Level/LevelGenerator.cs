using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    void Start()
    {
        MeshFactory.MeshGenerationSettings settings = new
        (
            VertexCountPerSide:     100,
            DistBetweenVerticies:   2
        );

        GameObject sectionPrefab = Resources.Load<GameObject>("Level/LevelSection");

        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                Vector3 position = new
                (
                    x * settings._VertexCountPerSide * settings._DistBetweenVertecies, 0,
                    z * settings._VertexCountPerSide * settings._DistBetweenVertecies
                );

                GameObject instance = GameObject.Instantiate(sectionPrefab, position, Quaternion.identity);

                LevelGroundSection sect = instance.GetComponent<LevelGroundSection>();
                sect.SetMesh(MeshFactory.Create(settings, new Vector2(position.x, position.z)));
            }
        }
    }
}
