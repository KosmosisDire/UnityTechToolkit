using System;
using System.Linq;
using UnityEngine;

namespace Toolkit
{

public static class UnityObjectExtentions
{
    public static Transform FindRecursive(this Transform transform, string name)
    {
        return (from x in transform.gameObject.GetComponentsInChildren<Transform>()
                where x.gameObject.name == name
                select x).FirstOrDefault();
    }

    public static GameObject FindInParents<T>(this GameObject transform, Func<T, bool> predicate) where T : Component
    {
        var current = transform.transform;
        while (current != null)
        {
            var component = current.GetComponent<T>();
            if (component != null && predicate(component))
            {
                return current.gameObject;
            }
            current = current.parent;
        }
        return null;
    }

    public static T FindInParents<T>(this GameObject transform) where T : Component
    {
        var current = transform.transform;
        while (current != null)
        {
            var component = current.GetComponent<T>();
            if (component != null)
            {
                return component;
            }
            current = current.parent;
        }
        return null;
    }

    
    // find using a predicate
    public static T FindRecursive<T>(this GameObject transform, Func<T, bool> predicate) where T : Component
    {
        return (from x in transform.gameObject.GetComponentsInChildren<T>()
                where predicate(x)
                select x).FirstOrDefault();
    }

    public static T[] FindAllRecursive<T>(this GameObject transform, Func<T, bool> predicate) where T : Component
    {
        return (from x in transform.gameObject.GetComponentsInChildren<T>()
                where predicate(x)
                select x).ToArray();
    }

    public static Transform[] FindAllRecursive(this Transform transform, string name)
    {
        return (from x in transform.gameObject.GetComponentsInChildren<Transform>()
                where x.gameObject.name == name
                select x).ToArray();
    }

    public static void DestroyImmediateIfExists<T>(this GameObject gameObject) where T : Component
    {
        var component = gameObject.GetComponent<T>();
        if (component != null)
        {
            GameObject.DestroyImmediate(component);
        }
    }
    
}

}