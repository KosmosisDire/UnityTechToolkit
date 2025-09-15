using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Toolkit.Visualization
{
    [ExecuteAlways]
    public class DrawShape : MonoBehaviour
    {
        public enum ShapeType
        {
            Point,
            Line,
            Triangle,
            Rectangle,
            Circle,
            Polygon,
            Arrow,
            Path,
            Box,
            Sphere,
            Cylinder,
            Cone
        }

        [Header("Shape Configuration")]
        [SerializeField] public ShapeType shapeType = ShapeType.Line;
        [SerializeField] public DrawMode drawMode = DrawMode.Filled;
        [SerializeField] public Color color = Color.white;
        [SerializeField] public float animationT = 1f;

        [Header("Transform")]
        [SerializeField] public Vector3 localPosition = Vector3.zero;
        [SerializeField] public Vector3 localRotation = Vector3.zero;
        [SerializeField] public Vector3 localScale = Vector3.one;

        [Header("Common Properties")]
        [SerializeField] public float lineThickness = 0.02f;
        [SerializeField] public float radius = 1f;
        [SerializeField] public bool roundedCorners = false;
        [SerializeField] public float cornerRadius = 0.1f;

        [Header("Line/Arrow Points")]
        [SerializeField] public Vector3 startPoint = Vector3.zero;
        [SerializeField] public Vector3 endPoint = Vector3.right;

        [Header("Triangle Points")]
        [SerializeField] public Vector3 triangleA = new Vector3(-0.5f, 0, 0);
        [SerializeField] public Vector3 triangleB = new Vector3(0.5f, 0, 0);
        [SerializeField] public Vector3 triangleC = new Vector3(0, 1, 0);

        [Header("Polygon/Path Points")]
        [SerializeField]
        public List<Vector3> points = new List<Vector3> {
            new Vector3(-1, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 0, 0)
        };
        [SerializeField] public bool closedPath = true;

        [Header("Shape-Specific")]
        [SerializeField] public int segments = 32;
        [SerializeField] public bool lit = false;
        [SerializeField] public float height = 2f;
        [SerializeField] public Vector3 direction = Vector3.up;
        [SerializeField] public float arrowHeadLength = 0.3f;
        [SerializeField] public float arrowHeadAngle = 30f;
        [SerializeField] public bool fillArrowHead = true;

        void OnEnable()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EditorApplication.update += EditorUpdate;
            }
#endif
        }

        void OnDisable()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EditorApplication.update -= EditorUpdate;
            }
#endif
        }

#if UNITY_EDITOR
        void EditorUpdate()
        {
            if (!Application.isPlaying)
            {
                DrawCurrentShape();
            }
        }
#endif

        void Update()
        {
            if (Application.isPlaying)
            {
                DrawCurrentShape();
            }
        }

        void DrawCurrentShape()
        {
            Matrix4x4 shapeMatrix = GetShapeMatrix();

            switch (shapeType)
            {
                case ShapeType.Point:
                    DrawPoint(shapeMatrix);
                    break;
                case ShapeType.Line:
                    DrawLine();
                    break;
                case ShapeType.Triangle:
                    DrawTriangle(shapeMatrix, drawMode);
                    break;
                case ShapeType.Rectangle:
                    DrawRectangle(shapeMatrix, drawMode);
                    break;
                case ShapeType.Circle:
                    DrawCircle(shapeMatrix, drawMode);
                    break;
                case ShapeType.Polygon:
                    DrawPolygon(shapeMatrix, drawMode);
                    break;
                case ShapeType.Arrow:
                    DrawArrow();
                    break;
                case ShapeType.Path:
                    DrawPath();
                    break;
                case ShapeType.Box:
                    DrawBox(shapeMatrix, drawMode);
                    break;
                case ShapeType.Sphere:
                    DrawSphere(shapeMatrix, drawMode);
                    break;
                case ShapeType.Cone:
                    DrawCone(shapeMatrix, drawMode);
                    break;
                case ShapeType.Cylinder:
                    DrawCylinder(shapeMatrix, drawMode);
                    break;
            }
        }

        Matrix4x4 GetShapeMatrix()
        {
            Vector3 worldPos = transform.TransformPoint(localPosition);
            Quaternion worldRot = transform.rotation * Quaternion.Euler(localRotation);
            Vector3 worldScale = Vector3.Scale(localScale, transform.lossyScale);
            return Matrix4x4.TRS(worldPos, worldRot, worldScale);
        }

        void DrawPoint(Matrix4x4 matrix)
        {
            SphereSettings settings = SphereSettings.Default;
            settings.baseSettings.color = color;
            settings.baseSettings.drawMode = DrawMode.Filled;
            settings.radius = radius;
            settings.baseSettings.lit = lit;

            Draw.Sphere(matrix, settings);
        }

        void DrawLine()
        {
            LineSettings settings = LineSettings.Default;
            settings.baseSettings.color = color;
            settings.baseSettings.lineThickness = lineThickness * GetAverageScale();
            settings.baseSettings.t = animationT;

            Vector3 worldStart = transform.TransformPoint(startPoint);
            Vector3 worldEnd = transform.TransformPoint(endPoint);
            Draw.Line(worldStart, worldEnd, settings);
        }

        void DrawTriangle(Matrix4x4 matrix, DrawMode mode)
        {
            TriangleSettings settings = TriangleSettings.Default;
            settings.baseSettings.color = color;
            settings.baseSettings.drawMode = mode;
            settings.baseSettings.lineThickness = lineThickness * GetAverageScale();
            settings.baseSettings.t = animationT;
            settings.roundedCorners = roundedCorners;
            settings.cornerRadius = cornerRadius * GetAverageScale();
            settings.vertices = new Vector3[] { triangleA, triangleB, triangleC };

            Draw.Triangle(matrix, settings);
        }

        void DrawRectangle(Matrix4x4 matrix, DrawMode mode)
        {
            QuadSettings settings = QuadSettings.Default;
            settings.baseSettings.color = color;
            settings.baseSettings.drawMode = mode;
            settings.baseSettings.lineThickness = lineThickness * GetAverageScale();
            settings.baseSettings.t = animationT;
            settings.roundedCorners = roundedCorners;
            settings.cornerRadius = cornerRadius * GetAverageScale();

            Draw.Quad(matrix, settings);
        }

        void DrawCircle(Matrix4x4 matrix, DrawMode mode)
        {
            CircleSettings settings = CircleSettings.Default;
            settings.baseSettings.color = color;
            settings.baseSettings.drawMode = mode;
            settings.baseSettings.lineThickness = lineThickness * GetAverageScale();
            settings.baseSettings.t = animationT;
            settings.radius = radius;
            settings.baseSettings.lit = lit;

            Draw.Circle(matrix, settings);
        }

        void DrawPolygon(Matrix4x4 matrix, DrawMode mode)
        {
            if (points.Count < 3) return;

            PolygonSettings settings = PolygonSettings.Default;
            settings.baseSettings.color = color;
            settings.baseSettings.drawMode = mode;
            settings.baseSettings.lineThickness = lineThickness * GetAverageScale();
            settings.baseSettings.t = animationT;
            settings.closed = closedPath;
            settings.vertices = points.ToArray();

            Draw.Polygon(matrix, settings);
        }

        void DrawArrow()
        {
            ArrowSettings settings = ArrowSettings.Default;
            settings.baseSettings.color = color;
            settings.baseSettings.lineThickness = lineThickness * GetAverageScale();
            settings.headLength = arrowHeadLength * GetAverageScale();
            settings.headAngleDegrees = arrowHeadAngle;
            settings.baseSettings.drawMode = drawMode;
            settings.baseSettings.t = animationT;

            Vector3 worldStart = transform.TransformPoint(startPoint);
            Vector3 worldEnd = transform.TransformPoint(endPoint);
            Draw.Arrow(worldStart, worldEnd, settings);
        }

        void DrawPath()
        {
            if (points.Count < 2) return;

            PathSettings settings = PathSettings.Default;
            settings.baseSettings.color = color;
            settings.baseSettings.lineThickness = lineThickness * GetAverageScale();
            settings.closed = closedPath;
            settings.roundedJoints = roundedCorners;
            settings.baseSettings.t = animationT;

            Vector3[] worldPoints = new Vector3[points.Count];
            for (int i = 0; i < points.Count; i++)
                worldPoints[i] = transform.TransformPoint(points[i]);

            Draw.Path(worldPoints, settings);
        }

        void DrawBox(Matrix4x4 matrix, DrawMode mode)
        {
            BoxSettings settings = BoxSettings.Default;
            settings.baseSettings.color = color;
            settings.baseSettings.drawMode = mode;
            settings.baseSettings.lineThickness = lineThickness * GetAverageScale();
            settings.baseSettings.t = animationT;
            settings.roundedCorners = roundedCorners;
            settings.cornerRadius = cornerRadius * GetAverageScale();

            Draw.Box(matrix, settings);
        }

        void DrawSphere(Matrix4x4 matrix, DrawMode mode)
        {
            SphereSettings settings = SphereSettings.Default;
            settings.baseSettings.color = color;
            settings.baseSettings.drawMode = mode;
            settings.baseSettings.lineThickness = lineThickness * GetAverageScale();
            settings.baseSettings.t = animationT;
            settings.baseSettings.lit = lit;
            settings.radius = radius;
            settings.segments = segments;

            Draw.Sphere(matrix, settings);
        }

        void DrawCone(Matrix4x4 matrix, DrawMode mode)
        {
            ConeSettings settings = ConeSettings.Default;
            settings.baseSettings.color = color;
            settings.baseSettings.drawMode = mode;
            settings.baseSettings.lineThickness = lineThickness * GetAverageScale();
            settings.baseSettings.t = animationT;
            settings.segments = segments;

            Draw.Cone(matrix, settings);
        }

        void DrawCylinder(Matrix4x4 matrix, DrawMode mode)
        {
            CylinderSettings settings = CylinderSettings.Default;
            settings.baseSettings.color = color;
            settings.baseSettings.drawMode = mode;
            settings.baseSettings.lineThickness = lineThickness * GetAverageScale();
            settings.baseSettings.t = animationT;
            settings.segments = segments;

            Draw.Cylinder(matrix, settings);
        }

        float GetAverageScale()
        {
            Vector3 scale = transform.lossyScale;
            return (scale.x + scale.y + scale.z) / 3f;
        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(DrawShape))]
    public class DrawShapeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawShape shape = (DrawShape)target;

            // Core settings
            EditorGUILayout.LabelField("Shape Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("shapeType"));

            EditorGUILayout.Space();

            // Shape-specific parameters using reflection
            DrawShapeSpecificParameters(shape);

            serializedObject.ApplyModifiedProperties();
        }

        void DrawShapeSpecificParameters(DrawShape shape)
        {
            EditorGUILayout.LabelField("Shape Parameters", EditorStyles.boldLabel);

            // Get the settings type for the current shape
            System.Type settingsType = GetSettingsTypeForShape(shape.shapeType);
            if (settingsType != null)
            {
                DrawSettingsUsingReflection(settingsType, shape.shapeType.ToString() + " Settings");
            }

            // Draw additional shape-specific fields
            DrawAdditionalShapeFields(shape);
        }

        System.Type GetSettingsTypeForShape(DrawShape.ShapeType shapeType)
        {
            switch (shapeType)
            {
                case DrawShape.ShapeType.Point:
                case DrawShape.ShapeType.Circle:
                case DrawShape.ShapeType.Sphere:
                    return typeof(SphereSettings);
                case DrawShape.ShapeType.Line:
                    return typeof(LineSettings);
                case DrawShape.ShapeType.Triangle:
                    return typeof(TriangleSettings);
                case DrawShape.ShapeType.Rectangle:
                    return typeof(QuadSettings);
                case DrawShape.ShapeType.Polygon:
                    return typeof(PolygonSettings);
                case DrawShape.ShapeType.Arrow:
                    return typeof(ArrowSettings);
                case DrawShape.ShapeType.Path:
                    return typeof(PathSettings);
                case DrawShape.ShapeType.Box:
                    return typeof(BoxSettings);
                case DrawShape.ShapeType.Cylinder:
                    return typeof(CylinderSettings);
                case DrawShape.ShapeType.Cone:
                    return typeof(ConeSettings);
                default:
                    return null;
            }
        }

        void DrawSettingsUsingReflection(System.Type settingsType, string label)
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            // Get all public fields from the settings struct
            var fields = settingsType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            foreach (var field in fields)
            {
                DrawFieldBasedOnType(field);
            }
        }

        void DrawFieldBasedOnType(System.Reflection.FieldInfo field)
        {
            string fieldName = field.Name;

            // Skip baseSettings as we'll handle those separately
            if (fieldName == "baseSettings")
            {
                DrawBaseSettingsFields();
                return;
            }

            // Try to find corresponding property in DrawShape
            var property = serializedObject.FindProperty(fieldName);
            if (property != null)
            {
                EditorGUILayout.PropertyField(property, new GUIContent(ObjectNames.NicifyVariableName(fieldName)));
            }
        }

        void DrawBaseSettingsFields()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("color"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("drawMode"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("lineThickness"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("animationT"), new GUIContent("T (Animation)"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("lit"));
        }

        void DrawAdditionalShapeFields(DrawShape shape)
        {
            switch (shape.shapeType)
            {
                case DrawShape.ShapeType.Line:
                case DrawShape.ShapeType.Arrow:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("startPoint"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("endPoint"));
                    break;

                case DrawShape.ShapeType.Triangle:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("triangleA"), new GUIContent("Vertex A"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("triangleB"), new GUIContent("Vertex B"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("triangleC"), new GUIContent("Vertex C"));
                    break;

                case DrawShape.ShapeType.Polygon:
                case DrawShape.ShapeType.Path:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("points"), new GUIContent("Points"), true);
                    break;

                case DrawShape.ShapeType.Cone:
                case DrawShape.ShapeType.Cylinder:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("direction"));
                    break;
            }
        }
    }
#endif
}