using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class Draw : MonoBehaviour
{
    #region Singleton
    private static Draw instance;
    public static Draw Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<Draw>();
                if (instance == null)
                {
                    GameObject go = new GameObject("ShapeManager");
                    instance = go.AddComponent<Draw>();
                }
            }
            return instance;
        }
    }
    #endregion

    #region Setup
    [Header("Resources")]
    public Material shapeMaterial;
    public Mesh boundingCube;

    [Header("Default Settings")]
    public float defaultOutlineThickness = 0.02f;
    public Color defaultOutlineColor = Color.black;
    public bool defaultEnableLighting = true;
    public float defaultSmoothness = 0.5f;
    #endregion

    #region Data Structures
    public struct ShapeData
    {
        public int shapeType;
        public Vector4 params1, params2, params3;
        public Color fillColor, outlineColor;
        public float outlineThickness, cornerRadius, extrusion;
        public float enableLighting, smoothness;
        public Matrix4x4 matrix;
        public Vector3 scale;
    }

    private struct PersistentShape
    {
        public ShapeData data;
        public float endTime;
    }

    private Dictionary<string, PersistentShape> persistentShapes = new Dictionary<string, PersistentShape>();
    private List<ShapeData> immediateShapes = new List<ShapeData>();
    #endregion

    #region Lifecycle
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    void OnEnable()
    {
        if (shapeMaterial == null || boundingCube == null)
        {
            LoadDefaultResources();
        }

        if (!Application.isPlaying)
        {
            // UnityEditor.EditorApplication.update += DrawUpdate;
        }
    }

    void OnDisable()
    {
        if (!Application.isPlaying)
        {
            // UnityEditor.EditorApplication.update -= DrawUpdate;
        }
    }

    void LoadDefaultResources()
    {
        // Try to find default cube mesh
        GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        boundingCube = temp.GetComponent<MeshFilter>().sharedMesh;
        DestroyImmediate(temp);

        // Try to find or create a default shape material
        if (shapeMaterial == null)
        {
            shapeMaterial = new Material(Shader.Find("Standard"));
            Debug.LogWarning("ShapeManager: No shape material assigned, using Standard shader");
        }
    }
    #endregion

    #region Public API
    public void AddImmediateShape(ShapeData data)
    {
        immediateShapes.Add(data);
    }

    public void SetPersistentShape(string id, float duration, ShapeData data)
    {
        persistentShapes[id] = new PersistentShape
        {
            data = data,
            endTime = Time.time + duration
        };
    }

    public static void RemovePersistentShape(string id)
    {
        if (Instance != null)
        {
            Instance.persistentShapes.Remove(id);
        }
    }

    public static void ClearAllPersistentShapes()
    {
        if (Instance != null)
        {
            Instance.persistentShapes.Clear();
        }
    }
    #endregion

    #region Static Drawing API
    public static void Box(Vector3 position, Vector3 size, Color color, Quaternion? rotation = null, bool wireframe = false, float cornerRadius = 0f)
    {
        var data = new ShapeData
        {
            shapeType = (int)Shape.ShapeType.Box,
            params1 = new Vector4(size.x * 0.5f, size.y * 0.5f, size.z * 0.5f, 0),
            params2 = Vector4.zero,
            params3 = Vector4.zero,
            fillColor = color,
            outlineColor = Instance.defaultOutlineColor,
            outlineThickness = wireframe ? 0.015f : Instance.defaultOutlineThickness,
            cornerRadius = cornerRadius,
            extrusion = 0f,
            enableLighting = Instance.defaultEnableLighting ? 1f : 0f,
            smoothness = Instance.defaultSmoothness,
            matrix = Matrix4x4.TRS(position, rotation ?? Quaternion.identity, size)
        };
        Instance.AddImmediateShape(data);
    }

    public static void Sphere(Vector3 position, float radius, Color color, Quaternion? rotation = null, bool wireframe = false)
    {
        Vector3 scale = Vector3.one * (radius * 2f);
        var data = new ShapeData
        {
            shapeType = (int)Shape.ShapeType.Ellipsoid,
            params1 = new Vector4(radius, radius, radius, 0),
            params2 = Vector4.zero,
            params3 = Vector4.zero,
            fillColor = color,
            outlineColor = Instance.defaultOutlineColor,
            outlineThickness = wireframe ? 0.015f : Instance.defaultOutlineThickness,
            cornerRadius = 0f,
            extrusion = 0f,
            enableLighting = Instance.defaultEnableLighting ? 1f : 0f,
            smoothness = Instance.defaultSmoothness,
            matrix = Matrix4x4.TRS(position, rotation ?? Quaternion.identity, scale)
        };
        Instance.AddImmediateShape(data);
    }

    public static void Cylinder(Vector3 start, Vector3 end, float radius, Color color, bool wireframe = false)
    {
        Vector3 center = (start + end) * 0.5f;
        Vector3 direction = end - start;
        float height = direction.magnitude;
        
        // Calculate rotation to align cylinder with start-end direction
        Quaternion rotation = Quaternion.identity;
        if (direction != Vector3.zero)
        {
            rotation = Quaternion.LookRotation(direction, Vector3.up);
            rotation = rotation * Quaternion.Euler(90, 0, 0);
        }
        
        // Scale to fit bounding box
        Vector3 scale = new Vector3(radius * 2f, height, radius * 2f);
        
        // Convert to local space
        Vector3 localStart = rotation * new Vector3(0, -height * 0.5f, 0);
        Vector3 localEnd = rotation * new Vector3(0, height * 0.5f, 0);

        var data = new ShapeData
        {
            shapeType = (int)Shape.ShapeType.Cylinder,
            params1 = new Vector4(localStart.x, localStart.y, localStart.z, radius),
            params2 = new Vector4(localEnd.x, localEnd.y, localEnd.z, 0),
            params3 = Vector4.zero,
            fillColor = color,
            outlineColor = Instance.defaultOutlineColor,
            outlineThickness = wireframe ? 0.015f : Instance.defaultOutlineThickness,
            cornerRadius = 0f,
            extrusion = 0f,
            enableLighting = Instance.defaultEnableLighting ? 1f : 0f,
            smoothness = Instance.defaultSmoothness,
            matrix = Matrix4x4.TRS(center, rotation, scale)
        };
        Instance.AddImmediateShape(data);
    }

    public static void Line(Vector3 start, Vector3 end, float thickness, Color color)
    {
        Vector3 center = (start + end) * 0.5f;
        Vector3 direction = end - start;
        float length = direction.magnitude;
        
        // Calculate bounding box that encompasses the capsule
        Vector3 scale = new Vector3(thickness * 2f, length + thickness * 2f, thickness * 2f);
        
        // Calculate rotation
        Quaternion rotation = Quaternion.identity;
        if (direction != Vector3.zero)
        {
            rotation = Quaternion.LookRotation(direction, Vector3.up);
            rotation = rotation * Quaternion.Euler(90, 0, 0);
        }
        
        // Convert to local space
        Vector3 localStart = rotation * new Vector3(0, -length * 0.5f, 0);
        Vector3 localEnd = rotation * new Vector3(0, length * 0.5f, 0);

        var data = new ShapeData
        {
            shapeType = (int)Shape.ShapeType.Capsule,
            params1 = new Vector4(localStart.x, localStart.y, localStart.z, thickness),
            params2 = new Vector4(localEnd.x, localEnd.y, localEnd.z, 0),
            params3 = new Vector4(0, 0, 1, 0),
            fillColor = color,
            outlineColor = Instance.defaultOutlineColor,
            outlineThickness = Instance.defaultOutlineThickness,
            cornerRadius = 0f,
            extrusion = 0f,
            enableLighting = Instance.defaultEnableLighting ? 1f : 0f,
            smoothness = Instance.defaultSmoothness,
            matrix = Matrix4x4.TRS(center, rotation, scale)
        };
        Instance.AddImmediateShape(data);
    }

    public static void Arrow(Vector3 start, Vector3 end, float shaftRadius, float headRadius, float headLength, Color color)
    {
        // Calculate bounding box for the entire arrow
        Vector3 min = Vector3.Min(start, end);
        Vector3 max = Vector3.Max(start, end);
        float maxRadius = Mathf.Max(shaftRadius, headRadius);
        Vector3 size = (max - min) + Vector3.one * maxRadius * 2f;
        Vector3 center = (min + max) * 0.5f;
        
        var data = new ShapeData
        {
            shapeType = (int)Shape.ShapeType.Arrow,
            params1 = new Vector4(start.x, start.y, start.z, shaftRadius),
            params2 = new Vector4(end.x, end.y, end.z, headRadius),
            params3 = new Vector4(headLength, 0, 0, 0),
            fillColor = color,
            outlineColor = Instance.defaultOutlineColor,
            outlineThickness = Instance.defaultOutlineThickness,
            cornerRadius = 0f,
            extrusion = 0f,
            enableLighting = Instance.defaultEnableLighting ? 1f : 0f,
            smoothness = Instance.defaultSmoothness,
            matrix = Matrix4x4.TRS(center, Quaternion.identity, size)
        };
        Instance.AddImmediateShape(data);
    }

    public static void Arrow(Vector3 start, Vector3 end, Color color)
    {
        float totalLength = Vector3.Distance(start, end);
        float headLength = Mathf.Min(0.2f * totalLength, 0.3f);
        float shaftRadius = 0.02f * totalLength;
        float headRadius = 2f * shaftRadius;
        Arrow(start, end, shaftRadius, headRadius, headLength, color);
    }

    public static void ArrowRay(Vector3 origin, Vector3 direction, float length, Color color, float shaftRadius, float headRadius, float headLength)
    {
        Vector3 end = origin + direction.normalized * length;
        Arrow(origin, end, shaftRadius, headRadius, headLength, color);
    }

    public static void ArrowRay(Vector3 origin, Vector3 direction, float length, Color color)
    {
        Vector3 end = origin + direction.normalized * length;
        Arrow(origin, end, color);
    }

    public static void ArrowRay(Ray ray, float length, Color color)
    {
        Vector3 end = ray.origin + ray.direction.normalized * length;
        Arrow(ray.origin, end, color);
    }

    public static void CoordinateFrame(Vector3 position, Quaternion rotation, float scale = 1.0f, float axisRadius = 0.01f, float axisHeadRadius = 0.02f, float axisHeadLength = 0.05f)
    {
        Vector3 right = rotation * Vector3.right;
        Vector3 up = rotation * Vector3.up;
        Vector3 forward = rotation * Vector3.forward;

        Arrow(position, position + right * scale, axisRadius, axisHeadRadius, axisHeadLength, Color.red);
        Arrow(position, position + up * scale, axisRadius, axisHeadRadius, axisHeadLength, Color.green);
        Arrow(position, position + forward * scale, axisRadius, axisHeadRadius, axisHeadLength, Color.blue);
    }

    public static void CoordinateFrame(Matrix4x4 matrix, float scale = 1.0f, float axisRadius = 0.01f, float axisHeadRadius = 0.02f, float axisHeadLength = 0.05f)
    {
        Vector3 position = matrix.GetColumn(3);
        Quaternion rotation = Quaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1));
        CoordinateFrame(position, rotation, scale, axisRadius, axisHeadRadius, axisHeadLength);
    }

    #endregion

    #region Rendering
    void DrawUpdate()
    {
        // Clean up expired persistent shapes
        List<string> toRemove = new List<string>();
        foreach (var kvp in persistentShapes)
        {
            if (Time.time > kvp.Value.endTime)
            {
                toRemove.Add(kvp.Key);
            }
        }
        foreach (var key in toRemove)
        {
            persistentShapes.Remove(key);
        }

        // Render all shapes
        RenderShapes();

        // Clear immediate shapes after rendering
        immediateShapes.Clear();
    }

    void LateUpdate()
    {
        DrawUpdate();
    }

    void RenderShapes()
    {
        var allShapes = new List<ShapeData>(immediateShapes);
        foreach (var kvp in persistentShapes)
        {
            allShapes.Add(kvp.Value.data);
        }

        if (allShapes.Count == 0) return;

        // Expand composite shapes (like arrows) into their component shapes
        var expandedShapes = ExpandCompositeShapes(allShapes);

        // Draw in batches of 1023 (Unity's limit)
        for (int batchStart = 0; batchStart < expandedShapes.Count; batchStart += 1023)
        {
            int batchSize = Mathf.Min(1023, expandedShapes.Count - batchStart);

            var batchPropertyBlock = new MaterialPropertyBlock();

            Matrix4x4[] matrices = new Matrix4x4[batchSize];
            float[] shapeTypes = new float[batchSize];
            Vector4[] params1 = new Vector4[batchSize];
            Vector4[] params2 = new Vector4[batchSize];
            Vector4[] params3 = new Vector4[batchSize];
            Vector4[] fillColors = new Vector4[batchSize];
            Vector4[] outlineColors = new Vector4[batchSize];
            float[] outlineThickness = new float[batchSize];
            float[] cornerRadius = new float[batchSize];
            float[] extrusion = new float[batchSize];
            float[] lightingEnabled = new float[batchSize];
            float[] smoothnessArray = new float[batchSize];

            for (int i = 0; i < batchSize; i++)
            {
                var data = expandedShapes[batchStart + i];
                matrices[i] = data.matrix * Matrix4x4.Scale(Vector3.one * 1.01f);
                shapeTypes[i] = data.shapeType;
                params1[i] = data.params1;
                params2[i] = data.params2;
                params3[i] = data.params3;
                fillColors[i] = data.fillColor;
                outlineColors[i] = data.outlineColor;
                outlineThickness[i] = data.outlineThickness;
                cornerRadius[i] = data.cornerRadius;
                extrusion[i] = data.extrusion;
                lightingEnabled[i] = data.enableLighting;
                smoothnessArray[i] = data.smoothness;
            }

            batchPropertyBlock.SetFloatArray("_ShapeType", shapeTypes);
            batchPropertyBlock.SetVectorArray("_ShapeParams1", params1);
            batchPropertyBlock.SetVectorArray("_ShapeParams2", params2);
            batchPropertyBlock.SetVectorArray("_ShapeParams3", params3);
            batchPropertyBlock.SetVectorArray("_FillColor", fillColors);
            batchPropertyBlock.SetVectorArray("_OutlineColor", outlineColors);
            batchPropertyBlock.SetFloatArray("_OutlineThickness", outlineThickness);
            batchPropertyBlock.SetFloatArray("_CornerRadius", cornerRadius);
            batchPropertyBlock.SetFloatArray("_Extrusion", extrusion);
            batchPropertyBlock.SetFloatArray("_EnableLighting", lightingEnabled);
            batchPropertyBlock.SetFloatArray("_Smoothness", smoothnessArray);

            Graphics.DrawMeshInstanced(boundingCube, 0, shapeMaterial, matrices, batchSize, batchPropertyBlock);
        }
    }
    #endregion

    #region Composite Expansion
    private List<ShapeData> ExpandCompositeShapes(List<ShapeData> shapes)
    {
        var expandedShapes = new List<ShapeData>();

        for (int i = 0; i < shapes.Count; i++)
        {
            var shape = shapes[i];
            if (shape.shapeType == (int)Shape.ShapeType.Arrow)
            {
                // Extract arrow parameters
                Vector3 start = new Vector3(shape.params1.x, shape.params1.y, shape.params1.z);
                float shaftRadius = shape.params1.w;
                Vector3 end = new Vector3(shape.params2.x, shape.params2.y, shape.params2.z);
                float headRadius = shape.params2.w;
                float headLength = shape.params3.x;

                // Calculate shaft end point
                Vector3 direction = (end - start).normalized;
                float totalLength = Vector3.Distance(start, end);
                float shaftLength = Mathf.Max(0, totalLength - headLength);
                Vector3 shaftEnd = start + direction * shaftLength;
                Vector3 headBase = shaftEnd;

                // Create cylinder for shaft
                Vector3 shaftCenter = (start + shaftEnd) * 0.5f;
                Vector3 shaftDir = shaftEnd - start;
                float shaftHeight = shaftDir.magnitude;
                
                Quaternion shaftRotation = Quaternion.identity;
                if (shaftDir != Vector3.zero)
                {
                    shaftRotation = Quaternion.LookRotation(shaftDir, Vector3.up);
                    shaftRotation = shaftRotation * Quaternion.Euler(90, 0, 0);
                }
                
                Vector3 shaftScale = new Vector3(shaftRadius * 2f, shaftHeight, shaftRadius * 2f);
                Vector3 localShaftStart = shaftRotation * new Vector3(0, -shaftHeight * 0.5f, 0);
                Vector3 localShaftEnd = shaftRotation * new Vector3(0, shaftHeight * 0.5f, 0);
                
                var shaftData = shape;
                shaftData.shapeType = (int)Shape.ShapeType.Cylinder;
                shaftData.params1 = new Vector4(localShaftStart.x, localShaftStart.y, localShaftStart.z, shaftRadius);
                shaftData.params2 = new Vector4(localShaftEnd.x, localShaftEnd.y, localShaftEnd.z, 0);
                shaftData.matrix = Matrix4x4.TRS(shaftCenter, shaftRotation, shaftScale);
                expandedShapes.Add(shaftData);

                // Create cone for head
                Vector3 headCenter = (headBase + end) * 0.5f;
                Vector3 headDir = end - headBase;
                float headHeight = headDir.magnitude;
                
                Quaternion headRotation = Quaternion.identity;
                if (headDir != Vector3.zero)
                {
                    headRotation = Quaternion.LookRotation(headDir, Vector3.up);
                    headRotation = headRotation * Quaternion.Euler(90, 0, 0);
                }
                
                Vector3 headScale = new Vector3(headRadius * 2f, headHeight, headRadius * 2f);
                Vector3 localHeadBase = headRotation * new Vector3(0, -headHeight * 0.5f, 0);
                Vector3 localHeadTip = headRotation * new Vector3(0, headHeight * 0.5f, 0);
                
                var headData = shape;
                headData.shapeType = (int)Shape.ShapeType.Cone;
                headData.params1 = new Vector4(localHeadBase.x, localHeadBase.y, localHeadBase.z, headRadius);
                headData.params2 = new Vector4(localHeadTip.x, localHeadTip.y, localHeadTip.z, 0);
                headData.matrix = Matrix4x4.TRS(headCenter, headRotation, headScale);
                expandedShapes.Add(headData);
            }
            else
            {
                expandedShapes.Add(shape);
            }
        }

        return expandedShapes;
    }

    #endregion
}