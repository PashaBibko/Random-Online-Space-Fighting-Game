using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.Collections.Generic;

[CustomEditor(typeof(TerrainGenerator))] public class LevelGenerateGUI : Editor
{
    TerrainGenerator m_Target;

    public override void OnInspectorGUI()
    {
        // Draws the default GUI //
        base.OnInspectorGUI();

        // Button for generating worlds in the inspector //
        if (GUILayout.Button("Generate World"))
        {
            // Gathers all previously generated children //
            List<GameObject> children = new();
            foreach (Transform child in m_Target.transform) { children.Add(child.gameObject); }

            // And destroys them //
            foreach (GameObject child in children) { GameObject.DestroyImmediate(child); }

            // Starts a stopwatch to time the generation //
            Stopwatch stopwatch = new();
            stopwatch.Start();

            // Generates the terrain //
            m_Target.Generate();

            // Stops the stopwatch and prints the elapsed time to the console //
            stopwatch.Stop();
            #pragma warning disable IDE0071 // Simplify interpolation
            UnityEngine.Debug.Log($"Generation took: [{stopwatch.Elapsed.TotalSeconds.ToString("F2")}s]");
            #pragma warning restore IDE0071 // Simplify interpolation
        }

        // Button for saving the currently generated world //
        if (GUILayout.Button("Save world"))
        {
            GameObject duplicate = GameObject.Instantiate(m_Target.gameObject, new Vector3(0, -500, 0), Quaternion.identity);
            TerrainGenerator instance = duplicate.GetComponent<TerrainGenerator>();
            GameObject.DestroyImmediate(instance);
        }
    }

    private void OnEnable()
    {
        m_Target = (TerrainGenerator)target;
        Tools.hidden = true;
    }

    private void OnDisable()
    {
        Tools.hidden = false;
    }
}
