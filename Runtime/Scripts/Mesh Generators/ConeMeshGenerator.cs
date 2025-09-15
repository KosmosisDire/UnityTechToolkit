using UnityEngine;

namespace Toolkit.MeshGeneration
{
    public static class ConeMeshGenerator
    {
        private static Mesh _unitConeMesh;

        public static Mesh Generate(ConeShapeData data)
        {
            var mesh = CylinderMeshGenerator.Generate(data.height, data.radius, 0, data.offset, data.rotation, data.scale);
            return mesh;
        }

        public static Mesh GetIdentityMesh()
        {
            if (_unitConeMesh == null)
            {
                _unitConeMesh = CylinderMeshGenerator.Generate(1f, 1f, 0f);
                _unitConeMesh.name = "Unit Cone";
            }
            return _unitConeMesh;
        }
    }

    [System.Serializable]
    public struct ConeShapeData : System.IEquatable<ConeShapeData>
    {
        public float height;
        public float radius;
        public Vector3 offset;
        public Quaternion rotation;
        public Vector3 scale;

        public bool Equals(ConeShapeData other)
        {
            return height.Equals(other.height) && radius.Equals(other.radius) && offset.Equals(other.offset) && rotation.Equals(other.rotation) && scale.Equals(other.scale);
        }

        public override bool Equals(object other)
        {
            return other is ConeShapeData data && Equals(data);
        }

        public override readonly int GetHashCode()
        {
            return System.HashCode.Combine(height, radius, offset, rotation, scale);
        }

        public static bool operator ==(ConeShapeData left, ConeShapeData right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ConeShapeData left, ConeShapeData right)
        {
            return !left.Equals(right);
        }
    }

    
}