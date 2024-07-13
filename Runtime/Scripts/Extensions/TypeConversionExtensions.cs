
namespace SimToolkit
{
    public static class TypeConversionExtensions
    {
        public static UnityEngine.Vector3 ToUnity(this System.Numerics.Vector3 vector3)
        {
            return new UnityEngine.Vector3(vector3.X, vector3.Y, vector3.Z);
        }

        public static System.Numerics.Vector3 ToSystem(this UnityEngine.Vector3 vector3)
        {
            return new System.Numerics.Vector3(vector3.x, vector3.y, vector3.z);
        }

        public static UnityEngine.Quaternion ToUnity(this System.Numerics.Quaternion quaternion)
        {
            return new UnityEngine.Quaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
        }

        public static System.Numerics.Quaternion ToSystem(this UnityEngine.Quaternion quaternion)
        {
            return new System.Numerics.Quaternion(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
        }

        public static UnityEngine.Color ToUnity(this System.Numerics.Vector4 vector4)
        {
            return new UnityEngine.Color(vector4.X, vector4.Y, vector4.Z, vector4.W);
        }

        public static System.Numerics.Vector4 ToSystem(this UnityEngine.Color color)
        {
            return new System.Numerics.Vector4(color.r, color.g, color.b, color.a);
        }
    }
}