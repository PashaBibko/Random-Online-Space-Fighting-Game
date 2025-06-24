using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

using System.Diagnostics;
using Unity.AI.Navigation;

public class LevelGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Spawner m_PlayerSpawner;
    [SerializeField] Camera m_Camera;
    [SerializeField] Text m_TimeTakenText;
    [SerializeField] NavMeshSurface m_NavMesh;

    private ValleyNode m_ValleyStart;

    private double m_Time1;
    private double m_Time2;

    private async void Start()
    {
        // Creates the level in the background //
        await SpawnLevelAsync();

        // Removes the gameobject that the camera is attached to //
        Destroy(m_Camera.gameObject);

        // Displays the time taken to the canvas //
        m_TimeTakenText.text = $"Time taken: {m_Time1}s | {m_Time2}s";
    }

    private async Task SpawnLevelAsync()
    {
        // Captures the start time //
        Stopwatch stopwatch = Stopwatch.StartNew();

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
                GameObject sectionPrefab = Resources.Load<GameObject>("Level/LevelSection");
                GameObject instance = GameObject.Instantiate(sectionPrefab, position, Quaternion.identity);
                instance.transform.SetParent(transform, true);
                LevelGroundSection sect = instance.GetComponent<LevelGroundSection>();
                sect.SetMesh(mesh);

                // Yeilds control back to the main thread to keep UI responsive //
                await Task.Yield();
            }
        }

        // Generates the enemy spawners (temporary) //
        Unity.Mathematics.Random rng = new Unity.Mathematics.Random(1);
        m_ValleyStart.CallFuncOnNodes((ValleyNode node) =>
        {
            // Does not spawn a spawner on the player spawn location //
            if (node == m_ValleyStart) { return; }

            // 1 in 10 chance to spawn a spawner //
            if (rng.NextInt(0, 5) != 1) { return; }

            // Raycasts to find the height at the given location //
            Vector3 rayStart = node.Position() * 200;
            rayStart.y = 5000;
            Physics.Raycast(rayStart, Vector3.down, out RaycastHit hitInfo, Mathf.Infinity);

            // Creates the enemy spawner prefab //
            GameObject prefab = Resources.Load<GameObject>("Level/EnemySpawner");
            GameObject.Instantiate(prefab, hitInfo.point, Quaternion.identity);
        });

        // Captures how long the world generation took //
        stopwatch.Stop();
        m_Time1 = stopwatch.Elapsed.TotalSeconds;
        stopwatch.Restart();

        // Creates the nav mesh //
        m_NavMesh.collectObjects = CollectObjects.All;
        m_NavMesh.BuildNavMesh();

        // Captures how long the nav mesh building took //
        stopwatch.Stop();
        m_Time2 = stopwatch.Elapsed.TotalSeconds;

        // Spawns the player after world generation finishes //
        Physics.Raycast(m_Camera.transform.position, Vector3.down, out RaycastHit hit, Mathf.Infinity);
        m_PlayerSpawner.RequestSpawn(hit.point + Vector3.up);
    }
}
