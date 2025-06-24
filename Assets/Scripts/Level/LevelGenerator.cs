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
    [SerializeField] NavMeshSurface m_NavMesh;

    private ValleyNode m_ValleyStart;

    private async void Start()
    {
        // Creates the level in the background //
        await SpawnLevelAsync();

        // Removes the gameobject that the camera is attached to //
        Destroy(m_Camera.gameObject);
    }

    private async Task SpawnLevelAsync()
    {
        // Requests the spawning of the LevelManager (network object) //
        ServerController.HostRequestSpawn("Level/LevelManager", Vector3.zero);

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

        // Yeilds control back to the main thread to keep UI responsive //
        await Task.Yield();

        // Generates the enemy spawners (temporary) //
        EnemySpawner.Init();
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

        // Yeilds control back to the main thread to keep UI responsive //
        await Task.Yield();

        // Only the host needs to make the nav mesh as they are in control //
        if (OnlineState.IsHost())
        {
            // Creates the nav mesh //
            m_NavMesh.collectObjects = CollectObjects.All;
            m_NavMesh.layerMask = LayerMask.GetMask("NavMeshSurface");
            m_NavMesh.BuildNavMesh();

            // Yeilds control back to the main thread to keep UI responsive //
            await Task.Yield();
        }

        // Spawns the player after world generation finishes //
        Physics.Raycast(m_Camera.transform.position, Vector3.down, out RaycastHit hit, Mathf.Infinity);
        m_PlayerSpawner.RequestSpawn(hit.point + Vector3.up);

        // Yeilds control back to the main thread to keep UI responsive //
        await Task.Yield();

        // Starts the spawning loop in the LevelManager //
        LevelManager.StartSpawnLoop();
    }
}
