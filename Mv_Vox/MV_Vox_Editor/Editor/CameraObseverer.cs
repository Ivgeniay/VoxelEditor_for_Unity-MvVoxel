using UnityEditor;
using UnityEngine;
using System;

namespace MvVox
{
    public delegate void CameraChangedHandler(Vector3 newPosition, Quaternion newRotation);

    public class CameraData
    {
        public Vector3 Position;
        public Quaternion Rotation; 
        public VisibleSide VisibleSides;
    }

    public class CameraObseverer
    {
        public event CameraChangedHandler OnCameraPositionChanged;
        public event Action<CameraData> OnCameraChanged;

        private VoxCreater _target;
        private CameraData cameraData;
        
        public CameraObseverer(VoxCreater target)
        {
            _target = target;
            cameraData = new CameraData();
        }

        public void Observe()
        {
            Camera sceneCamera = SceneView.lastActiveSceneView.camera;
            Vector3 currentPosition = sceneCamera.transform.position;
            Quaternion currentRotation = sceneCamera.transform.rotation;

            if (HasCameraChanged(currentPosition, currentRotation))
            {
                UpdateVisibility(currentPosition);
                cameraData.Position = currentPosition;
                cameraData.Rotation = currentRotation;
                 
                OnCameraPositionChanged?.Invoke(currentPosition, currentRotation);
                OnCameraChanged?.Invoke(cameraData);
            }
        }

        private bool HasCameraChanged(Vector3 currentPosition, Quaternion currentRotation)
        {
            return currentPosition != cameraData.Position || currentRotation != cameraData.Rotation;
        }

        private void UpdateVisibility(Vector3 cameraPosition)
        {
            Vector3 localCameraPos = cameraPosition - _target.NetPosition;
            Vector3 cubeSize = new Vector3(_target.NetSize.x, _target.NetSize.y, _target.NetSize.z) * _target.VoxelSize;
             
            cameraData.VisibleSides = VisibleSide.None;
             
            if (localCameraPos.z > 0)
                cameraData.VisibleSides |= VisibleSide.IsFront;
             
            if (localCameraPos.z < cubeSize.z)
                cameraData.VisibleSides |= VisibleSide.IsBack;
             
            if (localCameraPos.x > 0)
                cameraData.VisibleSides |= VisibleSide.IsLeft;
             
            if (localCameraPos.x < cubeSize.x)
                cameraData.VisibleSides |= VisibleSide.IsRight;
             
            if (localCameraPos.y < cubeSize.y)
                cameraData.VisibleSides |= VisibleSide.IsTop;
             
            if (localCameraPos.y > 0)
                cameraData.VisibleSides |= VisibleSide.IsBottom;

            //Debug.Log($"Camera position: {localCameraPos}, Cube size: {cubeSize}, Visible sides: {cameraData.VisibleSides}");
        }
    }


}
