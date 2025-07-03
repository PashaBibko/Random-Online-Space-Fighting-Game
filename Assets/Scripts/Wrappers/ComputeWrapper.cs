using UnityEngine;

using System;

[CreateAssetMenu(menuName = "Custom/ComputeWrapper")] public class ComputeWrapper : ScriptableObject
{
    [Serializable] public class ShaderConstant
    {
        enum ConstantType
        {
            Int,
            Float
        }

        [SerializeField] public string m_Name;
        [SerializeField] public string m_Value;
        [SerializeField] ConstantType m_Type;

        public void ApplyConstantTo(ComputeShader shader)
        {
            // Catches errors thrown when setting shader constants //
            try
            {
                switch (m_Type)
                {
                    // Parses the constant value and applies it to the shader //
                    case ConstantType.Int:
                        shader.SetInt(m_Name, int.Parse(m_Value));
                        return;

                    // Parses the constant value and applies it to the shader //
                    case ConstantType.Float:
                        shader.SetFloat(m_Name, float.Parse(m_Value));
                        return;
                }
            }

            // Prints the error message to the console //
            catch (Exception e)
            {
                Debug.LogError($"Failed to apply shader constant [{m_Name}] to compute shader with value [{m_Value}]. Error message: [{e.Message}]");
            }
        }
    }

    [Header("Settings")]
    [SerializeField] ComputeShader m_ComputeShader;
    [SerializeField] ShaderConstant[] m_ShaderConstants;

    public void SetShaderConstant<T>(string name, T value)
    {
        // Checks the value was not null before turning it into a string //
        if (value == null) { Debug.LogError("Value passed was null"); }
        SetShaderConstant(name, value.ToString());
    }

    public void SetShaderConstant(string name, string value)
    {
        foreach (ShaderConstant constant in m_ShaderConstants)
        {
            if (constant.m_Name == name)
            {
                constant.m_Value = value;
                return;
            }
        }

        Debug.LogError($"Shader constant [{name}] does not exist");
    }

    public void LaunchShader(string kernelName, Vector2Int batches, ComputeBuffer buffer)
    {
        // Finds the kernel index by name //
        int kernelIndex = m_ComputeShader.FindKernel(kernelName);

        // Applies all shader constants to the compute shader //
        foreach (ShaderConstant constant in m_ShaderConstants)
        {
            // Applies the shader constant to the compute shader //
            constant.ApplyConstantTo(m_ComputeShader);
        }

        // Prepares the output buffer of the compute shader //
        m_ComputeShader.SetBuffer(kernelIndex, "ResultBuffer", buffer);

        // Launches the compute shader //
        m_ComputeShader.Dispatch(kernelIndex, batches.x, batches.y, 1);
    }
}
