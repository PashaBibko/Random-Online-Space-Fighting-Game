using UnityEngine;
using UnityEditor;

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
            m_Target.Generate();
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
