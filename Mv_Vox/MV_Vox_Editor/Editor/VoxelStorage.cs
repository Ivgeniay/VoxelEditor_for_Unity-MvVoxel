using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static MvVox.VoxCreater;

namespace MvVox
{
    internal class VoxelStorage
    {
        private VoxCreater _target;
        public float VoxelSize => _target.VoxelSize;    
        public VoxelStorage(VoxCreater vocexCreater)
        {
            this._target = vocexCreater;
        }

        public IEnumerable<VoxData> GetVoxels()
        {
            return _target.Voxels;
        }

        public void GetVoxelWoldPositionFromGrid(Vector3Int gridPosition)
        {
            Vector3 worldPosition = _target.NetPosition + new Vector3(
                gridPosition.x * _target.VoxelSize,
                gridPosition.y * _target.VoxelSize,
                gridPosition.z * _target.VoxelSize
            ); 
        }

        public VoxelBound GetVoxelBounds(Vector3Int gridPosition)
        {
            try
            { 
                var voxel = _target.Voxels.First(e => e.NetPosition == gridPosition);
                return new VoxelBound()
                {
                    Center = voxel.Center,
                    Size = _target.VoxelSize,
                }; 
            }
            catch
            {
                return new VoxelBound() {  Center = Vector3.zero, Size = 0.01f};
            }
        }

        public IEnumerable<VoxData> SortVoxelsRelativeCameraPosition(Vector3 cameraPosition)
        {
            var sort = GetVoxels()
                        .Where(e => e.IsFilled)
                        .ToList();

            sort.Sort((a, b) =>
                        Vector3.SqrMagnitude(b.Center - cameraPosition).CompareTo(
                        Vector3.SqrMagnitude(a.Center - cameraPosition)));
            return sort;
        }

        public void AddVoxel(VoxData vox)
        {
            Undo.RecordObject(_target, "Add Voxel");
            vox.IsFilled = true;
            vox.color = _target.DrawColor;
            EditorUtility.SetDirty(_target);
        }
        public void AddVoxel(Vector3Int position)
        {
            VoxData existingVoxel = _target.Voxels.Find(v => v.NetPosition == position);
            AddVoxel(existingVoxel);
        }

        public void RemoveVoxel(VoxData vox)
        {
            Undo.RecordObject(_target, "Remove Voxel");
            vox.IsFilled = false;
            EditorUtility.SetDirty(_target);
        }
        public void RemoveVoxel(Vector3Int position)
        {
            VoxData existingVoxel = _target.Voxels.Find(v => v.NetPosition == position);
            RemoveVoxel(existingVoxel);
        }

        internal VoxData GetVoxelFromNetPosition(Vector3Int nullable)
        {
            return _target.Voxels.Find(v => v.NetPosition == nullable);
        }
    }

    public struct VoxelBound
    {
        public Vector3 Center;
        public float Size;
    }
}
