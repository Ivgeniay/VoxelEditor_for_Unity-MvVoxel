using UnityEngine;
using UnityEditor;
using System.Linq;

internal class ShaderFinder
{
    public static Material CreateMaterialFromShader(string shaderName)
    {
        Shader shader = FindShader(shaderName);
        if (shader != null)
        {
            Material material = new Material(shader);
            material.name = "Material_" + shaderName;
            material.SetColor("_EmissionColor", Color.white);
            return material;
        }
        else
        {
            Debug.LogError($"Shader '{shaderName}' not found in the project. Available shaders: {string.Join(", ", GetAllShaderNames())}");
            return null;
        }
    }

    private static Shader FindShader(string shaderName)
    {
        Shader shader = Shader.Find(shaderName);
        if (shader != null)
            return shader;

        string[] guids = AssetDatabase.FindAssets("t:Shader");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
            if (shader != null && shader.name.Contains(shaderName, System.StringComparison.OrdinalIgnoreCase))
            {
                return shader;
            }
        }

        Debug.LogWarning($"Shader with exact name '{shaderName}' not found. Using '{shader.name}' instead.");
        return null;
    }

    private static string[] GetAllShaderNames()
    {
        return AssetDatabase.FindAssets("t:Shader")
            .Select(guid => AssetDatabase.LoadAssetAtPath<Shader>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(shader => shader != null)
            .Select(shader => shader.name)
            .ToArray();
    }
}
