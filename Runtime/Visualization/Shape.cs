using UnityEngine;
using System;

[ExecuteAlways]
[AddComponentMenu("Rendering/Shape")]
public class Shape : MonoBehaviour
{
    public enum ShapeType
    {
        Box,
        Sphere,
        Ellipsoid,
        Cylinder,
        Capsule,
        Cone,
        Arrow,
        Rectangle,
        Disk,
        Triangle,
        Line2D
    }

    [SerializeReference]
    public IShape shapeData;

    [Header("Appearance")]
    public bool fill = true;
    public Color fillColor = new Color(1f, 0.5f, 0.5f, 1f);

    [Range(0f, 1f)]
    public float outlineThickness = 0.02f;
    public Color outlineColor = Color.black;

    [Header("Lighting")]
    public bool enableLighting = true;
    [Range(0f, 1f)]
    public float smoothness = 0.5f;

    [Header("Transform")]
    public bool useTransformPosition = true;
    public bool useTransformRotation = true;
    public bool useTransformScale = false;
    public Vector3 localOffset = Vector3.zero;

    private string shapeId;

    void OnEnable()
    {
        shapeId = GetInstanceID().ToString();
        
        // Initialize with default shape if null
        if (shapeData == null)
        {
            shapeData = new BoxShape();
        }
    }

    void OnDisable()
    {
        if (Draw.Instance != null)
        {
            Draw.RemovePersistentShape(shapeId);
        }
    }

    void Update()
    {
        if (!isActiveAndEnabled) return;
        DrawShape();
    }

    void DrawShape()
    {
        Bounds bounds = shapeData.GetBounds();
        Vector3 position = useTransformPosition ? transform.position + transform.rotation * localOffset : localOffset;
        Quaternion rotation = useTransformRotation ? transform.rotation : Quaternion.identity;
        Vector3 scale = useTransformScale ? transform.lossyScale : Vector3.one;

        Matrix4x4 matrix = Matrix4x4.TRS(position, rotation, Vector3.Scale(scale, bounds.size));

        // Get shader parameters
        shapeData.GetShaderParameters(out Vector4 params1, out Vector4 params2, out Vector4 params3);

        // Create shape data
        var data = new Draw.ShapeData
        {
            shapeType = (int)shapeData.Type,
            params1 = params1,
            params2 = params2,
            params3 = params3,
            fillColor = fill ? fillColor : Color.clear,
            outlineColor = outlineColor,
            outlineThickness = outlineThickness,
            cornerRadius = shapeData.GetCornerRadius(),
            extrusion = shapeData.GetExtrusion(),
            enableLighting = enableLighting ? 1f : 0f,
            smoothness = smoothness,
            matrix = matrix
        };

        Draw.Instance.SetPersistentShape(shapeId, Time.deltaTime * 2f, data);
    }

    public void SetShapeType(ShapeType type)
    {
        shapeData = type switch
        {
            ShapeType.Box => new BoxShape(),
            ShapeType.Sphere => new SphereShape(),
            ShapeType.Ellipsoid => new EllipsoidShape(),
            ShapeType.Cylinder => new CylinderShape(),
            ShapeType.Capsule => new CapsuleShape(),
            ShapeType.Cone => new ConeShape(),
            ShapeType.Arrow => new ArrowShape(),
            ShapeType.Rectangle => new RectangleShape(),
            ShapeType.Disk => new DiskShape(),
            ShapeType.Triangle => new TriangleShape(),
            ShapeType.Line2D => new Line2DShape(),
            _ => new BoxShape()
        };
    }
}