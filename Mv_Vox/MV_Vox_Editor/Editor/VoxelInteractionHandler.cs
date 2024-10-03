using UnityEditor;
using UnityEngine;
using static MvVox.VoxCreater;

namespace MvVox
{
    public class VoxelInteractionHandler
    {
        private VoxCreater _target;
        private VoxelStorage _voxelStorage; 

        private VoxData currentVoxel;
        private Vector3Int netPosition;
        private BrushMode currentBrushMode;

        internal VoxelInteractionHandler(VoxCreater target, VoxelStorage voxelStorage)
        {
            _target = target;
            _voxelStorage = voxelStorage; 
        }

        public void HandleHoweredVoxel(Vector3Int? nullable)
        {
            if (nullable.HasValue)
            {
                currentVoxel = _voxelStorage.GetVoxelFromNetPosition(nullable.Value);
                netPosition = nullable.Value; 
            }
        }
        private bool IsValidNetPosition(Vector3Int netPos)
        {
            return  netPos.x <= _target.NetSize.x &&
                    netPos.y <= _target.NetSize.y &&
                    netPos.z <= _target.NetSize.z &&
                    netPos.x >= 0 &&
                    netPos.y >= 0 &&
                    netPos.z >= 0;
        }

        public void HandleMouseDown(Vector2 vector)
        { 
            if (IsValidNetPosition(netPosition))
            {
                if (currentBrushMode == BrushMode.Paint)
                {
                    _voxelStorage.AddVoxel(netPosition);
                }
                else if (currentBrushMode == BrushMode.Erase)
                {
                    _voxelStorage.RemoveVoxel(netPosition);
                } 
                else if (currentBrushMode == BrushMode.Pippette)
                {
                    Undo.RecordObject(_target, "Brush color changed");
                    _target.DrawColor = currentVoxel.color;
                    EditorUtility.SetDirty(_target);
                }
            } 
        }

        public void OnBrushModeChanged(BrushMode newBrushMode) =>
            currentBrushMode = newBrushMode;
        
    }
}