using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System;

namespace MvVox
{
    public class VoxCreater : MonoBehaviour
    {
        [SerializeField] public string Name = "VoxelMesh";
        [field: SerializeField] public float VoxelSize = 0.25f;
        [SerializeField] private Vector3Int _netSize = new Vector3Int(16, 16, 16);
        [SerializeField] private Vector3 netOffset = new Vector2();

        [SerializeField] public List<VoxData> Voxels = new List<VoxData>(); 
        [SerializeField] public Color DrawColor = Color.white;
        [SerializeField] public Color NetDrawColor = new Color(1, 1, 1, 0.3f);
        [SerializeField] public float MarketSize = 0.2f;
        [SerializeField] public bool IsDrawNet = true;
        [SerializeField] public BrushMode CurrentBrushMode = BrushMode.Paint;
        [SerializeField] public KeyCode PaintKey = KeyCode.Mouse0;
        [SerializeField] public bool IsDebug;
        [SerializeField] public string DebugMessage;
        [SerializeField] private string shaderName = "Mv_VoxShader"; 
        

        private MeshGenerator meshGenerator;
        private NetBound netBound;
        public MeshGenerator MeshGenerator { get => meshGenerator ?? (meshGenerator = new MeshGenerator(this)); }
        public NetBound NetBounds { get => netBound ?? (netBound = new NetBound(this)); }
        public Vector3Int NetSize => _netSize;
        public Vector3 NetPosition => transform.position + NetOffset;
        public Vector3 NetOffset => netOffset;
        public string NameOfObject => Name;

        public void GenerateMesh()
        {
            string name = string.IsNullOrEmpty(Name) ? "VoxelMesh" : Name;
            GameObject go = new GameObject(name);
            go.transform.position = transform.position;
            go.transform.rotation = transform.rotation;
            go.transform.localScale = transform.localScale;
            MeshFilter meshFilter = go.AddComponent<MeshFilter>();
            MeshRenderer renderer = go.AddComponent<MeshRenderer>();
            meshFilter.sharedMesh = MeshGenerator.GenerateMesh();
            meshFilter.sharedMesh.name = name;
#if UNITY_EDITOR
            Material material = ShaderFinder.CreateMaterialFromShader(shaderName);
            if (material != null)
            { 
                AssetDatabase.CreateAsset(material, $"Assets/Mv_VoxMaterial({Name}).mat");
                AssetDatabase.SaveAssets();
            }
            renderer.sharedMaterial = material;
#endif
        }

        public void PutNetAtCenter()
        {
            Vector3 netSize = new Vector3(
                -NetSize.x * VoxelSize,
                0,
                -NetSize.z * VoxelSize);

            Vector3 halfNetSize = netSize * 0.5f;
            netOffset = halfNetSize;
        }

        public void Clear()
        {
            Voxels = new List<VoxData>(NetSize.x * NetSize.y * NetSize.z);
            for (int x = 0; x < NetSize.x; x++)
            {
                for (int y = 0; y < NetSize.y; y++)
                {
                    for (int z = 0; z < NetSize.z; z++)
                    {
                        var worldPosition = NetPosition + new Vector3(x * VoxelSize, y * VoxelSize, z * VoxelSize);
                        Voxels.Add(new VoxData(this)
                        {
                            NetPosition = new Vector3Int(x, y, z),
                            Position = worldPosition,
                        });
                    }
                }
            }
        } 

        public void Resize()
        {
            List<VoxData> tempList = new List<VoxData>();
            foreach (var voxel in Voxels)
            {
                if (voxel.NetPosition.x < NetSize.x &&
                    voxel.NetPosition.y < NetSize.y &&
                    voxel.NetPosition.z < NetSize.z)
                {
                    tempList.Add(voxel);
                }
            }

            Clear();
            foreach(VoxData newVoxel in Voxels)
            { 
                VoxData oldVoxData = tempList.FirstOrDefault(e => e.NetPosition == newVoxel.NetPosition);
                if (oldVoxData != null) newVoxel.CloneFrom(oldVoxData);
            }
        }

        [SerializeField]
        public class NetBound
        {
            private VoxCreater _target;
            public NetBound(VoxCreater target)
            {
                this._target = target;
            } 

            public Vector3 BottomCornerZeroPos => _target.NetPosition;
            public Vector3 BottomCornerXFar => _target.NetPosition + new Vector3(_target._netSize.x * _target.VoxelSize, 0, 0);
            public Vector3 BottomCornerZFar => _target.NetPosition + new Vector3(0, 0, _target._netSize.z * _target.VoxelSize);
            public Vector3 BottomCornerXZFar => _target.NetPosition + new Vector3(_target._netSize.x * _target.VoxelSize, 0, _target._netSize.z * _target.VoxelSize);
            public Vector3 UpCornerZeroPos => _target.NetPosition + new Vector3(0, _target._netSize.y * _target.VoxelSize, 0);
            public Vector3 UpCornerXFar => _target.NetPosition + new Vector3(_target._netSize.x * _target.VoxelSize, _target._netSize.y * _target.VoxelSize, 0);
            public Vector3 UpCornerZFar => _target.NetPosition + new Vector3(0, _target._netSize.y * _target.VoxelSize, _target._netSize.z * _target.VoxelSize);
            public Vector3 UpCornerXZFar => _target.NetPosition + new Vector3(_target._netSize.x * _target.VoxelSize, _target._netSize.y * _target.VoxelSize, _target._netSize.z * _target.VoxelSize);

        }

        [Serializable]
        public class VoxData
        {
            private VoxCreater _target;
            public VoxData(VoxCreater target)
            {
                this._target = target;
            }

            [SerializeField] public Vector3Int NetPosition; 
            public Vector3 Position
            {
                get => position + _target.NetPosition;
                set => position = value - _target.NetPosition;
            }
            [SerializeField] private Vector3 position;
            [SerializeField] public Color color = Color.white;
            [SerializeField] public bool IsFilled;
            [SerializeField] public bool IsOutlined;
            [SerializeField]
            public Vector3 Center { get => Position + Vector3.one * _target.VoxelSize * 0.5f; }
            [SerializeField] private Vector3 center;
            public void Draw()
            {
                if (!IsFilled) return;

                Handles.color = color;
                Handles.CubeHandleCap(
                    0,
                    Center,
                    Quaternion.identity,
                    _target.VoxelSize,
                    EventType.Repaint
                );
            }

            public void CloneFrom(VoxData source)
            {
                NetPosition = source.NetPosition;
                Position = source.Position;
                color = source.color;
                IsFilled = source.IsFilled;
                IsOutlined = source.IsOutlined;
            }
        }
    } 
}
