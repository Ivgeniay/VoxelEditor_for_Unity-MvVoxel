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
        public VoxelStorage(VoxCreater vocexCreater)
        {
            this._target = vocexCreater;
        }

        public IEnumerable<VoxData> GetVoxels()
        {
            return _target.Voxels;
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
                var worldPosition = _target.NetPosition + new Vector3(gridPosition.x * _target.VoxelSize, gridPosition.y * _target.VoxelSize, gridPosition.z * _target.VoxelSize);
                var center = worldPosition + Vector3.one * _target.VoxelSize * 0.5f;
                return new VoxelBound() { Center = center, Size = _target.VoxelSize};
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

        public void AddVoxel(Vector3Int vox)
        {
            Undo.RecordObject(_target, "Add Voxel");
            VoxData voxData = new(_target)
            {
                IsFilled = true,
                NetPosition = vox,
                Position = _target.NetPosition + new Vector3(
                    vox.x * _target.VoxelSize,
                    vox.y * _target.VoxelSize,
                    vox.z * _target.VoxelSize),
                color = _target.DrawColor
            };
            _target.Voxels.Add(voxData);
            EditorUtility.SetDirty(_target);
        }

        public void RemoveVoxel(Vector3Int vox)
        {
            Undo.RecordObject(_target, "Remove Voxel");
            VoxData voxData = _target.Voxels.Find(v => v.NetPosition == vox);
            _target.Voxels.Remove(voxData);
            EditorUtility.SetDirty(_target);
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
