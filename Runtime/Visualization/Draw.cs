using UnityEngine;
using Toolkit.Visualization.Internal;
using Toolkit.MeshGeneration;

namespace Toolkit.Visualization
{

    // TODO: most draw functions don't support instancing. Should probably fix that at some point...
    public static class Draw
	{
		private static MaterialPropertyBlock materialProperties;

		// NOTE: My polygon mesh generator is a bit buggy. Should be replaced with better implementation.
		public static void Polygon(Vector2[] points, Color col)
		{
			if (points.Length <= 2) { return; }

			materialProperties = VisualizationRenderFeature.GetNewMaterialProperties();
			materialProperties.SetColor(DrawMaterials.colorID, col);
			Mesh mesh = VisualizationRenderFeature.GetMesh();
			PolygonMeshGenerator.GeneratePolygonMesh(mesh, points);
			VisualizationRenderFeature.DrawMesh(mesh, Matrix4x4.identity, DrawMaterials.unlitMat, materialProperties);
		}


		public static void Quad(Vector2 center, Vector2 size, Color col)
		{
			if (size.x == 0 && size.y == 0) { return; }
			
			materialProperties = VisualizationRenderFeature.GetNewMaterialProperties();
			materialProperties.SetColor(DrawMaterials.colorID, col);
			Matrix4x4 matrix = Matrix4x4.TRS(center, Quaternion.identity, new Vector3(size.x, size.y, 1));
            VisualizationRenderFeature.DrawMesh(QuadMeshGenerator.GetQuadMesh(), matrix, DrawMaterials.unlitMat, materialProperties);
		}

		public static void Quad(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color col)
		{
			
			Mesh mesh = QuadMeshGenerator.GetQuadMesh();
			materialProperties = VisualizationRenderFeature.GetNewMaterialProperties();
			materialProperties.SetColor(DrawMaterials.colorID, col);
			materialProperties.SetVector(DrawMaterials.quadPointA, a);
			materialProperties.SetVector(DrawMaterials.quadPointB, b);
			materialProperties.SetVector(DrawMaterials.quadPointC, c);
			materialProperties.SetVector(DrawMaterials.quadPointD, d);
            VisualizationRenderFeature.DrawMesh(mesh, Matrix4x4.identity, DrawMaterials.quadMat, materialProperties);
		}

		public static void Mesh(Mesh mesh, Color col)
		{
			Mesh(mesh, Vector3.zero, Quaternion.identity, Vector3.one, col);
		}

		public static void Mesh(Mesh mesh, Vector3 pos, Quaternion rot, Vector3 scale, Color col)
		{
			
			materialProperties = VisualizationRenderFeature.GetNewMaterialProperties();
			materialProperties.SetColor(DrawMaterials.colorID, col);
			Matrix4x4 matrix = Matrix4x4.TRS(pos, rot, scale);
            VisualizationRenderFeature.DrawMesh(mesh, matrix, DrawMaterials.unlitMat, materialProperties);
		}
		
	    public static void Mesh(Mesh mesh, Vector3 pos, Quaternion rot, Vector3 scale, Material mat)
		{
			materialProperties = VisualizationRenderFeature.GetNewMaterialProperties();
			Matrix4x4 matrix = Matrix4x4.TRS(pos, rot, scale);
            VisualizationRenderFeature.DrawMesh(mesh, matrix, mat, materialProperties);
		}

		public static void Arrow(Vector3 start, Vector3 end, ArrowSettings settings, float t = 1)
		{
			end = Vector3.Lerp(start, end, t); // animate end point
			if (settings.thickness <= 0 || (start - end).sqrMagnitude <= float.Epsilon) { return; }
			

			Vector3 dir = (end - start).normalized;
			Vector3 perp = new Vector2(-dir.y, dir.x);
			float angle = Mathf.Atan2(dir.y, dir.x);


			float headAngleA = angle + Mathf.PI + settings.headAngleDegrees * Mathf.Deg2Rad * (settings.fillHead ? 1 : t);
			float headAngleB = angle + Mathf.PI - settings.headAngleDegrees * Mathf.Deg2Rad * (settings.fillHead ? 1 : t);
			Vector3 headDirA = new Vector2(Mathf.Cos(headAngleA), Mathf.Sin(headAngleA));
			Vector3 headDirB = new Vector2(Mathf.Cos(headAngleB), Mathf.Sin(headAngleB));

			float maxHeadLength = VisMath.RayLineIntersectionDistance(end, headDirA, start - perp, start + perp);
			Vector3 headEndA = end + headDirA * Mathf.Min(maxHeadLength, settings.headLength);
			Vector3 headEndB = end + headDirB * Mathf.Min(maxHeadLength, settings.headLength);

			if (settings.fillHead)
			{
				float v = Mathf.Cos(settings.headAngleDegrees * Mathf.Deg2Rad) * settings.headLength;
				Line(start, end - dir * Mathf.Min((end - start).magnitude, v), settings.thickness, settings.col, false);
				Polygon(new Vector2[] { end, headEndA, headEndB }, settings.col);
			}
			else
			{
				Line(start, end, settings.thickness, settings.col, true);
				Line(end, headEndA, settings.thickness, settings.col, true, t);
				Line(end, headEndB, settings.thickness, settings.col, true, t);
			}
		}

		public static void Line(Vector3 start, Vector3 end, float thickness, Color col, float t = 1)
		{
			Line(start, end, thickness, col, false, t);
		}

		public static void Line(Vector3 start, Vector3 end, float thickness, Color col, bool roundedEdges, float t = 1)
		{
			if (roundedEdges == false)
			{
				DrawLineSharpEdges(start, end, thickness, col, t);
				return;
			}
			end = Vector3.Lerp(start, end, t); // animate end point
			if (thickness <= 0 || (start - end).sqrMagnitude == 0) { return; }

			
			float length = (start - end).magnitude;
			// Squish the rounding effect to 0 as line length goes from thickness -> 0
			float thicknessScaleT = Mathf.Min(1, length / thickness);
			Vector3 scale = new Vector3(length + thickness * 1, thickness, 1);

			materialProperties = VisualizationRenderFeature.GetNewMaterialProperties();
			materialProperties.SetColor(DrawMaterials.colorID, col);
			materialProperties.SetVector(DrawMaterials.sizeID, new Vector3(length + thickness, thickness, 1));

			Vector3 center = (start + end) / 2;
			Quaternion rot = Quaternion.FromToRotation(Vector3.left, start - end);

			Matrix4x4 matrix = Matrix4x4.TRS(center, rot, scale);
            VisualizationRenderFeature.DrawMesh(QuadMeshGenerator.GetQuadMesh(), matrix, DrawMaterials.lineMatRoundedEdge, materialProperties);
        }

		public static void LinePixel(Vector3 start, Vector3 end, Color col, float t = 1)
		{
			materialProperties = VisualizationRenderFeature.GetNewMaterialProperties();
			materialProperties.SetColor(DrawMaterials.colorID, col);
			materialProperties.SetVector(DrawMaterials.quadPointA, start);
			materialProperties.SetVector(DrawMaterials.quadPointB, Vector3.Lerp(start, end, t));
			VisualizationRenderFeature.DrawMesh(LineMeshGenerator.GetIdentityMesh(), Matrix4x4.identity, DrawMaterials.linePixelMat, materialProperties);
		}

		static void DrawLineSharpEdges(Vector3 start, Vector3 end, float thickness, Color col, float t = 1)
		{
			end = Vector3.Lerp(start, end, t); // animate end point
			if (thickness <= 0 || (start - end).sqrMagnitude == 0) { return; }

			
			float length = (start - end).magnitude;
			Vector3 scale = new Vector3(length, thickness, 1);

			materialProperties = VisualizationRenderFeature.GetNewMaterialProperties();
			materialProperties.SetColor(DrawMaterials.colorID, col);
			materialProperties.SetVector(DrawMaterials.sizeID, new Vector3(length, thickness, 1));

			Vector3 center = (start + end) / 2;
			Quaternion rot = Quaternion.FromToRotation(Vector3.left, start - end);

			Matrix4x4 matrix = Matrix4x4.TRS(center, rot, scale);
            VisualizationRenderFeature.DrawMesh(QuadMeshGenerator.GetQuadMesh(), matrix, DrawMaterials.lineMat, materialProperties);
		}


		public static void Ray(Vector3 start, Vector3 offset, float thickness, Color col)
		{
			Line(start, start + offset, thickness, col);
		}


        public static void RayDirection(Vector3 start, Vector3 direction, float length, float thickness, Color col)
		{
			Line(start, start + direction.normalized * length, thickness, col);
		}

		public static void Path(Vector2[] points, float thickness, bool closed, Color col)
		{
			Vector3[] points3D = new Vector3[points.Length];
			for (int i = 0; i < points.Length; i++)
			{
				points3D[i] = new Vector3(points[i].x, points[i].y, 0);
			}
			Path(points3D, thickness, closed, col);
		}

		public static void Path(Vector3[] points, float thickness, bool closed, Color col, float t = 1)
		{
			if (t == 0) { return; }

			float totalLength = 0;
			for (int i = 0; i < points.Length - 1; i++)
			{
				totalLength += Vector3.Distance(points[i], points[i + 1]);
			}
			if (closed)
			{
				totalLength += (Vector3.Distance(points[0], points[^1]));
			}

			float drawLength = totalLength * t;
			float lengthDrawn = 0;

			int lim = closed ? points.Length : points.Length - 1;
			for (int i = 0; i < lim; i++)
			{
				bool exit = false;
				int nextIndex = (i + 1) % points.Length;
				float segLength = Vector3.Distance(points[i], points[nextIndex]);
				if (lengthDrawn + segLength > drawLength)
				{
					segLength = drawLength - lengthDrawn;
					exit = true;
				}
				Vector3 a = points[i];
				Vector3 b = points[nextIndex];
				b = a + (b - a).normalized * segLength;
				Draw.Line(a, b, thickness, col, true);
				lengthDrawn += segLength;
				if (exit)
				{
					break;
				}
			}
		}

		public static void PathPixel(Vector3[] points, Color col)
		{
			for (int i = 0; i < points.Length - 1; i++)
			{
				LinePixel(points[i], points[i + 1], col);
			}
		}

		public static void BoxOutline(Vector2 center, Vector2 size, float thickness, Color col, float t = 1)
		{
			Vector3[] path =
			{
				center + new Vector2(-size.x,size.y) * 0.5f,
				center + new Vector2(size.x,size.y) * 0.5f,
				center + new Vector2(size.x,-size.y) * 0.5f,
				center + new Vector2(-size.x,-size.y) * 0.5f
			};
			Draw.Path(path, thickness, true, col, t);
		}

		public static void Bounds(Bounds bounds, Color col)
		{
			Vector3[] pathTop = 
			{
				new Vector3(bounds.min.x, bounds.min.y, bounds.max.z),
				new Vector3(bounds.max.x, bounds.min.y, bounds.max.z),
				new Vector3(bounds.max.x, bounds.min.y, bounds.min.z),
				new Vector3(bounds.min.x, bounds.min.y, bounds.min.z),
				new Vector3(bounds.min.x, bounds.min.y, bounds.max.z)
			};
			Vector3[] pathBottom = 
			{
				new Vector3(bounds.min.x, bounds.max.y, bounds.max.z),
				new Vector3(bounds.max.x, bounds.max.y, bounds.max.z),
				new Vector3(bounds.max.x, bounds.max.y, bounds.min.z),
				new Vector3(bounds.min.x, bounds.max.y, bounds.min.z),
				new Vector3(bounds.min.x, bounds.max.y, bounds.max.z)
			};

			Draw.PathPixel(pathTop, col);
			Draw.PathPixel(pathBottom, col);

			Draw.LinePixel(pathTop[0], pathBottom[0], col);
			Draw.LinePixel(pathTop[1], pathBottom[1], col);
			Draw.LinePixel(pathTop[2], pathBottom[2], col);
			Draw.LinePixel(pathTop[3], pathBottom[3], col);
		}

		// Draw a 2D point
		public static void Point(Vector3 center, float radius, Color col)
		{
			// Skip if radius or alpha is zero
			if (radius == 0 || col.a == 0) { return; }

			// Initialize frame (ensures draw commands from prev frame are cleared, etc.)
			

			// Create matrix to control position/rotation/scale of mesh
			Vector3 scale = new Vector3(radius * 2, radius * 2, 1);
			Matrix4x4 matrix = Matrix4x4.TRS(center, Quaternion.identity, scale);

			// Draw quad mesh with point shader
			Mesh quadMesh = QuadMeshGenerator.GetQuadMesh();
			materialProperties = VisualizationRenderFeature.GetNewMaterialProperties();
			materialProperties.SetColor(DrawMaterials.colorID, col);
            VisualizationRenderFeature.DrawMesh(quadMesh, matrix, DrawMaterials.pointMat, materialProperties);
		}

		public static void Sphere(Vector3 center, float radius, Color col, bool unlit = false)
		{
			materialProperties = VisualizationRenderFeature.GetNewMaterialProperties();
			materialProperties.SetColor(DrawMaterials.colorID, col);
			Matrix4x4 matrix = Matrix4x4.TRS(center, Quaternion.identity, Vector3.one * radius);
            VisualizationRenderFeature.DrawMesh(SphereMeshGenerator.GetMesh(), matrix, unlit ? DrawMaterials.unlitMat : DrawMaterials.shadedMat, materialProperties);
        }

		public static void Cube(Vector3 center, Quaternion rotation, Vector3 scale, Color col)
		{
			materialProperties = VisualizationRenderFeature.GetNewMaterialProperties();
			materialProperties.SetColor(DrawMaterials.colorID, col);
			Matrix4x4 matrix = Matrix4x4.TRS(center, rotation, scale);
            VisualizationRenderFeature.DrawMesh(CubeMeshGenerator.GetIdentityMesh(), matrix, DrawMaterials.unlitMat, materialProperties);
		}

	}
}