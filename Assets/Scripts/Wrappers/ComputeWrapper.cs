using UnityEngine;

using System;

[CreateAssetMenu(menuName = "Custom/ComputeWrapper")] public class ComputeWrapper : ScriptableObject
{
    [Serializable] public struct ShaderConstant
    {
        enum ConstantType
        {
            Int,
            Float
        }

        [SerializeField] string m_Name;
        [SerializeField] string m_Value;
        [SerializeField] ConstantType m_Type;

        public readonly void ApplyConstantTo(ComputeShader shader)
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
                Debug.LogError($"Failed to apply shader constant [{m_Name}] to compute shader. Error message: [{e.Message}]");
            }
        }
    }

    [Header("Settings")]
    [SerializeField] ComputeShader m_ComputeShader;
    [SerializeField] ShaderConstant[] m_ShaderConstants;

    public void LaunchShader(string kernelName, Vector2Int groups, ref Array output, int typeSize)
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
        ComputeBuffer buffer = new(output.Length, typeSize);
        m_ComputeShader.SetBuffer(kernelIndex, "ResultBuffer", buffer);

        // Launches the compute shader //
        m_ComputeShader.Dispatch(kernelIndex, groups.x, groups.y, 1);

        // Transfers the output data from the compute shader //
        buffer.GetData(output);
        buffer.Release();
    }
}
