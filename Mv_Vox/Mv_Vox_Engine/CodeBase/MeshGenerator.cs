using System.Collections.Generic;
using static MvVox.VoxCreater;
using System.Linq; 
using UnityEngine;
using System;
using static UnityEngine.GraphicsBuffer;

namespace MvVox
{
    public class MeshGenerator
    {
        private VoxCreater _target;
        private float _voxelSize => _target.VoxelSize;

        public MeshGenerator(VoxCreater target)
        {
            _target = target; 
        }

        public Mesh GenerateMesh()
        {
            List<Cube> cubes = GenerateCubes();
            return CreateMeshFromCubes(cubes);
        }

        private List<Cube> GenerateCubes()
        {
            List<Cube> cubes = new List<Cube>();

            _target.Voxels.RemoveAll(v => !v.IsFilled);

            foreach (VoxData voxel in _target.Voxels.Where(v => v.IsFilled))
            {
                VisibleSide visibleSides = VisibleSide.All;

                if (HasNeighbor(voxel, Vector3Int.forward)) visibleSides &= ~VisibleSide.IsFront;
                if (HasNeighbor(voxel, Vector3Int.back)) visibleSides &= ~VisibleSide.IsBack;
                if (HasNeighbor(voxel, Vector3Int.left)) visibleSides &= ~VisibleSide.IsLeft;
                if (HasNeighbor(voxel, Vector3Int.right)) visibleSides &= ~VisibleSide.IsRight;
                if (HasNeighbor(voxel, Vector3Int.up)) visibleSides &= ~VisibleSide.IsTop;
                if (HasNeighbor(voxel, Vector3Int.down)) visibleSides &= ~VisibleSide.IsBottom;

                if (visibleSides != VisibleSide.None)
                {
                    cubes.Add(new Cube(voxel.Position, visibleSides, voxel.color, _voxelSize));
                }
            }

            return cubes;
        }

        private Mesh CreateMeshFromCubes(List<Cube> cubes)
        {
            Mesh mesh = new Mesh();
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Color> colors = new List<Color>();
            List<Vector2> uvs = new List<Vector2>();

            foreach (Cube cube in cubes)
            {
                var (cubeVertices, cubeTriangles, cubeColors, cubeUVs) = cube.GetCube();

                int vertexOffset = vertices.Count;
                vertices.AddRange(cubeVertices);
                colors.AddRange(cubeColors);
                uvs.AddRange(cubeUVs);

                foreach (int triangle in cubeTriangles)
                {
                    triangles.Add(vertexOffset + triangle);
                }
            }

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetColors(colors);
            mesh.SetUVs(0, uvs);

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        private bool HasNeighbor(VoxData voxel, Vector3Int direction)
        {
            Vector3Int neighborPos = voxel.NetPosition + direction;
            return _target.Voxels.Any(v => v.NetPosition == neighborPos && v.IsFilled);
        }
    }

    [Serializable]
    public class Cube
    {
        public Vector3 Position { get; private set; }
        public VisibleSide VisibleSides { get; private set; }
        public Color Color { get; private set; }
        private float Size { get; set; }

        public Cube(Vector3 position, VisibleSide visibleSides, Color color, float size)
        {
            Position = position;
            VisibleSides = visibleSides;
            Color = color;
            Size = size;
        }

        public (List<Vector3> vertices, List<int> triangles, List<Color> colors, List<Vector2> uvs) GetCube()
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Color> colors = new List<Color>();
            List<Vector2> uvs = new List<Vector2>();

            if (VisibleSides.HasFlag(VisibleSide.IsFront))
                AddFace(Vector3.forward, vertices, triangles, colors, uvs);
            if (VisibleSides.HasFlag(VisibleSide.IsBack))
                AddFace(Vector3.back, vertices, triangles, colors, uvs);
            if (VisibleSides.HasFlag(VisibleSide.IsLeft))
                AddFace(Vector3.left, vertices, triangles, colors, uvs);
            if (VisibleSides.HasFlag(VisibleSide.IsRight))
                AddFace(Vector3.right, vertices, triangles, colors, uvs);
            if (VisibleSides.HasFlag(VisibleSide.IsTop))
                AddFace(Vector3.up, vertices, triangles, colors, uvs);
            if (VisibleSides.HasFlag(VisibleSide.IsBottom))
                AddFace(Vector3.down, vertices, triangles, colors, uvs);

            return (vertices, triangles, colors, uvs);
        }


        private void AddFace(Vector3 normal, List<Vector3> vertices, List<int> triangles, List<Color> colors, List<Vector2> uvs)
        {
            Vector3 side1 = new Vector3(normal.y, normal.z, normal.x);
            Vector3 side2 = Vector3.Cross(normal, side1);

            int vCount = vertices.Count;

            vertices.Add(Position + (normal - side1 - side2) * Size * 0.5f);
            vertices.Add(Position + (normal - side1 + side2) * Size * 0.5f);
            vertices.Add(Position + (normal + side1 + side2) * Size * 0.5f);
            vertices.Add(Position + (normal + side1 - side2) * Size * 0.5f);

            triangles.Add(vCount);
            triangles.Add(vCount + 2);
            triangles.Add(vCount + 1);
            triangles.Add(vCount);
            triangles.Add(vCount + 3);
            triangles.Add(vCount + 2);

            for (int i = 0; i < 4; i++)
                colors.Add(Color);

            // Добавляем UV-координаты
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(0, 1));
        }
    }
}
