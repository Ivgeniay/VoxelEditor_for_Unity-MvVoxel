using UnityEngine;
using static MvVox.VoxCreater;

namespace MvVox
{
    public class VoxelInteractionHandler
    {
        private VoxCreater _target;
        private VoxelStorage _voxelStorage; 

        private VoxData currentVoxel;
        private BrushMode currentBrushMode;

        internal VoxelInteractionHandler(VoxCreater target, VoxelStorage voxelStorage)
        {
            _target = target;
            _voxelStorage = voxelStorage; 
        }

        public void HandleHoweredVoxel(Vector3Int? nullable)
        {
            if (nullable.HasValue)
                currentVoxel = _voxelStorage.GetVoxelFromNetPosition(nullable.Value);
        }

        public void HandleMouseDown(Vector2 vector)
        {
            if (currentVoxel != null)
            {
                if (currentBrushMode == BrushMode.Paint)
                {
                    _voxelStorage.AddVoxel(currentVoxel);
                }
                else if (currentBrushMode == BrushMode.Erase)
                {
                    _voxelStorage.RemoveVoxel(currentVoxel);
                } 
                else if (currentBrushMode == BrushMode.Pippette)
                {
                    _target.DrawColor = currentVoxel.color;
                }
            } 
        }

        public void OnBrushModeChanged(BrushMode newBrushMode) =>
            currentBrushMode = newBrushMode;
        
    }
}