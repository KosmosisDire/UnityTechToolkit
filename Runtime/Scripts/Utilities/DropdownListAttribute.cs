using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

// Dropdown attribute for fields
[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public sealed class DropdownListAttribute : PropertyAttribute
{
    public string OptionsGetter { get; }
    public string OnChangeCallback { get; }
    public string Label { get; }
    
    public DropdownListAttribute(string optionsGetter, string onChangeCallback = null, string label = null)
    {
        OptionsGetter = optionsGetter;
        OnChangeCallback = onChangeCallback;
        Label = label;
    }
}

#if UNITY_EDITOR
// Custom property drawer for dropdown fields
[CustomPropertyDrawer(typeof(DropdownListAttribute))]
public class DropdownListPropertyDrawer : PropertyDrawer
{
    private Dictionary<string, object> cachedOptions = null;
    private string[] cachedDisplayNames = null;
    private object[] cachedValues = null;
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var dropdownAttribute = (DropdownListAttribute)attribute;
        var target = property.serializedObject.targetObject;
        
        // Get the options from the specified method
        var optionsMethod = target.GetType().GetMethod(dropdownAttribute.OptionsGetter, 
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        
        if (optionsMethod == null)
        {
            EditorGUI.LabelField(position, label.text, $"Method '{dropdownAttribute.OptionsGetter}' not found");
            return;
        }
        
        // Get options dictionary
        var options = optionsMethod.Invoke(target, null);
        
        if (options == null)
        {
            EditorGUI.LabelField(position, label.text, "Options method returned null");
            return;
        }
        
        // Handle different return types
        Dictionary<string, object> optionsDict = null;
        
        if (options is Dictionary<string, object> dict)
        {
            optionsDict = dict;
        }
        else if (options is Dictionary<string, string> stringDict)
        {
            optionsDict = stringDict.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
        }
        else if (options is Dictionary<string, int> intDict)
        {
            optionsDict = intDict.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
        }
        else if (options is string[] stringArray)
        {
            optionsDict = stringArray.ToDictionary(s => s, s => (object)s);
        }
        else
        {
            EditorGUI.LabelField(position, label.text, "Invalid return type for options");
            return;
        }
        
        // Cache the options for performance
        if (cachedOptions == null || !optionsDict.SequenceEqual(cachedOptions))
        {
            cachedOptions = optionsDict;
            cachedDisplayNames = optionsDict.Keys.ToArray();
            cachedValues = optionsDict.Values.ToArray();
        }
        
        // Get current value
        object currentValue = GetPropertyValue(property);
        int selectedIndex = Array.IndexOf(cachedValues, currentValue);
        if (selectedIndex < 0) selectedIndex = 0;
        
        // Draw dropdown
        string displayLabel = string.IsNullOrEmpty(dropdownAttribute.Label) ? label.text : dropdownAttribute.Label;
        EditorGUI.BeginChangeCheck();
        int newIndex = EditorGUI.Popup(position, displayLabel, selectedIndex, cachedDisplayNames);
        
        if (EditorGUI.EndChangeCheck() && newIndex >= 0 && newIndex < cachedValues.Length)
        {
            // Set the new value
            SetPropertyValue(property, cachedValues[newIndex]);
            property.serializedObject.ApplyModifiedProperties();
            
            // Call onChange callback if specified
            if (!string.IsNullOrEmpty(dropdownAttribute.OnChangeCallback))
            {
                var onChangeMethod = target.GetType().GetMethod(dropdownAttribute.OnChangeCallback,
                    BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                
                if (onChangeMethod != null)
                {
                    var parameters = onChangeMethod.GetParameters();
                    if (parameters.Length == 0)
                    {
                        onChangeMethod.Invoke(target, null);
                    }
                    else if (parameters.Length == 1)
                    {
                        onChangeMethod.Invoke(target, new[] { cachedValues[newIndex] });
                    }
                    else if (parameters.Length == 2)
                    {
                        onChangeMethod.Invoke(target, new[] { cachedDisplayNames[newIndex], cachedValues[newIndex] });
                    }
                }
            }
        }
    }
    
    private object GetPropertyValue(SerializedProperty property)
    {
        switch (property.propertyType)
        {
            case SerializedPropertyType.String:
                return property.stringValue;
            case SerializedPropertyType.Integer:
                return property.intValue;
            case SerializedPropertyType.Float:
                return property.floatValue;
            case SerializedPropertyType.Boolean:
                return property.boolValue;
            case SerializedPropertyType.Enum:
                return property.enumValueIndex;
            default:
                return null;
        }
    }
    
    private void SetPropertyValue(SerializedProperty property, object value)
    {
        switch (property.propertyType)
        {
            case SerializedPropertyType.String:
                property.stringValue = value?.ToString() ?? "";
                break;
            case SerializedPropertyType.Integer:
                property.intValue = Convert.ToInt32(value);
                break;
            case SerializedPropertyType.Float:
                property.floatValue = Convert.ToSingle(value);
                break;
            case SerializedPropertyType.Boolean:
                property.boolValue = Convert.ToBoolean(value);
                break;
            case SerializedPropertyType.Enum:
                property.enumValueIndex = Convert.ToInt32(value);
                break;
        }
    }
}

#endif