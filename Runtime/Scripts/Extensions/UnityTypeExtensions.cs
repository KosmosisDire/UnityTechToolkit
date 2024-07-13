using UnityEngine;
using UnityEngine.Rendering;

namespace Toolkit
{

public static class UnityTypeExtensions
{
    public static Color WithAlpha(this Color color, float alpha)
    {
        return new Color(color.r, color.g, color.b, alpha);
    }

    public static Mesh GetReadableMesh(this Mesh nonReadableMesh)
    {
        Mesh meshCopy = new()
        {
            indexFormat = nonReadableMesh.indexFormat
        };

        // Handle vertices
        GraphicsBuffer verticesBuffer = nonReadableMesh.GetVertexBuffer(0);
        int totalSize = verticesBuffer.stride * verticesBuffer.count;
        byte[] data = new byte[totalSize];
        verticesBuffer.GetData(data);
        meshCopy.SetVertexBufferParams(nonReadableMesh.vertexCount, nonReadableMesh.GetVertexAttributes());
        meshCopy.SetVertexBufferData(data, 0, 0, totalSize);
        verticesBuffer.Release();

        // Handle triangles
        meshCopy.subMeshCount = nonReadableMesh.subMeshCount;
        GraphicsBuffer indexesBuffer = nonReadableMesh.GetIndexBuffer();
        int tot = indexesBuffer.stride * indexesBuffer.count;
        byte[] indexesData = new byte[tot];
        indexesBuffer.GetData(indexesData);
        meshCopy.SetIndexBufferParams(indexesBuffer.count, nonReadableMesh.indexFormat);
        meshCopy.SetIndexBufferData(indexesData, 0, 0, tot);
        indexesBuffer.Release();

        // Restore submesh structure
        uint currentIndexOffset = 0;
        for (int i = 0; i < meshCopy.subMeshCount; i++)
        {
            uint subMeshIndexCount = nonReadableMesh.GetIndexCount(i);
            meshCopy.SetSubMesh(i, new SubMeshDescriptor((int)currentIndexOffset, (int)subMeshIndexCount));
            currentIndexOffset += subMeshIndexCount;
        }

        // Recalculate normals and bounds
        meshCopy.RecalculateNormals();
        meshCopy.RecalculateBounds();

        return meshCopy;
    }


    public static Bounds GetWorldBounds(this LineRenderer lineRenderer)
    {
        Bounds bounds = new Bounds(lineRenderer.transform.TransformPoint(lineRenderer.GetPosition(0)), Vector3.zero);
        for (int i = 1; i < lineRenderer.positionCount; i++)
        {
            bounds.Encapsulate(lineRenderer.transform.TransformPoint(lineRenderer.GetPosition(i)));
        }
        bounds.Expand(lineRenderer.widthMultiplier / 2f);
        return bounds;
    }

    public static float ScreenDistance(this LineRenderer lineRenderer, Vector2 screenPosition, Camera cam)
    {
        // get line between each point and check if distance to line is less than width
        var min = float.MaxValue;
        for (int i = 0; i < lineRenderer.positionCount - 1; i++)
        {
            Vector3 p1 = lineRenderer.transform.TransformPoint(lineRenderer.GetPosition(i));
            Vector3 p2 = lineRenderer.transform.TransformPoint(lineRenderer.GetPosition(i + 1));
            Debug.DrawLine(p1, p2, Color.green);
            var dist = Math3d.ScreenDistanceToLine(p1, p2, screenPosition, cam);
            if (dist < min)
            {
                min = dist;
            }
        }
        return min;
    }

    public static Bounds GetWorldBounds(this MeshFilter meshFilter)
    {
        if (meshFilter.sharedMesh == null)
        {
            var temp = new Bounds();
            temp.center = meshFilter.transform.position;
            temp.size = Vector3.one * 0.01f;
            return temp;
        }
        Bounds bounds = meshFilter.sharedMesh.bounds;
        bounds.center = meshFilter.transform.TransformPoint(bounds.center);
        bounds.size = meshFilter.transform.TransformVector(bounds.size);
        return bounds;
    }

    public static Bounds GetWorldBounds(this MeshRenderer meshRenderer)
    {
        return meshRenderer.GetComponent<MeshFilter>().GetWorldBounds();
    }

}

}