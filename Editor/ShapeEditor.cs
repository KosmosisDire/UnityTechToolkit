using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

[CustomEditor(typeof(Shape))]
[CanEditMultipleObjects]
public class ShapeEditor : Editor
{
    #region Property Fields
    private class PropertyField
    {
        public FieldInfo field;
        public ShapeFieldAttribute attribute;
        public bool hasMultipleValues;
        public object currentValue;
        public Type declaringType;
    }

    private List<PropertyField> commonProperties = new List<PropertyField>();
    private bool hasMixedTypes;
    private Shape.ShapeType? commonType;
    
    private SerializedProperty fillProp;
    private SerializedProperty fillColorProp;
    private SerializedProperty outlineColorProp;
    private SerializedProperty outlineThicknessProp;
    private SerializedProperty enableLightingProp;
    private SerializedProperty smoothnessProp;
    private SerializedProperty useTransformPositionProp;
    private SerializedProperty useTransformRotationProp;
    private SerializedProperty useTransformScaleProp;
    private SerializedProperty localOffsetProp;
    
    private GUIStyle headerStyle;
    #endregion

    #region Unity Callbacks
    void OnEnable()
    {
        fillProp = serializedObject.FindProperty("fill");
        fillColorProp = serializedObject.FindProperty("fillColor");
        outlineColorProp = serializedObject.FindProperty("outlineColor");
        outlineThicknessProp = serializedObject.FindProperty("outlineThickness");
        enableLightingProp = serializedObject.FindProperty("enableLighting");
        smoothnessProp = serializedObject.FindProperty("smoothness");
        useTransformPositionProp = serializedObject.FindProperty("useTransformPosition");
        useTransformRotationProp = serializedObject.FindProperty("useTransformRotation");
        useTransformScaleProp = serializedObject.FindProperty("useTransformScale");
        localOffsetProp = serializedObject.FindProperty("localOffset");
        
        RefreshCommonProperties();
    }

    public override void OnInspectorGUI()
    {
        InitializeStyles();
        serializedObject.Update();
        
        // Shape Type Section
        EditorGUILayout.Space(5);
        DrawHeader("Shape Type");
        DrawShapeTypeSelector();
        
        // Shape Parameters
        if (commonProperties.Count > 0)
        {
            EditorGUILayout.Space(10);
            DrawHeader("Parameters");
            DrawShapeParameters();
        }
        
        // Appearance
        EditorGUILayout.Space(10);
        DrawHeader("Appearance");
        DrawAppearanceProperties();
        
        // Lighting
        EditorGUILayout.Space(10);
        DrawHeader("Lighting");
        DrawLightingProperties();
        
        // Transform
        EditorGUILayout.Space(10);
        DrawHeader("Transform");
        DrawTransformProperties();
        
        serializedObject.ApplyModifiedProperties();
        
        // Utility buttons
        EditorGUILayout.Space(10);
        DrawUtilityButtons();
    }
    #endregion

    #region Property Management
    void RefreshCommonProperties()
    {
        commonProperties.Clear();
        var shapes = targets.Cast<Shape>().Select(s => s.shapeData).Where(s => s != null).ToArray();
        
        if (shapes.Length == 0) return;
        
        // Check if all shapes are the same type
        var firstType = shapes[0].GetType();
        hasMixedTypes = shapes.Any(s => s.GetType() != firstType);
        commonType = hasMixedTypes ? null : shapes[0].Type;
        
        if (!hasMixedTypes)
        {
            // All same type - get all properties with ShapeField attribute
            var fields = firstType.GetFields()
                .Where(f => f.GetCustomAttribute<ShapeFieldAttribute>() != null)
                .ToList();
            
            foreach (var field in fields)
            {
                var prop = new PropertyField
                {
                    field = field,
                    attribute = field.GetCustomAttribute<ShapeFieldAttribute>(),
                    declaringType = firstType
                };
                
                // Check if values are consistent across all selected objects
                var firstValue = field.GetValue(shapes[0]);
                prop.hasMultipleValues = shapes.Skip(1).Any(s => !ValuesEqual(field.GetValue(s), firstValue));
                prop.currentValue = firstValue;
                
                commonProperties.Add(prop);
            }
        }
        else
        {
            // Mixed types - find truly common properties
            var commonFields = new Dictionary<string, FieldInfo>();
            
            // Get intersection of all fields
            foreach (var shapeType in shapes.Select(s => s.GetType()).Distinct())
            {
                var fields = shapeType.GetFields()
                    .Where(f => f.GetCustomAttribute<ShapeFieldAttribute>() != null);
                
                if (commonFields.Count == 0)
                {
                    // First type - add all fields
                    foreach (var field in fields)
                        commonFields[field.Name] = field;
                }
                else
                {
                    // Keep only fields that exist in this type too
                    commonFields = commonFields
                        .Where(kvp => fields.Any(f => f.Name == kvp.Key && f.FieldType == kvp.Value.FieldType))
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }
            }
            
            // Create PropertyField for each common field
            foreach (var kvp in commonFields)
            {
                var fieldName = kvp.Key;
                var fieldInfo = kvp.Value;
                
                var prop = new PropertyField
                {
                    field = fieldInfo,
                    attribute = fieldInfo.GetCustomAttribute<ShapeFieldAttribute>(),
                    declaringType = null // Mixed types
                };
                
                // Check values across all objects
                var values = shapes.Select(s => s.GetType().GetField(fieldName)?.GetValue(s)).ToArray();
                var firstValue = values[0];
                prop.hasMultipleValues = values.Skip(1).Any(v => !ValuesEqual(v, firstValue));
                prop.currentValue = firstValue;
                
                commonProperties.Add(prop);
            }
        }
    }
    #endregion

    #region Drawing Methods
    void DrawShapeTypeSelector()
    {
        EditorGUI.BeginChangeCheck();
        
        if (hasMixedTypes)
        {
            EditorGUILayout.HelpBox("Multiple shape types selected", MessageType.Info);
            var newType = (Shape.ShapeType)EditorGUILayout.EnumPopup("New Shape Type", Shape.ShapeType.Box);
            if (GUILayout.Button("Unify Shape Types"))
            {
                Undo.RecordObjects(targets, "Unify Shape Types");
                foreach (Shape shape in targets)
                {
                    shape.SetShapeType(newType);
                    EditorUtility.SetDirty(shape);
                }
                RefreshCommonProperties();
            }
        }
        else if (commonType.HasValue)
        {
            var newType = (Shape.ShapeType)EditorGUILayout.EnumPopup("Shape Type", commonType.Value);
            if (newType != commonType.Value)
            {
                Undo.RecordObjects(targets, "Change Shape Type");
                foreach (Shape shape in targets)
                {
                    shape.SetShapeType(newType);
                    EditorUtility.SetDirty(shape);
                }
                RefreshCommonProperties();
            }
        }
        
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }
    }

    void DrawShapeParameters()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        foreach (var prop in commonProperties)
        {
            EditorGUI.showMixedValue = prop.hasMultipleValues;
            
            object newValue = null;
            var label = prop.attribute.DisplayName;
            
            if (prop.field.FieldType == typeof(float))
            {
                float currentFloat = prop.hasMultipleValues ? 0 : (float)(prop.currentValue ?? 0f);
                if (prop.attribute.HasRange)
                    newValue = EditorGUILayout.Slider(label, currentFloat, prop.attribute.Min, prop.attribute.Max);
                else
                    newValue = EditorGUILayout.FloatField(label, currentFloat);
            }
            else if (prop.field.FieldType == typeof(Vector3))
            {
                Vector3 currentVec = prop.hasMultipleValues ? Vector3.zero : (Vector3)(prop.currentValue ?? Vector3.zero);
                newValue = EditorGUILayout.Vector3Field(label, currentVec);
            }
            else if (prop.field.FieldType == typeof(Vector2))
            {
                Vector2 currentVec = prop.hasMultipleValues ? Vector2.zero : (Vector2)(prop.currentValue ?? Vector2.zero);
                newValue = EditorGUILayout.Vector2Field(label, currentVec);
            }
            else if (prop.field.FieldType == typeof(Color))
            {
                Color currentColor = prop.hasMultipleValues ? Color.white : (Color)(prop.currentValue ?? Color.white);
                newValue = EditorGUILayout.ColorField(label, currentColor);
            }
            
            EditorGUI.showMixedValue = false;
            
            // Apply changes to all selected objects
            if (newValue != null && (prop.hasMultipleValues || !ValuesEqual(newValue, prop.currentValue)))
            {
                Undo.RecordObjects(targets, "Change Shape Parameter");
                foreach (Shape shape in targets)
                {
                    if (shape.shapeData != null)
                    {
                        var field = shape.shapeData.GetType().GetField(prop.field.Name);
                        if (field != null)
                        {
                            field.SetValue(shape.shapeData, newValue);
                            EditorUtility.SetDirty(shape);
                        }
                    }
                }
                prop.currentValue = newValue;
                prop.hasMultipleValues = false;
            }
        }
        
        EditorGUILayout.EndVertical();
    }

    void DrawAppearanceProperties()
    {
        bool showFill = false;
        bool showOutline = false;

        // Check render modes across all targets
        foreach (Shape shape in targets)
        {
            if (shape.fill)
                showFill = true;
            if (shape.outlineThickness > 0.000001f)
                showOutline = true;
        }

        EditorGUILayout.PropertyField(fillProp);
        if (showFill)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Fill Color", GUILayout.Width(EditorGUIUtility.labelWidth - 4));
            EditorGUILayout.PropertyField(fillColorProp, GUIContent.none);
            EditorGUILayout.EndHorizontal();
        }
        
        
        EditorGUILayout.PropertyField(outlineThicknessProp);
        if (showOutline)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Outline Color", GUILayout.Width(EditorGUIUtility.labelWidth - 4));
            EditorGUILayout.PropertyField(outlineColorProp, GUIContent.none);
            EditorGUILayout.EndHorizontal();
        }
        
    }

    void DrawLightingProperties()
    {
        EditorGUILayout.PropertyField(enableLightingProp);
        
        // Check if any selected object has lighting enabled
        bool anyLightingEnabled = false;
        foreach (Shape shape in targets)
        {
            if (shape.enableLighting)
            {
                anyLightingEnabled = true;
                break;
            }
        }
        
        if (anyLightingEnabled)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(smoothnessProp);
            EditorGUI.indentLevel--;
        }
    }

    void DrawTransformProperties()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.PropertyField(useTransformPositionProp, new GUIContent("Use Position"));
        EditorGUILayout.PropertyField(useTransformRotationProp, new GUIContent("Use Rotation"));
        EditorGUILayout.PropertyField(useTransformScaleProp, new GUIContent("Use Scale"));
        
        // Check if any selected object uses transform position
        bool anyUsePosition = false;
        foreach (Shape shape in targets)
        {
            if (shape.useTransformPosition)
            {
                anyUsePosition = true;
                break;
            }
        }
        
        if (anyUsePosition)
        {
            EditorGUILayout.PropertyField(localOffsetProp, new GUIContent("Local Offset"));
        }
        
        EditorGUILayout.EndVertical();
    }

    void DrawUtilityButtons()
    {
        if (GUILayout.Button("Ensure ShapeManager Exists", GUILayout.Height(25)))
        {
            if (Draw.Instance == null)
            {
                GameObject go = new GameObject("ShapeManager");
                go.AddComponent<Draw>();
                Debug.Log("ShapeManager added to scene");
            }
        }
        
        if (targets.Length > 1)
        {
            if (GUILayout.Button("Reset All to Defaults", GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog("Reset Shapes", 
                    "Are you sure you want to reset all selected shapes to defaults?", "Yes", "No"))
                {
                    Undo.RecordObjects(targets, "Reset Shapes");
                    foreach (Shape shape in targets)
                    {
                        shape.SetShapeType(Shape.ShapeType.Box);
                        shape.fill = true;
                        shape.fillColor = new Color(1f, 0.5f, 0.5f, 1f);
                        shape.outlineColor = Color.black;
                        shape.outlineThickness = 0.02f;
                        shape.enableLighting = true;
                        shape.smoothness = 0.5f;
                        EditorUtility.SetDirty(shape);
                    }
                    RefreshCommonProperties();
                }
            }
        }
    }
    #endregion

    #region Helper Methods
    void InitializeStyles()
    {
        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 12;
            headerStyle.normal.textColor = EditorGUIUtility.isProSkin
                ? new Color(0.9f, 0.9f, 0.9f)
                : new Color(0.1f, 0.1f, 0.1f);
        }
    }

    void DrawHeader(string text)
    {
        EditorGUILayout.LabelField(text, headerStyle);
    }

    bool ValuesEqual(object a, object b)
    {
        if (a == null || b == null) return a == b;
        if (a.GetType() != b.GetType()) return false;
        
        if (a is Vector3 v3a && b is Vector3 v3b)
            return v3a == v3b;
        if (a is Vector2 v2a && b is Vector2 v2b)
            return v2a == v2b;
        if (a is Color ca && b is Color cb)
            return ca == cb;
        if (a is float fa && b is float fb)
            return Mathf.Approximately(fa, fb);
        
        return a.Equals(b);
    }
    #endregion
}

#region Menu Items
public static class ShapeMenuItems
{
    private const string MENU_PATH = "GameObject/Shapes/";
    private const int MENU_PRIORITY = 10;
    
    [MenuItem(MENU_PATH + "Shape Manager", false, MENU_PRIORITY)]
    static void CreateShapeManager()
    {
        if (GameObject.FindObjectOfType<Draw>() != null)
        {
            Debug.LogWarning("ShapeManager already exists in scene");
            return;
        }
        
        GameObject manager = new GameObject("ShapeManager");
        manager.AddComponent<Draw>();
        
        Undo.RegisterCreatedObjectUndo(manager, "Create Shape Manager");
        Selection.activeGameObject = manager;
    }
    
    [MenuItem(MENU_PATH + "Box", false, MENU_PRIORITY + 10)]
    static void CreateBox()
    {
        CreateShapePreset("Box", Shape.ShapeType.Box);
    }
    
    [MenuItem(MENU_PATH + "Sphere", false, MENU_PRIORITY + 11)]
    static void CreateSphere()
    {
        CreateShapePreset("Sphere", Shape.ShapeType.Sphere);
    }
    
    [MenuItem(MENU_PATH + "Cylinder", false, MENU_PRIORITY + 12)]
    static void CreateCylinder()
    {
        CreateShapePreset("Cylinder", Shape.ShapeType.Cylinder);
    }
    
    [MenuItem(MENU_PATH + "Capsule", false, MENU_PRIORITY + 13)]
    static void CreateCapsule()
    {
        CreateShapePreset("Capsule", Shape.ShapeType.Capsule);
    }
    
    [MenuItem(MENU_PATH + "Cone", false, MENU_PRIORITY + 14)]
    static void CreateCone()
    {
        CreateShapePreset("Cone", Shape.ShapeType.Cone);
    }
    
    [MenuItem(MENU_PATH + "Rectangle", false, MENU_PRIORITY + 20)]
    static void CreateRectangle()
    {
        CreateShapePreset("Rectangle", Shape.ShapeType.Rectangle);
    }
    
    [MenuItem(MENU_PATH + "Disk", false, MENU_PRIORITY + 21)]
    static void CreateDisk()
    {
        CreateShapePreset("Disk", Shape.ShapeType.Disk);
    }
    
    [MenuItem(MENU_PATH + "Triangle", false, MENU_PRIORITY + 22)]
    static void CreateTriangle()
    {
        CreateShapePreset("Triangle", Shape.ShapeType.Triangle);
    }
    
    [MenuItem(MENU_PATH + "Line", false, MENU_PRIORITY + 23)]
    static void CreateLine()
    {
        CreateShapePreset("Line", Shape.ShapeType.Line2D);
    }
    
    private static void CreateShapePreset(string name, Shape.ShapeType type)
    {
        GameObject go = new GameObject(name);
        Shape shape = go.AddComponent<Shape>();
        shape.SetShapeType(type);
        
        // Position at scene view center or selection
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView != null)
        {
            go.transform.position = sceneView.camera.transform.position + sceneView.camera.transform.forward * 5f;
        }
        else if (Selection.activeTransform != null)
        {
            go.transform.position = Selection.activeTransform.position + Vector3.up;
        }
        
        // Ensure ShapeManager exists
        if (GameObject.FindObjectOfType<Draw>() == null)
        {
            GameObject manager = new GameObject("ShapeManager");
            manager.AddComponent<Draw>();
            Undo.RegisterCreatedObjectUndo(manager, "Create Shape Manager");
        }
        
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        Selection.activeGameObject = go;
    }
    
    [MenuItem(MENU_PATH + "Shape Manager", true)]
    static bool ValidateShapeManager()
    {
        return GameObject.FindObjectOfType<Draw>() == null;
    }
}
#endregion