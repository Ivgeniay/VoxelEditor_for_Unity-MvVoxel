using System.Linq;
using UnityEditor;
using UnityEngine;
using static MvVox.VoxCreater;

namespace MvVox
{
    internal class ProjectionManager
    {
        private Projection _projection = new Projection();
        private VoxelStorage _voxelStorage;
        private VoxCreater _target;
        public ProjectionManager(VoxCreater voxCreater, VoxelStorage voxelStorage)
        {
            _target = voxCreater;
            _voxelStorage = voxelStorage;
        }

        public void DrawFromHoveredPosition(Vector3Int? hoveredPosition)
        {
            if (hoveredPosition.HasValue)
                Draw(hoveredPosition.Value);
        }

        public void Draw(Vector3Int gridPosition)
        {
            Vector3 worldPosition = _target.NetPosition + new Vector3(
                gridPosition.x * _target.VoxelSize,
                gridPosition.y * _target.VoxelSize,
                gridPosition.z * _target.VoxelSize
            );

            VoxelBound bound = _voxelStorage.GetVoxelBounds(gridPosition);
            Draw(bound);
        }

        public void Draw(VoxelBound bound)
        {
            _projection.Draw(bound.Center, Vector3.one * bound.Size);
        }

        public void Draw(Vector3 worldPosition)
        {
            _projection.Draw(worldPosition + Vector3.one * _target.VoxelSize * 0.5f, Vector3.one * _target.VoxelSize);
        }

        public void DrawOutlined()
        {
            foreach (VoxData item in _target.Voxels.Where(e => e.IsOutlined))
            {
                Draw(item.NetPosition);
            }
        }
    }
}
