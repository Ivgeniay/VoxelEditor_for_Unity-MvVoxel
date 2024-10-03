using UnityEngine;

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

        private void Draw(VoxelBound bound)
        {
            _projection.Draw(bound.Center, Vector3.one * bound.Size);
        }
    }
}
