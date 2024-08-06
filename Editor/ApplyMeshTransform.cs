
// this script adds a tool that zeros the rotation of a transform applying the rotation to the mesh filter on the object

using UnityEditor;
using UnityEngine;

public class ApplyMeshTransform : EditorWindow
{
    [MenuItem("Tools/Apply Mesh Rotation")]
    public static void ShowWindow()
    {
        foreach (var obj in Selection.gameObjects)
        {
            var meshFilters = obj.GetComponentsInChildren<MeshFilter>();
            foreach (var meshFilter in meshFilters)
            {
                Undo.RecordObject(obj, "Apply Rotation");

                
                var oldMesh = meshFilter.sharedMesh;

                if (!oldMesh.name.EndsWith("(Clone)"))
                {
                    meshFilter.sharedMesh = Instantiate(oldMesh);
                }
                meshFilter.sharedMesh = ApplyRotation(meshFilter.sharedMesh, obj.transform.rotation);
                obj.transform.rotation = Quaternion.identity;

                var meshCollider = meshFilter.GetComponent<MeshCollider>();
                if (meshCollider != null && meshCollider.sharedMesh == oldMesh)
                {
                    meshCollider.sharedMesh = meshFilter.sharedMesh;
                }

                var boxCollider = meshFilter.GetComponent<BoxCollider>();
                if (boxCollider != null)
                {
                    boxCollider.size = meshFilter.sharedMesh.bounds.size;
                    boxCollider.center = meshFilter.sharedMesh.bounds.center;
                }
            }
        }
    }

    private static Mesh ApplyRotation(Mesh mesh, Quaternion rotation)
    {
        var vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = rotation * vertices[i];
        }
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    [MenuItem("Tools/Apply Mesh Scale")]
    public static void ApplyScale()
    {
        foreach (var obj in Selection.gameObjects)
        {
            var meshFilters = obj.GetComponentsInChildren<MeshFilter>();
            foreach (var meshFilter in meshFilters)
            {
                Undo.RecordObject(obj, "Apply Scale");

                var oldMesh = meshFilter.sharedMesh;

                if (!oldMesh.name.EndsWith("(Clone)"))
                {
                    meshFilter.sharedMesh = Instantiate(oldMesh);
                }
                meshFilter.sharedMesh = ApplyScale(meshFilter.sharedMesh, obj.transform.localScale);
                obj.transform.localScale = Vector3.one;

                var meshCollider = meshFilter.GetComponent<MeshCollider>();
                if (meshCollider != null && meshCollider.sharedMesh == oldMesh)
                {
                    meshCollider.sharedMesh = meshFilter.sharedMesh;
                }

                var boxCollider = meshFilter.GetComponent<BoxCollider>();
                if (boxCollider != null)
                {
                    boxCollider.size = meshFilter.sharedMesh.bounds.size;
                    boxCollider.center = meshFilter.sharedMesh.bounds.center;
                }
            }
        }
    }

    private static Mesh ApplyScale(Mesh mesh, Vector3 scale)
    {
        var vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(vertices[i].x * scale.x, vertices[i].y * scale.y, vertices[i].z * scale.z);
        }
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}