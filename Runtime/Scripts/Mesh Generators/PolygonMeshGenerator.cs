using System.Collections;
using System.Collections.Generic;
using Toolkit.MeshGeneration.Internal;
using UnityEngine;

namespace Toolkit.MeshGeneration
{
	public static class PolygonMeshGenerator
	{

		public static void GeneratePolygonMesh(Mesh mesh, Vector2[] points)
		{
			mesh.Clear();
			(Vector2[] verts2D, int[] indices) = Triangulator.Triangulate(points);
			Vector3[] verts = To3DArray(verts2D, 0);

			mesh.SetVertices(verts);
			mesh.SetTriangles(indices, 0, true);
		}

		public static void GeneratePolygonMesh(Mesh mesh, Vector3[] points)
		{
			mesh.Clear();
			
			if (points.Length < 3) return;

			// Project 3D points to 2D for triangulation
			Vector3 normal = CalculateNormal(points);
			Vector3 tangent = (points[1] - points[0]).normalized;
			Vector3 bitangent = Vector3.Cross(normal, tangent).normalized;

			// Project all points to the polygon's 2D plane
			Vector2[] points2D = new Vector2[points.Length];
			for (int i = 0; i < points.Length; i++)
			{
				Vector3 localPoint = points[i] - points[0];
				points2D[i] = new Vector2(Vector3.Dot(localPoint, tangent), Vector3.Dot(localPoint, bitangent));
			}

			// Triangulate in 2D
			(Vector2[] verts2D, int[] indices) = Triangulator.Triangulate(points2D);

			// Convert back to 3D using the original points as vertices
			Vector3[] verts3D = new Vector3[verts2D.Length];
			for (int i = 0; i < verts2D.Length; i++)
			{
				// Find closest original point or interpolate
				if (i < points.Length)
				{
					verts3D[i] = points[i];
				}
				else
				{
					// For additional vertices created by triangulation, reconstruct in 3D space
					verts3D[i] = points[0] + tangent * verts2D[i].x + bitangent * verts2D[i].y;
				}
			}

			mesh.SetVertices(verts3D);
			mesh.SetTriangles(indices, 0, true);
			mesh.RecalculateNormals();
		}

		static Vector3 CalculateNormal(Vector3[] points)
		{
			if (points.Length < 3) return Vector3.up;
			
			// Use Newell's method for robust normal calculation
			Vector3 normal = Vector3.zero;
			for (int i = 0; i < points.Length; i++)
			{
				Vector3 current = points[i];
				Vector3 next = points[(i + 1) % points.Length];
				
				normal.x += (current.y - next.y) * (current.z + next.z);
				normal.y += (current.z - next.z) * (current.x + next.x);
				normal.z += (current.x - next.x) * (current.y + next.y);
			}
			
			return normal.normalized;
		}

		static Vector3[] To3DArray(Vector2[] array2D, float z = 0)
		{
			Vector3[] array3D = new Vector3[array2D.Length];

			for (int i = 0; i < array3D.Length; i++)
			{
				array3D[i] = new Vector3(array2D[i].x, array2D[i].y, z);
			}

			return array3D;
		}


	}
}