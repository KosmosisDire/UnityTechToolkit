using System;
using System.Collections.Generic;
using UnityEngine;

namespace Toolkit.MeshGeneration
{
    public static class PlaneMeshGenerator
    {
        public static Mesh Generate(PlaneShapeData data, Mesh mesh)
        {
            var halfWidth = data.width / 2;
            var halfLength = data.length / 2;

            var vertices = new Vector3[4]
            {
                new(-halfWidth, 0, -halfLength),
                new(halfWidth, 0, -halfLength),
                new(halfWidth, 0, halfLength),
                new(-halfWidth, 0, halfLength)
            };

            var triangles = new int[] {0, 1, 2, 0, 2, 3};
            var normals = new Vector3[] {Vector3.up, Vector3.up, Vector3.up, Vector3.up};
            var uvs = new Vector2[] {new (0, 0), new (1, 0), new (1, 1), new (0, 1)};

            // Apply transform
            if (data.offset != Vector3.zero || data.rotation != Quaternion.identity || data.scale != 1f)
            {
                var matrix = Matrix4x4.TRS(data.offset, data.rotation, Vector3.one * data.scale);
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = matrix.MultiplyPoint3x4(vertices[i]);
                }
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.normals = normals;
            mesh.uv = uvs;

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            return mesh;
        }

        public static Mesh Generate(PlaneShapeData data)
        {
            var mesh = new Mesh();
            return Generate(data, mesh);
        }
    }

    [System.Serializable]
    public struct PlaneShapeData : IEquatable<PlaneShapeData>
    {
        public float width;
        public float length;
        public Vector3 offset;
        public Quaternion rotation;
        public float scale;
        public float outlineWidth;

        public bool Equals(PlaneShapeData other)
        {
            return width.Equals(other.width) && length.Equals(other.length) && offset.Equals(other.offset) && rotation.Equals(other.rotation) && scale.Equals(other.scale) && outlineWidth.Equals(other.outlineWidth);
        }

        public override bool Equals(object obj)
        {
            return obj is PlaneShapeData other && Equals(other);
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(width, length, offset, rotation, scale, outlineWidth);
        }

        public static bool operator ==(PlaneShapeData left, PlaneShapeData right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PlaneShapeData left, PlaneShapeData right)
        {
            return !left.Equals(right);
        }
    }
}
