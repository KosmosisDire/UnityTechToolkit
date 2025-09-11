using System;
using System.Reflection;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

// The attribute to mark methods with
[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public sealed class ButtonAttribute : Attribute
{
    public string Name { get; }
    public ButtonAttribute(string name = null)
    {
        Name = name;
    }
}

#if UNITY_EDITOR
// Custom editor that draws buttons for all MonoBehaviours
[CustomEditor(typeof(MonoBehaviour), true)]
[CanEditMultipleObjects]
public class ButtonAttributeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();
        
        // Get all methods with the Button attribute
        var methods = target.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(m => m.GetCustomAttributes(typeof(ButtonAttribute), true).Length > 0)
            .ToArray();
        
        if (methods.Length > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Debug Methods", EditorStyles.boldLabel);
            
            foreach (var method in methods)
            {
                var buttonAttribute = (ButtonAttribute)method.GetCustomAttribute(typeof(ButtonAttribute));
                string buttonName = string.IsNullOrEmpty(buttonAttribute.Name) ? method.Name : buttonAttribute.Name;
                
                // Draw button
                GUI.enabled = Application.isPlaying || !RequiresPlayMode(method);
                
                if (GUILayout.Button(buttonName))
                {
                    // Handle multiple targets
                    foreach (var t in targets)
                    {
                        method.Invoke(t, null);
                    }
                }
                
                GUI.enabled = true;
            }
        }
    }
    
    // Optional: Check if method has an attribute indicating it needs play mode
    private bool RequiresPlayMode(MethodInfo method)
    {
        // You could extend this to check for a [RequiresPlayMode] attribute if needed
        return false;
    }
}
#endif