using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class LevelGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Spawner m_PlayerSpawner;
    [SerializeField] Camera m_Camera;
    [SerializeField] Text m_TimeTakenText;

    private ValleyNode m_ValleyStart;

    private async void Start()
    {
        // Captures the start time //
        float start = Time.time;

        // Creates the level in the background //
        await SpawnLevelAsync();

        // Removes the gameobject that the camera is attached to //
        Destroy(m_Camera.gameObject);

        // Calculates the time taken and applies it to the text //
        m_TimeTakenText.text = $"Time taken: {(Time.time - start)}s";
    }

    private async Task SpawnLevelAsync()
    {
        // Generates the valley nodes and calculates the bounding box //
        m_ValleyStart = new ValleyNode();
        m_ValleyStart.CalculateBoundingBox(out Vector2 min, out Vector2 max);

        // The settings that the mesh-factory uses //
        // Are supposed to be changeable but everything breaks when you change them atm //
        MeshFactory.MeshGenerationSettings settings = new
        (
            VertexCountPerSide: 100,
            DistBetweenVerticies: 2
        );

        // Loads the levelsection prefab outside of the loop so it is only loaded once //
        GameObject sectionPrefab = Resources.Load<GameObject>("Level/LevelSection");

        // Loops over all the sections and creates them //
        for (int x = (int)min.x; x <= (int)max.x; x++)
        {
            for (int z = (int)min.y; z <= (int)max.y; z++)
            {
                // Calculates the section's position in world space //
                Vector3 position = new
                (
                    x * settings._VertexCountPerSide * settings._DistBetweenVertecies, 0,
                    z * settings._VertexCountPerSide * settings._DistBetweenVertecies
                );

                // Generates the mesh in the background as that is the most computationaly expensive //
                MeshFactory.MeshGenData data = await Task.Run(() =>
                {
                    return MeshFactory.Create(settings, new Vector2(position.x, position.z), m_ValleyStart);
                });

                // Creates the mesh from the generated data //
                Mesh mesh = new()
                {
                    vertices = data.verticies,
                    triangles = data.triangles,
                    colors = data.colors
                };

                mesh.RecalculateBounds();
                mesh.RecalculateNormals();

                // Creates the section gameobject //
                GameObject instance = GameObject.Instantiate(sectionPrefab, position, Quaternion.identity);
                instance.transform.SetParent(transform, true);
                LevelGroundSection sect = instance.GetComponent<LevelGroundSection>();
                sect.SetMesh(mesh);

                // Yeilds control back to the main thread to keep UI responsive //
                await Task.Yield();
            }
        }

        // Spawns the player after world generation finishes //
        Physics.Raycast(m_Camera.transform.position, Vector3.down, out RaycastHit hit, Mathf.Infinity);
        m_PlayerSpawner.RequestSpawn(hit.point + Vector3.up);
    }
}
