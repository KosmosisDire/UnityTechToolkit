using UnityEngine;
using System;

#region Attributes and Interfaces

[AttributeUsage(AttributeTargets.Field)]
public class ShapeFieldAttribute : PropertyAttribute
{
    public string DisplayName { get; set; }
    public float Min { get; set; } = float.MinValue;
    public float Max { get; set; } = float.MaxValue;
    public bool HasRange => Min > float.MinValue + 1 && Max < float.MaxValue - 1;
    
    public ShapeFieldAttribute(string displayName)
    {
        DisplayName = displayName;
    }
}

public interface IShape
{
    Shape.ShapeType Type { get; }
    void GetShaderParameters(out Vector4 params1, out Vector4 params2, out Vector4 params3);
    float GetCornerRadius() { return 0f; }
    float GetExtrusion() { return 0f; }
    Bounds GetBounds();
}

#endregion

#region 3D Shapes

[System.Serializable]
public class BoxShape : IShape
{
    [ShapeField("Size")]
    public Vector3 size = Vector3.one;
    
    [ShapeField("Corner Radius", Min = 0f, Max = 0.5f)]
    public float cornerRadius = 0f;
    
    public Shape.ShapeType Type => Shape.ShapeType.Box;
    
    public void GetShaderParameters(out Vector4 params1, out Vector4 params2, out Vector4 params3)
    {
        params1 = new Vector4(size.x * 0.5f, size.y * 0.5f, size.z * 0.5f, 0);
        params2 = Vector4.zero;
        params3 = Vector4.zero;
    }
    
    public float GetCornerRadius() => cornerRadius;
    public Bounds GetBounds() => new Bounds(Vector3.zero, size);
}

[System.Serializable]
public class SphereShape : IShape
{
    [ShapeField("Radius", Min = 0.01f)]
    public float radius = 0.5f;
    
    public Shape.ShapeType Type => Shape.ShapeType.Sphere;
    
    public void GetShaderParameters(out Vector4 params1, out Vector4 params2, out Vector4 params3)
    {
        params1 = new Vector4(radius, radius, radius, 0);
        params2 = Vector4.zero;
        params3 = Vector4.zero;
    }
    
    public Bounds GetBounds() => new Bounds(Vector3.zero, Vector3.one * radius * 2f);
}

[System.Serializable]
public class EllipsoidShape : IShape
{
    [ShapeField("Radii")]
    public Vector3 radii = new Vector3(0.5f, 0.3f, 0.4f);
    
    public Shape.ShapeType Type => Shape.ShapeType.Ellipsoid;
    
    public void GetShaderParameters(out Vector4 params1, out Vector4 params2, out Vector4 params3)
    {
        params1 = new Vector4(radii.x, radii.y, radii.z, 0);
        params2 = Vector4.zero;
        params3 = Vector4.zero;
    }
    
    public Bounds GetBounds() => new Bounds(Vector3.zero, radii * 2f);
}

[System.Serializable]
public class CylinderShape : IShape
{
    [ShapeField("Height", Min = 0.01f)]
    public float height = 1f;
    
    [ShapeField("Radius", Min = 0.01f)]
    public float radius = 0.3f;

    [ShapeField("Corner Radius", Min = 0f, Max = 0.5f)]
    public float cornerRadius = 0f;
    
    public Shape.ShapeType Type => Shape.ShapeType.Cylinder;
    
    public void GetShaderParameters(out Vector4 params1, out Vector4 params2, out Vector4 params3)
    {
        Vector3 start = Vector3.down * height * 0.5f;
        Vector3 end = Vector3.up * height * 0.5f;
        params1 = new Vector4(start.x, start.y, start.z, radius);
        params2 = new Vector4(end.x, end.y, end.z, 0);
        params3 = Vector4.zero;
    }

    public float GetCornerRadius() => cornerRadius;
    
    public Bounds GetBounds() => new Bounds(Vector3.zero, new Vector3(radius * 2f, height, radius * 2f));
}

[System.Serializable]
public class CapsuleShape : IShape
{
    [ShapeField("Height", Min = 0.01f)]
    public float height = 1f;
    
    [ShapeField("Radius", Min = 0.01f)]
    public float radius = 0.2f;
    
    
    public Shape.ShapeType Type => Shape.ShapeType.Capsule;
    
    public void GetShaderParameters(out Vector4 params1, out Vector4 params2, out Vector4 params3)
    {
        Vector3 start = Vector3.down * height * 0.5f;
        Vector3 end = Vector3.up * height * 0.5f;
        params1 = new Vector4(start.x, start.y, start.z, radius);
        params2 = new Vector4(end.x, end.y, end.z, 0);
        params3 = Vector4.zero;
    }
    
    public Bounds GetBounds() => new Bounds(Vector3.zero, new Vector3(radius * 2f, height + radius * 2f, radius * 2f));
}

[System.Serializable]
public class ConeShape : IShape
{
    [ShapeField("Height", Min = 0.01f)]
    public float height = 1f;
    
    [ShapeField("Base Radius", Min = 0.01f)]
    public float radius = 0.4f;

    [ShapeField("Corner Radius", Min = 0f, Max = 0.5f)]
    public float cornerRadius = 0f;
    
    public Shape.ShapeType Type => Shape.ShapeType.Cone;
    
    public void GetShaderParameters(out Vector4 params1, out Vector4 params2, out Vector4 params3)
    {
        Vector3 basePos = Vector3.down * height * 0.5f;
        Vector3 tip = Vector3.up * height * 0.5f;
        params1 = new Vector4(basePos.x, basePos.y, basePos.z, radius);
        params2 = new Vector4(tip.x, tip.y, tip.z, 0);
        params3 = Vector4.zero;
    }
    
    public float GetCornerRadius() => cornerRadius;
    public Bounds GetBounds() => new Bounds(Vector3.zero, new Vector3(radius * 2f, height, radius * 2f));
}

[System.Serializable]
public class ArrowShape : IShape
{
    [ShapeField("Start Point")]
    public Vector3 start = new Vector3(0, -0.5f, 0);
    
    [ShapeField("End Point")]
    public Vector3 end = new Vector3(0, 0.5f, 0);
    
    [ShapeField("Shaft Radius", Min = 0.01f)]
    public float shaftRadius = 0.1f;
    
    [ShapeField("Head Radius", Min = 0.01f)]
    public float headRadius = 0.2f;
    
    [ShapeField("Head Length", Min = 0.01f)]
    public float headLength = 0.3f;

    [ShapeField("Corner Radius", Min = 0.0f, Max = 0.5f)]
    public float cornerRadius = 0.0f;
    
    public Shape.ShapeType Type => Shape.ShapeType.Arrow;
    
    public void GetShaderParameters(out Vector4 params1, out Vector4 params2, out Vector4 params3)
    {
        // For arrows, we'll use special encoding
        // params1: start.xyz, shaftRadius
        // params2: end.xyz, headRadius
        // params3: headLength, 0, 0, 0
        params1 = new Vector4(start.x, start.y, start.z, shaftRadius);
        params2 = new Vector4(end.x, end.y, end.z, headRadius);
        params3 = new Vector4(headLength, 0, 0, 0);
    }

    public float GetCornerRadius() => cornerRadius;
    
    public Bounds GetBounds()
    {
        Vector3 min = Vector3.Min(start, end);
        Vector3 max = Vector3.Max(start, end);
        float maxRadius = Mathf.Max(shaftRadius, headRadius);
        return new Bounds((min + max) * 0.5f, (max - min) + Vector3.one * maxRadius * 2f);
    }
}

#endregion

#region 2D Shapes

[System.Serializable]
public class RectangleShape : IShape
{
    [ShapeField("Size")]
    public Vector2 size = Vector2.one;
    
    [ShapeField("Corner Radius", Min = 0f, Max = 0.5f)]
    public float cornerRadius = 0f;
    
    [ShapeField("Extrusion", Min = 0f, Max = 1f)]
    public float extrusion = 0f;
    
    public Shape.ShapeType Type => Shape.ShapeType.Rectangle;
    
    public void GetShaderParameters(out Vector4 params1, out Vector4 params2, out Vector4 params3)
    {
        params1 = new Vector4(size.x * 0.5f, size.y * 0.5f, 0, 0);
        params2 = Vector4.zero;
        params3 = new Vector4(0, 0, 1, 0); // Normal direction
    }
    
    public float GetCornerRadius() => cornerRadius;
    public float GetExtrusion() => extrusion;
    public Bounds GetBounds() => new Bounds(Vector3.zero, new Vector3(size.x, size.y, extrusion));
}

[System.Serializable]
public class DiskShape : IShape
{
    [ShapeField("Radius", Min = 0.01f)]
    public float radius = 0.5f;
    
    [ShapeField("Extrusion", Min = 0f, Max = 1f)]
    public float extrusion = 0f;
    
    public Shape.ShapeType Type => Shape.ShapeType.Disk;
    
    public void GetShaderParameters(out Vector4 params1, out Vector4 params2, out Vector4 params3)
    {
        params1 = new Vector4(radius, 0, 0, 0);
        params2 = Vector4.zero;
        params3 = new Vector4(0, 0, 1, 0); // Normal direction
    }
    
    public float GetExtrusion() => extrusion;
    public Bounds GetBounds() => new Bounds(Vector3.zero, new Vector3(radius * 2f, radius * 2f, extrusion));
}

[System.Serializable]
public class TriangleShape : IShape
{
    [ShapeField("Vertex 1")]
    public Vector3 vertex1 = new Vector3(-0.5f, -0.5f, 0);
    
    [ShapeField("Vertex 2")]
    public Vector3 vertex2 = new Vector3(0.5f, -0.5f, 0);
    
    [ShapeField("Vertex 3")]
    public Vector3 vertex3 = new Vector3(0, 0.5f, 0);
    
    [ShapeField("Extrusion", Min = 0f, Max = 1f)]
    public float extrusion = 0f;
    
    public Shape.ShapeType Type => Shape.ShapeType.Triangle;
    
    public void GetShaderParameters(out Vector4 params1, out Vector4 params2, out Vector4 params3)
    {
        params1 = new Vector4(vertex1.x, vertex1.y, vertex1.z, 0);
        params2 = new Vector4(vertex2.x, vertex2.y, vertex2.z, 0);
        params3 = new Vector4(vertex3.x, vertex3.y, vertex3.z, 0);
    }
    
    public float GetExtrusion() => extrusion;
    
    public Bounds GetBounds()
    {
        Vector3 min = Vector3.Min(Vector3.Min(vertex1, vertex2), vertex3);
        Vector3 max = Vector3.Max(Vector3.Max(vertex1, vertex2), vertex3);
        return new Bounds((min + max) * 0.5f, (max - min) + new Vector3(0, 0, extrusion));
    }
}

[System.Serializable]
public class Line2DShape : IShape
{
    [ShapeField("Start Point")]
    public Vector3 start = new Vector3(-0.5f, 0, 0);
    
    [ShapeField("End Point")]
    public Vector3 end = new Vector3(0.5f, 0, 0);
    
    [ShapeField("Thickness", Min = 0.01f, Max = 0.5f)]
    public float thickness = 0.05f;
    
    public Shape.ShapeType Type => Shape.ShapeType.Line2D;
    
    public void GetShaderParameters(out Vector4 params1, out Vector4 params2, out Vector4 params3)
    {
        params1 = new Vector4(start.x, start.y, start.z, thickness);
        params2 = new Vector4(end.x, end.y, end.z, 0);
        params3 = new Vector4(0, 0, 1, 0);
    }
    
    public Bounds GetBounds()
    {
        Vector3 min = Vector3.Min(start, end);
        Vector3 max = Vector3.Max(start, end);
        return new Bounds((min + max) * 0.5f, (max - min) + Vector3.one * thickness);
    }
}

#endregion