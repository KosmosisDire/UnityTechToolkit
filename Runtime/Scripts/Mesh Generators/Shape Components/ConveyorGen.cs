using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum ConveyorMode
{
    Pill,
    Roller
}

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ConveyorGen : MonoBehaviour
{
    [Header("Conveyor Mode")]
    [SerializeField] private ConveyorMode conveyorMode = ConveyorMode.Pill;
    
    [Header("Basic Parameters")]
    [SerializeField, Range(0.2f, 20f)] public float length = 4f;
    [SerializeField, Range(0.03f, 2f)] public float width = 0.2f;
    
    [Header("Pill Shape Parameters")]
    [SerializeField, Range(0.01f, 0.2f)] private float pillRadius = 0.1f;
    
    [Header("Roller Parameters")]
    [SerializeField, Range(0.01f, 0.2f)] private float rollerRadius = 0.05f;
    [SerializeField, Range(0.01f, 1f)] private float rollerSpacing = 0.2f;
    [SerializeField, Range(0.01f, 0.5f), Tooltip("Thickness of each roller in the length direction")] 
    private float rollerThickness = 0.02f;
    
    [Header("Quality Settings")]
    [SerializeField, Range(6, 64)] private int semicircleSegments = 16;
    [SerializeField, Range(6, 32)] private int rollerSegments = 12;
    [SerializeField, Range(1, 32), Tooltip("Number of subdivisions along the width (extrusion). Higher values provide more geometry for better physics contact points.")] 
    private int extrusionSubdivisions = 1;
    
    [Header("UV Settings")]
    [SerializeField] private bool generateUVs = true;
    [SerializeField] private float uvScale = 1f;
    [SerializeField, Tooltip("World units per UV unit (1 means 1 world unit = 1 UV unit)")] 
    private float worldUnitsPerUV = 1f;
    [SerializeField, Tooltip("Maintain 1:1 aspect ratio and scale UVs with width instead of using worldUnitsPerUV")]
    private bool maintainAspectRatio = false;
    
    [Header("Debug")]
    [SerializeField] private bool autoUpdate = true;
    
    private MeshFilter meshFilter;
    private Mesh generatedMesh;
    
    // Store previous values to detect changes
    private ConveyorMode lastConveyorMode = ConveyorMode.Pill;
    private float lastLength = -1f, lastPillRadius = -1f, lastWidth = -1f;
    private float lastRollerRadius = -1f, lastRollerSpacing = -1f, lastRollerThickness = -1f;
    private int lastSemicircleSegments = -1, lastRollerSegments = -1;
    private int lastExtrusionSubdivisions = -1;
    private bool lastGenerateUVs;
    private float lastUvScale = -1f;
    private float lastWorldUnitsPerUV = -1f;
    private bool lastMaintainAspectRatio;
    private bool initialized = false;
    
    void Awake()
    {
        InitializeComponents();
    }

    void Start()
    {
        InitializeComponents();
        if (!initialized)
        {
            GenerateMesh();
            initialized = true;
        }
    }
    
    void Update()
    {
        // Only auto-update in play mode to avoid performance issues in editor
        if (Application.isPlaying && autoUpdate && HasParametersChanged())
        {
            GenerateMesh();
            StoreCurrentValues();
        }
    }
    
    void OnValidate()
    {
        // Initialize components if needed
        InitializeComponents();

        EditorApplication.delayCall += () =>
        {
            Debug.Log("OnValidate called for ConveyorGen on " + gameObject.name);
            // Always regenerate in editor when values change
            if (!Application.isPlaying || autoUpdate)
            {
                if (HasParametersChanged())
                {
                    GenerateMesh();
                    StoreCurrentValues();
                }
            }
        };
    }
    
    private void InitializeComponents()
    {
        if (meshFilter == null)
        {
            meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                meshFilter = gameObject.AddComponent<MeshFilter>();
            }
        }
        
        // Ensure we have a MeshRenderer
        if (GetComponent<MeshRenderer>() == null)
        {
            gameObject.AddComponent<MeshRenderer>();
        }
    }
    
    private bool HasParametersChanged()
    {
        return conveyorMode != lastConveyorMode ||
               !Mathf.Approximately(length, lastLength) ||
               !Mathf.Approximately(pillRadius, lastPillRadius) ||
               !Mathf.Approximately(rollerRadius, lastRollerRadius) ||
               !Mathf.Approximately(rollerSpacing, lastRollerSpacing) ||
               !Mathf.Approximately(rollerThickness, lastRollerThickness) ||
               !Mathf.Approximately(width, lastWidth) ||
               semicircleSegments != lastSemicircleSegments ||
               rollerSegments != lastRollerSegments ||
               extrusionSubdivisions != lastExtrusionSubdivisions ||
               generateUVs != lastGenerateUVs ||
               !Mathf.Approximately(uvScale, lastUvScale) ||
               !Mathf.Approximately(worldUnitsPerUV, lastWorldUnitsPerUV) ||
               maintainAspectRatio != lastMaintainAspectRatio ||
               lastLength < 0; // Force initial generation
    }
    
    private void StoreCurrentValues()
    {
        lastConveyorMode = conveyorMode;
        lastLength = length;
        lastPillRadius = pillRadius;
        lastRollerRadius = rollerRadius;
        lastRollerSpacing = rollerSpacing;
        lastRollerThickness = rollerThickness;
        lastWidth = width;
        lastSemicircleSegments = semicircleSegments;
        lastRollerSegments = rollerSegments;
        lastExtrusionSubdivisions = extrusionSubdivisions;
        lastGenerateUVs = generateUVs;
        lastUvScale = uvScale;
        lastWorldUnitsPerUV = worldUnitsPerUV;
        lastMaintainAspectRatio = maintainAspectRatio;
    }
    
    [ContextMenu("Regenerate Mesh")]
    public void GenerateMesh()
    {
        InitializeComponents();
        
        // Properly dispose of old mesh
        if (generatedMesh != null)
        {
            if (Application.isPlaying)
            {
                Destroy(generatedMesh);
            }
            else
            {
                DestroyImmediate(generatedMesh);
            }
        }

        // Create new mesh
        generatedMesh = new()
        {
            name = conveyorMode == ConveyorMode.Pill ? "Generated Pill Conveyor" : "Generated Roller Conveyor"
        };

        // Generate the mesh data based on mode
        if (conveyorMode == ConveyorMode.Pill)
        {
            GeneratePillMesh();
        }
        else
        {
            GenerateRollerMesh();
        }
        
        // Assign to mesh filter
        if (meshFilter != null)
        {
            meshFilter.sharedMesh = generatedMesh;
            UpdateMeshCollider();
        }
    }
    
    private void GenerateRollerMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        
        // Calculate how many rollers we can fit
        int rollerCount = Mathf.FloorToInt(length / rollerSpacing);
        if (rollerCount < 1) rollerCount = 1;
        
        // Adjust spacing to fit evenly
        float actualSpacing = length / rollerCount;
        float startX = -length * 0.5f + actualSpacing * 0.5f;
        
        // Generate each roller
        for (int rollerIndex = 0; rollerIndex < rollerCount; rollerIndex++)
        {
            float xPos = startX + rollerIndex * actualSpacing;
            GenerateCylinder(vertices, triangles, uvs, xPos, rollerRadius, rollerThickness, width, rollerSegments);
        }
        
        // Apply to mesh
        generatedMesh.Clear();
        generatedMesh.vertices = vertices.ToArray();
        generatedMesh.triangles = triangles.ToArray();
        
        if (generateUVs && uvs.Count > 0)
        {
            generatedMesh.uv = uvs.ToArray();
        }
        
        // Let Unity calculate normals automatically
        generatedMesh.RecalculateNormals();
        generatedMesh.RecalculateBounds();
        generatedMesh.RecalculateTangents();
        
        // Optimize the mesh
        generatedMesh.Optimize();
    }
    
    private void GenerateCylinder(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, 
                                 float xPos, float radius, float thickness, float height, int segments)
    {
        int startVertexIndex = vertices.Count;
        float halfThickness = thickness * 0.5f;
        float halfHeight = height * 0.5f;
        
        // Calculate UV scaling
        float uvScaleX, uvScaleY;
        GetUVScaling(out uvScaleX, out uvScaleY);
        
        // Generate vertices for the cylinder
        // We'll create rings of vertices along the Z-axis (width direction)
        int zSubdivisions = 2; // Just two rings for now (front and back)
        
        for (int zRing = 0; zRing < zSubdivisions; zRing++)
        {
            float z = Mathf.Lerp(-halfHeight, halfHeight, (float)zRing / (zSubdivisions - 1));
            
            for (int i = 0; i < segments; i++)
            {
                float angle = 2f * Mathf.PI * i / segments;
                
                // Create circular cross-section in X-Y plane
                float x = xPos + radius * Mathf.Cos(angle);
                float y = radius * Mathf.Sin(angle);
                
                Vector3 vertex = new Vector3(x, y, z);
                vertices.Add(vertex);
                
                if (generateUVs)
                {
                    // Cylindrical UV mapping
                    float u = (float)i / segments * uvScaleX;
                    float v = (float)zRing / (zSubdivisions - 1) * uvScaleY;
                    uvs.Add(new Vector2(u, v));
                }
            }
        }
        
        // Generate side faces connecting the rings
        for (int i = 0; i < segments; i++)
        {
            int next = (i + 1) % segments;
            
            // First ring indices
            int ring0Current = startVertexIndex + i;
            int ring0Next = startVertexIndex + next;
            
            // Second ring indices  
            int ring1Current = startVertexIndex + segments + i;
            int ring1Next = startVertexIndex + segments + next;
            
            // Create two triangles for each quad with correct winding order for outward normals
            triangles.Add(ring0Current);
            triangles.Add(ring1Next);
            triangles.Add(ring1Current);
            
            triangles.Add(ring0Current);
            triangles.Add(ring0Next);
            triangles.Add(ring1Next);
        }
        
        // Generate end caps
        GenerateCylinderCap(vertices, triangles, uvs, xPos, radius, -halfHeight, segments, false); // Back cap
        GenerateCylinderCap(vertices, triangles, uvs, xPos, radius, halfHeight, segments, true);   // Front cap
    }
    
    private void GenerateCylinderCap(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs,
                                    float xPos, float radius, float z, int segments, bool isFront)
    {
        int centerIndex = vertices.Count;
        
        // Add center vertex
        vertices.Add(new Vector3(xPos, 0, z));
        if (generateUVs) uvs.Add(new Vector2(0.5f, 0.5f));
        
        // Calculate UV scaling
        float uvScaleX, uvScaleY;
        GetUVScaling(out uvScaleX, out uvScaleY);
        
        // Add ring vertices for the cap
        for (int i = 0; i < segments; i++)
        {
            float angle = 2f * Mathf.PI * i / segments;
            float x = xPos + radius * Mathf.Cos(angle);
            float y = radius * Mathf.Sin(angle);
            
            vertices.Add(new Vector3(x, y, z));
            
            if (generateUVs)
            {
                // Radial UV mapping for caps
                float u = 0.5f + 0.5f * Mathf.Cos(angle) * uvScaleX;
                float v = 0.5f + 0.5f * Mathf.Sin(angle) * uvScaleY;
                uvs.Add(new Vector2(u, v));
            }
        }
        
        // Generate triangles for the cap
        for (int i = 0; i < segments; i++)
        {
            int next = (i + 1) % segments;
            
            if (isFront)
            {
                triangles.Add(centerIndex);
                triangles.Add(centerIndex + 1 + i);
                triangles.Add(centerIndex + 1 + next);
            }
            else
            {
                triangles.Add(centerIndex);
                triangles.Add(centerIndex + 1 + next);
                triangles.Add(centerIndex + 1 + i);
            }
        }
    }
    
    private void GeneratePillMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        
        float halfWidth = width * 0.5f;
        
        // Calculate straight section length
        float straightLength = length - 2f * pillRadius;
        
        // Generate the 2D pill profile points
        List<Vector2> profilePoints = GeneratePillProfile(straightLength, pillRadius);
        
        // Create front and back faces (end caps)
        GenerateEndCaps(vertices, triangles, uvs, profilePoints, halfWidth, true);  // Front
        GenerateEndCaps(vertices, triangles, uvs, profilePoints, -halfWidth, false); // Back
        
        // Create side faces (extrusion)
        GenerateSideFaces(vertices, triangles, uvs, profilePoints, halfWidth);
        
        // Apply to mesh
        generatedMesh.Clear();
        generatedMesh.vertices = vertices.ToArray();
        generatedMesh.triangles = triangles.ToArray();
        
        if (generateUVs && uvs.Count > 0)
        {
            generatedMesh.uv = uvs.ToArray();
        }
        
        // Let Unity calculate normals automatically
        generatedMesh.RecalculateNormals();
        generatedMesh.RecalculateBounds();
        generatedMesh.RecalculateTangents();
        
        // Optimize the mesh
        generatedMesh.Optimize();
    }
    
    private List<Vector2> GeneratePillProfile(float straightLength, float radius)
    {
        List<Vector2> points = new List<Vector2>();
        
        float halfStraight = straightLength * 0.5f;
        
        // Generate profile going counterclockwise starting from bottom-right
        
        // 1. Right semicircle (bottom to top)
        for (int i = 0; i <= semicircleSegments; i++)
        {
            float angle = -Mathf.PI * 0.5f + Mathf.PI * i / semicircleSegments;
            float x = halfStraight + radius * Mathf.Cos(angle);
            float y = radius * Mathf.Sin(angle);
            points.Add(new Vector2(x, y));
        }
        
        // 2. Top straight line (right to left)
        if (straightLength > 0.001f)
        {
            points.Add(new Vector2(-halfStraight, radius));
        }
        
        // 3. Left semicircle (top to bottom)
        for (int i = 1; i <= semicircleSegments; i++)
        {
            float angle = Mathf.PI * 0.5f + Mathf.PI * i / semicircleSegments;
            float x = -halfStraight + radius * Mathf.Cos(angle);
            float y = radius * Mathf.Sin(angle);
            points.Add(new Vector2(x, y));
        }
        
        // 4. Bottom straight line (left to right)
        if (straightLength > 0.001f)
        {
            points.Add(new Vector2(halfStraight, -radius));
        }
        
        return points;
    }
    
    private void GenerateEndCaps(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, 
                                List<Vector2> profilePoints, float zPos, bool isFront)
    {
        int startIndex = vertices.Count;
        
        // Add center vertex
        vertices.Add(new Vector3(0, 0, zPos));
        if (generateUVs) uvs.Add(new Vector2(0.5f, 0.5f));
        
        // Calculate bounds for centering UVs
        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;
        
        foreach (Vector2 point in profilePoints)
        {
            minX = Mathf.Min(minX, point.x);
            maxX = Mathf.Max(maxX, point.x);
            minY = Mathf.Min(minY, point.y);
            maxY = Mathf.Max(maxY, point.y);
        }
        
        float centerX = (minX + maxX) * 0.5f;
        float centerY = (minY + maxY) * 0.5f;
        
        // Calculate UV scaling based on the selected mode
        float uvScaleX, uvScaleY;
        GetUVScaling(out uvScaleX, out uvScaleY);
        
        // Add profile vertices
        foreach (Vector2 point in profilePoints)
        {
            vertices.Add(new Vector3(point.x, point.y, zPos));
            if (generateUVs)
            {
                float uvX = (point.x - centerX) * uvScaleX + 0.5f;
                float uvY = (point.y - centerY) * uvScaleY + 0.5f;
                uvs.Add(new Vector2(uvX, uvY));
            }
        }
        
        // Generate triangles for the cap
        for (int i = 0; i < profilePoints.Count; i++)
        {
            int next = (i + 1) % profilePoints.Count;
            
            if (isFront)
            {
                triangles.Add(startIndex);
                triangles.Add(startIndex + 1 + i);
                triangles.Add(startIndex + 1 + next);
            }
            else
            {
                triangles.Add(startIndex);
                triangles.Add(startIndex + 1 + next);
                triangles.Add(startIndex + 1 + i);
            }
        }
    }
    
    private void GenerateSideFaces(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, 
                                  List<Vector2> profilePoints, float halfWidth)
    {
        int profileCount = profilePoints.Count;
        float perimeter = 0f;
        List<float> cumulativeDistances = new List<float>();
        
        // Calculate perimeter for UV mapping
        cumulativeDistances.Add(0f);
        for (int i = 0; i < profileCount; i++)
        {
            int next = (i + 1) % profileCount;
            float edgeLength = Vector2.Distance(profilePoints[i], profilePoints[next]);
            perimeter += edgeLength;
            cumulativeDistances.Add(perimeter);
        }
        
        // Calculate UV scaling based on the selected mode
        float uvScaleX, uvScaleY;
        GetUVScaling(out uvScaleX, out uvScaleY);
        
        for (int i = 0; i < profileCount; i++)
        {
            int next = (i + 1) % profileCount;
            
            Vector2 current = profilePoints[i];
            Vector2 nextPoint = profilePoints[next];
            
            // Generate vertices along the extrusion with subdivisions
            List<Vector3> edgeVertices = new List<Vector3>();
            List<Vector2> edgeUVs = new List<Vector2>();
            
            // Generate vertices from back to front with subdivisions
            for (int sub = 0; sub <= extrusionSubdivisions; sub++)
            {
                float t = (float)sub / extrusionSubdivisions;
                float z = Mathf.Lerp(-halfWidth, halfWidth, t);
                
                edgeVertices.Add(new Vector3(current.x, current.y, z));
                edgeVertices.Add(new Vector3(nextPoint.x, nextPoint.y, z));
                
                if (generateUVs)
                {
                    float uStart = cumulativeDistances[i] * uvScaleX;
                    float uEnd = cumulativeDistances[i + 1] * uvScaleX;
                    float v = (z + halfWidth) * uvScaleY;
                    
                    edgeUVs.Add(new Vector2(uStart, v));
                    edgeUVs.Add(new Vector2(uEnd, v));
                }
            }
            
            // Add all vertices for this edge
            int startIndex = vertices.Count;
            vertices.AddRange(edgeVertices);
            if (generateUVs) uvs.AddRange(edgeUVs);
            
            // Generate triangles for each subdivision segment
            for (int sub = 0; sub < extrusionSubdivisions; sub++)
            {
                int baseIndex = startIndex + sub * 2;
                
                triangles.Add(baseIndex);
                triangles.Add(baseIndex + 1);
                triangles.Add(baseIndex + 3);
                
                triangles.Add(baseIndex);
                triangles.Add(baseIndex + 3);
                triangles.Add(baseIndex + 2);
            }
        }
    }
    
    private void GetUVScaling(out float uvScaleX, out float uvScaleY)
    {
        if (maintainAspectRatio)
        {
            float widthBasedScale = uvScale / width;
            uvScaleX = widthBasedScale;
            uvScaleY = widthBasedScale;
        }
        else
        {
            float originalScale = uvScale / worldUnitsPerUV;
            uvScaleX = originalScale;
            uvScaleY = originalScale;
        }
    }
    
    private void UpdateMeshCollider()
    {
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        if (meshCollider != null && generatedMesh != null)
        {
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = generatedMesh;
            
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(meshCollider);
            }
#endif
        }
    }
    
    void OnEnable()
    {
        if (initialized && meshFilter != null && meshFilter.sharedMesh == null)
        {
            GenerateMesh();
        }
    }
    
    void OnDisable()
    {
        CleanupMesh();
    }
    
    void OnDestroy()
    {
        CleanupMesh();
    }
    
    private void CleanupMesh()
    {
        if (generatedMesh != null)
        {
            if (Application.isPlaying)
            {
                Destroy(generatedMesh);
            }
            else
            {
                DestroyImmediate(generatedMesh);
            }
            generatedMesh = null;
        }
    }
    
#if UNITY_EDITOR
    void Reset()
    {
        conveyorMode = ConveyorMode.Pill;
        length = 4f;
        pillRadius = 0.1f;
        rollerRadius = 0.05f;
        rollerSpacing = 0.2f;
        rollerThickness = 0.02f;
        width = 0.2f;
        semicircleSegments = 16;
        rollerSegments = 12;
        extrusionSubdivisions = 1;
        generateUVs = true;
        uvScale = 1f;
        worldUnitsPerUV = 1f;
        maintainAspectRatio = false;
        autoUpdate = true;
        
        InitializeComponents();
        GenerateMesh();
    }
#endif
}