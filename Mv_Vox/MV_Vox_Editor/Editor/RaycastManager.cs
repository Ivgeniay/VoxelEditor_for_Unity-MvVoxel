using System.Collections.Generic;
using UnityEditor;
using UnityEngine; 
using System;
using static MvVox.VoxCreater;

namespace MvVox
{
    internal class RaycastManager
    {
        public event Action<Vector3Int?> OnHoweredVoxel;

        private BrushMode currentBrushMode;
        private IEnumerable<PlaneModel> _cachedVisiblePlanes;
        private VoxCreater _target; 

        public RaycastManager(VoxCreater vox)
        {
            _target = vox;
        }

        public void HandleInteraction(Vector2 mousePosition)
        {
            if (currentBrushMode == BrushMode.None) return;

            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition); 
            var (voxelHit, distance) = CheckVoxelIntersection(ray);

            if (currentBrushMode == BrushMode.Paint)
            {
                if (voxelHit.HasValue)
                { 
                    Vector3Int placementPosition = GetPlacementPosition(voxelHit.Value, ray);
                    if (IsInsideWorkArea(placementPosition))
                    {
                        OnHoweredVoxel?.Invoke(placementPosition);
                    }
                }
                else
                {
                    Vector3Int? hoveredPosition = GetHoveredGridPosition(ray);
                    if (hoveredPosition.HasValue && IsInsideWorkArea(hoveredPosition.Value))
                    {
                        OnHoweredVoxel?.Invoke(hoveredPosition.Value);
                    }
                }
            }
            else if (currentBrushMode == BrushMode.Erase)
            {
                if (voxelHit.HasValue)
                {
                    OnHoweredVoxel?.Invoke(voxelHit.Value);
                }
            }
            else if (currentBrushMode == BrushMode.Pippette)
            {
                if (voxelHit.HasValue)
                {
                    OnHoweredVoxel?.Invoke(voxelHit.Value);
                }
            }
        }

        public void UpdateVisiblePlanes(CameraData cameraData) =>
            _cachedVisiblePlanes = GetVisiblePlanes(cameraData);
        public void OnBrushModeChanged(BrushMode newBrushMode) =>
            currentBrushMode = newBrushMode;

        private Vector3Int GetPlacementPosition(Vector3Int hitVoxel, Ray ray)
        {
            Vector3 voxelCenter = _target.NetPosition + new Vector3(
                (hitVoxel.x + 0.5f) * _target.VoxelSize,
                (hitVoxel.y + 0.5f) * _target.VoxelSize,
                (hitVoxel.z + 0.5f) * _target.VoxelSize
            );

            Bounds voxelBounds = new Bounds(voxelCenter, Vector3.one * _target.VoxelSize);
            voxelBounds.IntersectRay(ray, out float distance);
            Vector3 hitPoint = ray.GetPoint(distance);

            Vector3 direction = hitPoint - voxelCenter;
            float maxComponent = Mathf.Max(Mathf.Abs(direction.x), Mathf.Abs(direction.y), Mathf.Abs(direction.z));

            if (maxComponent == Mathf.Abs(direction.x))
                return hitVoxel + new Vector3Int(direction.x > 0 ? 1 : -1, 0, 0);
            else if (maxComponent == Mathf.Abs(direction.y))
                return hitVoxel + new Vector3Int(0, direction.y > 0 ? 1 : -1, 0);
            else
                return hitVoxel + new Vector3Int(0, 0, direction.z > 0 ? 1 : -1);
        } 
        private bool IsSkipPlane(PlaneModel planeModel, Vector3 hitPoint)
        {
            NetBound bounds = _target.NetBounds;

            switch (planeModel.VisibleSide)
            {
                case VisibleSide.IsBottom:
                    return hitPoint.y < bounds.BottomCornerZeroPos.y ||
                           hitPoint.x < bounds.BottomCornerZeroPos.x || hitPoint.x > bounds.BottomCornerXFar.x ||
                           hitPoint.z < bounds.BottomCornerZeroPos.z || hitPoint.z > bounds.BottomCornerZFar.z;

                case VisibleSide.IsTop:
                    return hitPoint.y > bounds.UpCornerZeroPos.y ||
                           hitPoint.x < bounds.UpCornerZeroPos.x || hitPoint.x > bounds.UpCornerXFar.x ||
                           hitPoint.z < bounds.UpCornerZeroPos.z || hitPoint.z > bounds.UpCornerZFar.z;

                case VisibleSide.IsFront:
                    return hitPoint.z < bounds.BottomCornerZeroPos.z ||
                           hitPoint.x < bounds.BottomCornerZeroPos.x || hitPoint.x > bounds.BottomCornerXFar.x ||
                           hitPoint.y < bounds.BottomCornerZeroPos.y || hitPoint.y > bounds.UpCornerZeroPos.y;

                case VisibleSide.IsBack:
                    return hitPoint.z > bounds.BottomCornerZFar.z ||
                           hitPoint.x < bounds.BottomCornerZFar.x || hitPoint.x > bounds.BottomCornerXZFar.x ||
                           hitPoint.y < bounds.BottomCornerZFar.y || hitPoint.y > bounds.UpCornerZFar.y;

                case VisibleSide.IsLeft:
                    return hitPoint.x < bounds.BottomCornerZeroPos.x ||
                           hitPoint.y < bounds.BottomCornerZeroPos.y || hitPoint.y > bounds.UpCornerZeroPos.y ||
                           hitPoint.z < bounds.BottomCornerZeroPos.z || hitPoint.z > bounds.BottomCornerZFar.z;

                case VisibleSide.IsRight:
                    return hitPoint.x > bounds.BottomCornerXFar.x ||
                           hitPoint.y < bounds.BottomCornerXFar.y || hitPoint.y > bounds.UpCornerXFar.y ||
                           hitPoint.z < bounds.BottomCornerXFar.z || hitPoint.z > bounds.BottomCornerXZFar.z;

                default:
                    return true;
            }
        }
        public Vector3Int? GetHoveredGridPosition(Ray ray)
        {
            Vector3Int? voxelHit = CheckVoxelIntersection(ray).position;
            if (voxelHit.HasValue)
            {
                return voxelHit.Value;
            }

            if (_cachedVisiblePlanes == null) return null;

            foreach (PlaneModel planeModel in _cachedVisiblePlanes)
            {
                Plane plane = planeModel.Plane;
                float enter;
                if (plane.Raycast(ray, out enter))
                { 
                    Vector3 hitPoint = ray.GetPoint(enter);
                    if (IsSkipPlane(planeModel, hitPoint)) continue;

                    Vector3 localHitPoint = hitPoint - _target.transform.position;
                    localHitPoint += plane.normal / 10000;

                    Vector3Int gridPosition = new Vector3Int(
                        Mathf.FloorToInt((localHitPoint.x - _target.NetOffset.x) / _target.VoxelSize),
                        Mathf.FloorToInt((localHitPoint.y - _target.NetOffset.y) / _target.VoxelSize),
                        Mathf.FloorToInt((localHitPoint.z - _target.NetOffset.z) / _target.VoxelSize)
                    );

                    gridPosition.x = Math.Clamp(gridPosition.x, 0, _target.NetSize.x);
                    gridPosition.y = Math.Clamp(gridPosition.y, 0, _target.NetSize.y);
                    gridPosition.z = Math.Clamp(gridPosition.z, 0, _target.NetSize.z);


                    if (IsInsideWorkArea(gridPosition))
                    {
                        if (_target.IsDebug)
                        {
                            _target.DebugMessage = gridPosition.ToString();
                            Color c = Handles.color;
                            Handles.color = Color.white;
                            Handles.DrawWireDisc(localHitPoint, plane.normal, 0.01f);
                            Handles.color = c;
                        }

                        return gridPosition;
                    }
                    else
                    {
                        if (_target.IsDebug)
                        {
                            _target.DebugMessage = gridPosition.ToString();
                            Color c = Handles.color;
                            Handles.color = Color.red;
                            Handles.DrawWireDisc(localHitPoint, plane.normal, 0.01f);
                            Handles.color = c;
                        }
                    }
                }
            }

            return null;
        } 
        private (Vector3Int? position, float distance) CheckVoxelIntersection(Ray ray)
        {
            float minDistance = float.MaxValue;
            Vector3Int? closestVoxel = null;

            foreach (VoxData voxel in _target.Voxels)
            {
                if (!voxel.IsFilled) continue;

                Bounds voxelBounds = new Bounds(voxel.Center, Vector3.one * _target.VoxelSize);
                if (voxelBounds.IntersectRay(ray, out float distance))
                {
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestVoxel = voxel.NetPosition;
                    }
                }
            }

            return (closestVoxel, minDistance);
        } 
        private IEnumerable<PlaneModel> GetVisiblePlanes(CameraData _cameraData)
        {
            Vector3 cubeSize = new Vector3(_target.NetSize.x, _target.NetSize.y, _target.NetSize.z) * _target.VoxelSize;

            if (_cameraData.VisibleSides.HasFlag(VisibleSide.IsBottom))
                yield return new PlaneModel
                {
                    Plane = new Plane(Vector3.up, _target.NetPosition),
                    VisibleSide = VisibleSide.IsBottom,
                };
            if (_cameraData.VisibleSides.HasFlag(VisibleSide.IsTop))
                yield return new PlaneModel
                {
                    Plane = new Plane(Vector3.down, _target.NetPosition + Vector3.up * cubeSize.y),
                    VisibleSide = VisibleSide.IsTop,
                };
            if (_cameraData.VisibleSides.HasFlag(VisibleSide.IsFront))
                yield return new PlaneModel
                {
                    Plane = new Plane(Vector3.back, _target.NetPosition),
                    VisibleSide = VisibleSide.IsFront,
                };
            if (_cameraData.VisibleSides.HasFlag(VisibleSide.IsBack))
            {
                yield return new PlaneModel
                {
                    Plane = new Plane(Vector3.back, _target.NetPosition + Vector3.forward * cubeSize.z),
                    VisibleSide = VisibleSide.IsBack,
                };
            }
            if (_cameraData.VisibleSides.HasFlag(VisibleSide.IsLeft))
                yield return new PlaneModel
                {
                    Plane = new Plane(Vector3.right, _target.NetPosition),
                    VisibleSide = VisibleSide.IsLeft,
                };
            if (_cameraData.VisibleSides.HasFlag(VisibleSide.IsRight))
                yield return new PlaneModel
                {
                    Plane = new Plane(Vector3.left, _target.NetPosition + Vector3.right * cubeSize.x),
                    VisibleSide = VisibleSide.IsRight,
                };
        } 
        private bool IsInsideWorkArea(Vector3Int position)
        {
            return position.x >= 0 && position.x < _target.NetSize.x &&
                   position.y >= 0 && position.y < _target.NetSize.y &&
                   position.z >= 0 && position.z < _target.NetSize.z;
        }

        private struct PlaneModel
        {
            public Plane Plane { get; set; }
            public VisibleSide VisibleSide { get; set; }
        }
         
    }
}
