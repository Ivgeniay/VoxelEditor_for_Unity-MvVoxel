using UnityEditor;
using UnityEngine; 

namespace MvVox
{
    public class NetDrawer
    {
        private VoxCreater _target;
        private CameraData _camObserver;

        public NetDrawer(VoxCreater target)
        {
            _target = target;
        }

        public void UpdateCameraData(CameraData _camObserver)
        {
            this._camObserver = _camObserver;
        }

        public void DrawInsideCubeFaces()
        {
            if (_camObserver == null) return;

            Color innerColor = Handles.color;
            Handles.color = _target.NetDrawColor;

            Vector3 position = _target.NetPosition;
            Vector3Int netSize = _target.NetSize;
            float voxelSize = _target.VoxelSize;
            Vector3 cubeSize = new Vector3(netSize.x * voxelSize, netSize.y * voxelSize, netSize.z * voxelSize);

            if (_camObserver.VisibleSides.HasFlag(VisibleSide.IsFront))
                DrawFace(position, cubeSize, Vector3.right, Vector3.up);
            if (_camObserver.VisibleSides.HasFlag(VisibleSide.IsBack))
                DrawFace(position + Vector3.forward * cubeSize.z, cubeSize, Vector3.right, Vector3.up);
            if (_camObserver.VisibleSides.HasFlag(VisibleSide.IsLeft))
                DrawFace(position, cubeSize, Vector3.forward, Vector3.up);
            if (_camObserver.VisibleSides.HasFlag(VisibleSide.IsRight))
                DrawFace(position + Vector3.right * cubeSize.x, cubeSize, Vector3.forward, Vector3.up);
            if (_camObserver.VisibleSides.HasFlag(VisibleSide.IsTop))
                DrawFace(position + Vector3.up * cubeSize.y, cubeSize, Vector3.right, Vector3.forward);
            if (_camObserver.VisibleSides.HasFlag(VisibleSide.IsBottom))
                DrawFace(position, cubeSize, Vector3.right, Vector3.forward);

            Handles.color = innerColor;
        }

        private void DrawFace(Vector3 origin, Vector3 size, Vector3 right, Vector3 up)
        {
            Vector3 rightSize = Vector3.Scale(right, size);
            Vector3 upSize = Vector3.Scale(up, size);

            Vector3 p0 = origin;
            Vector3 p1 = origin + rightSize;
            Vector3 p2 = origin + rightSize + upSize;
            Vector3 p3 = origin + upSize;

            Handles.DrawLine(p0, p1);
            Handles.DrawLine(p1, p2);
            Handles.DrawLine(p2, p3);
            Handles.DrawLine(p3, p0);
        }
    }
}