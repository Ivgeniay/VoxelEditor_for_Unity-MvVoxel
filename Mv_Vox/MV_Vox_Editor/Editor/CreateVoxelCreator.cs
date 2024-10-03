using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;

namespace MvVox
{
    public class VoxelCreatorMenu : Editor
    {
        [MenuItem("GameObject/3D Object/Voxel Creator", false, 10)]
        static void CreateVoxelCreator(MenuCommand menuCommand)
        { 
            GameObject go = new GameObject("Voxel Creator"); 
            go.AddComponent<VoxCreater>(); 
            Undo.RegisterCreatedObjectUndo(go, "Create Voxel Creator"); 
            Selection.activeObject = go; 
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject); 
            if (!Application.isPlaying)
                EditorSceneManager.MarkSceneDirty(go.scene);
        }
    }
}
