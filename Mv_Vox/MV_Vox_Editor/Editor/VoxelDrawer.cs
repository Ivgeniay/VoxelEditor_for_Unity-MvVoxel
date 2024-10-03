using UnityEditor;
using static MvVox.VoxCreater;

namespace MvVox
{
    internal class VoxelDrawer
    {
        private VoxelStorage _voxStorage;
        public VoxelDrawer(VoxelStorage voxStorage)
        {
            _voxStorage = voxStorage;
        }

        public void DrawVoxels(float voxelSize)
        {
            var _sortedVoxels = _voxStorage.SortVoxelsRelativeCameraPosition(SceneView.lastActiveSceneView.camera.transform.position);

            foreach (VoxData voxel in _sortedVoxels)
            {
                if (voxel.IsFilled)
                {
                    voxel.Draw(); 
                }
            }
        }
    }
}
