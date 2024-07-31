
// this script adds a tool that zeros the rotation of a transform applying the rotation to the mesh filter on the object

using UnityEditor;
using UnityEngine;

public class ApplyRotationMesh : EditorWindow
{
    [MenuItem("Tools/Apply Mesh Rotation")]
    public static void ShowWindow()
    {
        foreach (var obj in Selection.gameObjects)
        {
            var meshFilter = obj.GetComponent<MeshFilter>();
            if (meshFilter)
            {
                Undo.RecordObject(obj, "Apply Rotation");
                meshFilter.sharedMesh = Instantiate(meshFilter.sharedMesh);
                meshFilter.sharedMesh = ApplyRotation(meshFilter.sharedMesh, obj.transform.rotation);
                obj.transform.rotation = Quaternion.identity;
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
}