using UnityEngine;
using Toolkit.Visualization.Internal;
using Toolkit.MeshGeneration;

namespace Toolkit.Visualization
{
	public static class Draw
	{
		private static MaterialPropertyBlock materialProperties;

		#region Core Drawing Functions with Matrix4x4

		public static void Sphere(Matrix4x4 transform, SphereSettings settings)
		{
			switch (settings.baseSettings.drawMode)
			{
				case DrawMode.Filled:
					DrawSphereFilled(transform, settings);
					break;
				case DrawMode.Outline:
				case DrawMode.PixelOutline:
					DrawSphereOutline(transform, settings);
					break;
			}
		}

		public static void Box(Matrix4x4 transform, BoxSettings settings)
		{
			switch (settings.baseSettings.drawMode)
			{
				case DrawMode.Filled:
					DrawBoxFilled(transform, settings);
					break;
				case DrawMode.Outline:
				case DrawMode.PixelOutline:
					DrawBoxOutline(transform, settings);
					break;
			}
		}

		public static void Cylinder(Matrix4x4 transform, CylinderSettings settings)
		{
			switch (settings.baseSettings.drawMode)
			{
				case DrawMode.Filled:
					DrawCylinderFilled(transform, settings);
					break;
				case DrawMode.Outline:
				case DrawMode.PixelOutline:
					DrawCylinderOutline(transform, settings);
					break;
			}
		}

		public static void Cone(Matrix4x4 transform, ConeSettings settings)
		{
			switch (settings.baseSettings.drawMode)
			{
				case DrawMode.Filled:
					DrawConeFilled(transform, settings);
					break;
				case DrawMode.Outline:
				case DrawMode.PixelOutline:
					DrawConeOutline(transform, settings);
					break;
			}
		}

		public static void Triangle(Matrix4x4 transform, TriangleSettings settings)
		{
			switch (settings.baseSettings.drawMode)
			{
				case DrawMode.Filled:
					DrawTriangleFilled(transform, settings);
					break;
				case DrawMode.Outline:
				case DrawMode.PixelOutline:
					DrawTriangleOutline(transform, settings);
					break;
			}
		}

		public static void Quad(Matrix4x4 transform, QuadSettings settings)
		{
			switch (settings.baseSettings.drawMode)
			{
				case DrawMode.Filled:
					DrawQuadFilled(transform, settings);
					break;
				case DrawMode.Outline:
				case DrawMode.PixelOutline:
					DrawQuadOutline(transform, settings);
					break;
			}
		}

		public static void Circle(Matrix4x4 transform, CircleSettings settings)
		{
			switch (settings.baseSettings.drawMode)
			{
				case DrawMode.Filled:
					DrawCircleFilled(transform, settings);
					break;
				case DrawMode.Outline:
				case DrawMode.PixelOutline:
					DrawCircleOutline(transform, settings);
					break;
			}
		}

		public static void Polygon(Matrix4x4 transform, PolygonSettings settings)
		{
			if (settings.vertices.Length < 3) return;

			switch (settings.baseSettings.drawMode)
			{
				case DrawMode.Filled:
					DrawPolygonFilled(transform, settings);
					break;
				case DrawMode.Outline:
				case DrawMode.PixelOutline:
					DrawPolygonOutline(transform, settings);
					break;
			}
		}

		#endregion

		#region Line Drawing

		public static void Line(Matrix4x4 transform, LineSettings settings)
		{
			if (settings.baseSettings.drawMode == DrawMode.Filled || settings.baseSettings.drawMode == DrawMode.Outline)
			{
				DrawLineThick(transform, settings);
			}
			else
			{
				DrawLinePixel(transform, settings);
			}
		}

		public static void Line(Vector3 start, Vector3 end, LineSettings settings)
		{
			Vector3 actualEnd = Vector3.Lerp(start, end, settings.baseSettings.t);
			if ((start - actualEnd).sqrMagnitude < float.Epsilon) return;

			Vector3 center = (start + actualEnd) * 0.5f;
			Vector3 direction = actualEnd - start;
			float length = direction.magnitude;
			Quaternion rotation = Quaternion.FromToRotation(Vector3.right, direction);

			Matrix4x4 transform = Matrix4x4.TRS(center, rotation, new Vector3(length, 1, 1));
			Line(transform, settings);
		}

		public static void Path(Vector3[] points, PathSettings settings)
		{
			if (points.Length < 2 || settings.baseSettings.t <= 0) return;

			float totalLength = 0;
			for (int i = 0; i < points.Length - 1; i++)
				totalLength += Vector3.Distance(points[i], points[i + 1]);
			if (settings.closed && points.Length > 2)
				totalLength += Vector3.Distance(points[points.Length - 1], points[0]);

			float drawLength = totalLength * settings.baseSettings.t;
			float lengthDrawn = 0;

			int limit = settings.closed ? points.Length : points.Length - 1;
			for (int i = 0; i < limit; i++)
			{
				int nextIndex = (i + 1) % points.Length;
				float segLength = Vector3.Distance(points[i], points[nextIndex]);

				if (lengthDrawn + segLength > drawLength)
				{
					segLength = drawLength - lengthDrawn;
					Vector3 endPoint = points[i] + (points[nextIndex] - points[i]).normalized * segLength;

					LineSettings _lineSettings = new LineSettings
					{
						baseSettings = settings.baseSettings,
						roundedCaps = settings.roundedJoints,
					};
					Line(points[i], endPoint, _lineSettings);
					break;
				}

				LineSettings lineSettings = new LineSettings
				{
					baseSettings = settings.baseSettings,
					roundedCaps = settings.roundedJoints,
				};
				Line(points[i], points[nextIndex], lineSettings);
				lengthDrawn += segLength;
			}
		}

		public static void Arrow(Vector3 start, Vector3 end, ArrowSettings settings)
		{
			Vector3 actualEnd = Vector3.Lerp(start, end, settings.baseSettings.t);
			if ((start - actualEnd).sqrMagnitude < float.Epsilon) return;

			Vector3 dir = (actualEnd - start).normalized;
			Vector3 up = Mathf.Abs(Vector3.Dot(dir, Vector3.up)) > 0.9f ? Vector3.right : Vector3.up;

			float v = Mathf.Cos(settings.headAngleDegrees * Mathf.Deg2Rad) * settings.headLength;
			Vector3 shaftEnd = actualEnd - dir * Mathf.Min((actualEnd - start).magnitude, v);
			CylinderSettings cylinderSettings = CylinderSettings.Default;
			cylinderSettings.baseSettings = settings.baseSettings;
			cylinderSettings.radius = settings.baseSettings.lineThickness * 0.5f;
			cylinderSettings.segments = 12;
			Cylinder(start, shaftEnd, cylinderSettings);

			// Draw filled cone head
			float coneRadius = Mathf.Tan(settings.headAngleDegrees * Mathf.Deg2Rad) * settings.headLength;
			Vector3 coneCenter = actualEnd - dir * settings.headLength * 0.5f;
			Quaternion rotation = Quaternion.FromToRotation(Vector3.up, dir);
			Matrix4x4 coneTransform = Matrix4x4.TRS(coneCenter, rotation, new Vector3(coneRadius, settings.headLength, coneRadius));

			ConeSettings coneSettings = ConeSettings.Default;
			coneSettings.baseSettings = settings.baseSettings;
			Cone(coneTransform, coneSettings);
		}

		#endregion

		#region Private Implementation Methods

		private static void DrawSphereFilled(Matrix4x4 transform, SphereSettings settings)
		{
			materialProperties = VisualizationRenderFeature.GetNewMaterialProperties();
			materialProperties.SetColor(DrawMaterials.colorID, settings.baseSettings.color);
			Material mat = settings.baseSettings.lit ? DrawMaterials.shadedMat : DrawMaterials.unlitMat;
			transform *= Matrix4x4.Scale(2 * settings.radius * Vector3.one);
			VisualizationRenderFeature.DrawMesh(SphereMeshGenerator.GetIdentityMesh(), transform, mat, materialProperties);
		}

		private static void DrawSphereOutline(Matrix4x4 transform, SphereSettings settings)
		{
			// Extract position and radius from transform
			Vector3 center = transform.GetColumn(3);
			float radius = transform.lossyScale.x * settings.radius;

			// Draw three circles for wireframe sphere
			int segments = settings.segments;
			Vector3[] pointsXY = new Vector3[segments];
			Vector3[] pointsXZ = new Vector3[segments];
			Vector3[] pointsYZ = new Vector3[segments];

			for (int i = 0; i < segments; i++)
			{
				float angle = i * Mathf.PI * 2 / segments;
				float cos = Mathf.Cos(angle);
				float sin = Mathf.Sin(angle);

				pointsXY[i] = center + new Vector3(cos * radius, sin * radius, 0);
				pointsXZ[i] = center + new Vector3(cos * radius, 0, sin * radius);
				pointsYZ[i] = center + new Vector3(0, cos * radius, sin * radius);
			}

			PathSettings pathSettings = new PathSettings
			{
				baseSettings = settings.baseSettings,
				closed = true,
			};

			Path(pointsXY, pathSettings);
			Path(pointsXZ, pathSettings);
			Path(pointsYZ, pathSettings);
		}

		private static void DrawBoxFilled(Matrix4x4 transform, BoxSettings settings)
		{
			materialProperties = VisualizationRenderFeature.GetNewMaterialProperties();
			materialProperties.SetColor(DrawMaterials.colorID, settings.baseSettings.color);
			VisualizationRenderFeature.DrawMesh(CubeMeshGenerator.GetIdentityMesh(), transform, DrawMaterials.unlitMat, materialProperties);
		}

		private static void DrawBoxOutline(Matrix4x4 transform, BoxSettings settings)
		{
			// Extract corners from transform
			Vector3 center = transform.GetColumn(3);
			Vector3 halfSize = transform.lossyScale * 0.5f;
			Quaternion rotation = transform.rotation;

			Vector3[] localCorners = new Vector3[]
			{
				new (-halfSize.x, -halfSize.y, -halfSize.z),
				new ( halfSize.x, -halfSize.y, -halfSize.z),
				new ( halfSize.x, -halfSize.y,  halfSize.z),
				new (-halfSize.x, -halfSize.y,  halfSize.z),
				new (-halfSize.x,  halfSize.y, -halfSize.z),
				new ( halfSize.x,  halfSize.y, -halfSize.z),
				new ( halfSize.x,  halfSize.y,  halfSize.z),
				new (-halfSize.x,  halfSize.y,  halfSize.z)
			};

			Vector3[] corners = new Vector3[8];
			for (int i = 0; i < 8; i++)
			{
				corners[i] = center + rotation * localCorners[i];
			}

			LineSettings lineSettings = new LineSettings
			{
				baseSettings = settings.baseSettings,
			};

			// Bottom face
			Line(corners[0], corners[1], lineSettings);
			Line(corners[1], corners[2], lineSettings);
			Line(corners[2], corners[3], lineSettings);
			Line(corners[3], corners[0], lineSettings);

			// Top face
			Line(corners[4], corners[5], lineSettings);
			Line(corners[5], corners[6], lineSettings);
			Line(corners[6], corners[7], lineSettings);
			Line(corners[7], corners[4], lineSettings);

			// Vertical edges
			Line(corners[0], corners[4], lineSettings);
			Line(corners[1], corners[5], lineSettings);
			Line(corners[2], corners[6], lineSettings);
			Line(corners[3], corners[7], lineSettings);
		}

		private static void DrawCylinderFilled(Matrix4x4 transform, CylinderSettings settings)
		{
			materialProperties = VisualizationRenderFeature.GetNewMaterialProperties();
			materialProperties.SetColor(DrawMaterials.colorID, settings.baseSettings.color);
			var mat = settings.baseSettings.lit ? DrawMaterials.shadedMat : DrawMaterials.unlitMat;
			VisualizationRenderFeature.DrawMesh(CylinderMeshGenerator.GetIdentityMesh(), transform, mat, materialProperties);
		}

		private static void DrawCylinderOutline(Matrix4x4 transform, CylinderSettings settings)
		{
			Vector3 center = transform.GetColumn(3);
			Vector3 up = transform.GetColumn(1).normalized;
			float radius = transform.lossyScale.x * settings.radius;
			float height = transform.lossyScale.y * settings.height;

			// Top and bottom circles
			Vector3 topCenter = center + up * (height * 0.5f);
			Vector3 bottomCenter = center - up * (height * 0.5f);

			Vector3[] topCircle = new Vector3[settings.segments];
			Vector3[] bottomCircle = new Vector3[settings.segments];

			for (int i = 0; i < settings.segments; i++)
			{
				float angle = i * Mathf.PI * 2 / settings.segments;
				Vector3 offset = (transform.GetColumn(0).normalized * Mathf.Cos(angle) +
								 transform.GetColumn(2).normalized * Mathf.Sin(angle)) * radius;
				topCircle[i] = topCenter + offset;
				bottomCircle[i] = bottomCenter + offset;
			}

			PathSettings pathSettings = new PathSettings
			{
				baseSettings = settings.baseSettings,
				closed = true,
			};

			Path(topCircle, pathSettings);
			Path(bottomCircle, pathSettings);

			// Vertical lines
			LineSettings lineSettings = new LineSettings
			{
				baseSettings = settings.baseSettings,
			};

			int step = Mathf.Max(1, settings.segments / 8);
			for (int i = 0; i < settings.segments; i += step)
			{
				Line(topCircle[i], bottomCircle[i], lineSettings);
			}
		}

		private static void DrawConeFilled(Matrix4x4 transform, ConeSettings settings)
		{
			materialProperties = VisualizationRenderFeature.GetNewMaterialProperties();
			materialProperties.SetColor(DrawMaterials.colorID, settings.baseSettings.color);
			var mat = settings.baseSettings.lit ? DrawMaterials.shadedMat : DrawMaterials.unlitMat;
			VisualizationRenderFeature.DrawMesh(ConeMeshGenerator.GetIdentityMesh(), transform, mat, materialProperties);
		}

		private static void DrawConeOutline(Matrix4x4 transform, ConeSettings settings)
		{
			Vector3 center = transform.GetColumn(3);
			Vector3 up = transform.GetColumn(1).normalized;
			float radius = transform.lossyScale.x * settings.radius;
			float height = transform.lossyScale.y * settings.height;

			Vector3 apex = center + up * (height * 0.5f);
			Vector3 baseCenter = center - up * (height * 0.5f);

			Vector3[] baseCircle = new Vector3[settings.segments];
			for (int i = 0; i < settings.segments; i++)
			{
				float angle = i * Mathf.PI * 2 / settings.segments;
				Vector3 offset = (transform.GetColumn(0).normalized * Mathf.Cos(angle) +
								 transform.GetColumn(2).normalized * Mathf.Sin(angle)) * radius;
				baseCircle[i] = baseCenter + offset;
			}

			PathSettings pathSettings = new PathSettings
			{
				baseSettings = settings.baseSettings,
				closed = true,
			};

			Path(baseCircle, pathSettings);

			// Lines from apex to base
			LineSettings lineSettings = new LineSettings
			{
				baseSettings = settings.baseSettings,
			};

			int step = Mathf.Max(1, settings.segments / 8);
			for (int i = 0; i < settings.segments; i += step)
			{
				Line(apex, baseCircle[i], lineSettings);
			}
		}

		private static void DrawTriangleFilled(Matrix4x4 transform, TriangleSettings settings)
		{
			if (settings.vertices.Length != 3) return;

			materialProperties = VisualizationRenderFeature.GetNewMaterialProperties();
			materialProperties.SetColor(DrawMaterials.colorID, settings.baseSettings.color);
			materialProperties.SetVector(DrawMaterials.trianglePointA, settings.vertices[0]);
			materialProperties.SetVector(DrawMaterials.trianglePointB, settings.vertices[1]);
			materialProperties.SetVector(DrawMaterials.trianglePointC, settings.vertices[2]);
			materialProperties.SetFloat(DrawMaterials.roundedEdgesID, settings.roundedCorners ? 1.0f : 0.0f);
			materialProperties.SetFloat(DrawMaterials.roundRadiusID, settings.cornerRadius);

			// No outline for filled mode
			materialProperties.SetColor(DrawMaterials.outlineColorID, Color.clear);
			materialProperties.SetFloat(DrawMaterials.outlineWidthID, 0.0f);
			materialProperties.SetFloat(DrawMaterials.usePixelOutlineID, 0.0f);

			VisualizationRenderFeature.DrawMesh(QuadMeshGenerator.GetQuadMesh(), transform, DrawMaterials.triangleMat, materialProperties);
		}

		private static void DrawTriangleOutline(Matrix4x4 transform, TriangleSettings settings)
		{
			if (settings.vertices.Length != 3) return;

			materialProperties = VisualizationRenderFeature.GetNewMaterialProperties();

			// Set triangle vertices
			materialProperties.SetVector(DrawMaterials.trianglePointA, settings.vertices[0]);
			materialProperties.SetVector(DrawMaterials.trianglePointB, settings.vertices[1]);
			materialProperties.SetVector(DrawMaterials.trianglePointC, settings.vertices[2]);
			materialProperties.SetFloat(DrawMaterials.roundedEdgesID, settings.roundedCorners ? 1.0f : 0.0f);
			materialProperties.SetFloat(DrawMaterials.roundRadiusID, settings.cornerRadius);

			// Set outline properties
			materialProperties.SetColor(DrawMaterials.outlineColorID, settings.baseSettings.color);
			materialProperties.SetFloat(DrawMaterials.usePixelOutlineID, settings.baseSettings.drawMode == DrawMode.PixelOutline ? 1.0f : 0.0f);
			materialProperties.SetFloat(DrawMaterials.outlineWidthID, settings.baseSettings.lineThickness);

			// Make fill transparent for outline-only mode
			materialProperties.SetColor(DrawMaterials.colorID, Color.clear);

			VisualizationRenderFeature.DrawMesh(QuadMeshGenerator.GetQuadMesh(), transform, DrawMaterials.triangleMat, materialProperties);
		}

		private static void DrawQuadFilled(Matrix4x4 transform, QuadSettings settings)
		{
			if (settings.vertices != null && settings.vertices.Length == 4)
			{
				materialProperties = VisualizationRenderFeature.GetNewMaterialProperties();
				materialProperties.SetColor(DrawMaterials.colorID, settings.baseSettings.color);
				materialProperties.SetVector(DrawMaterials.quadPointA, settings.vertices[0]);
				materialProperties.SetVector(DrawMaterials.quadPointB, settings.vertices[1]);
				materialProperties.SetVector(DrawMaterials.quadPointC, settings.vertices[2]);
				materialProperties.SetVector(DrawMaterials.quadPointD, settings.vertices[3]);
				VisualizationRenderFeature.DrawMesh(QuadMeshGenerator.GetQuadMesh(), transform, DrawMaterials.quadMat, materialProperties);
			}
			else
			{
				// Use as a simple quad/plane
				materialProperties = VisualizationRenderFeature.GetNewMaterialProperties();
				materialProperties.SetColor(DrawMaterials.colorID, settings.baseSettings.color);
				VisualizationRenderFeature.DrawMesh(QuadMeshGenerator.GetQuadMesh(), transform, DrawMaterials.unlitMat, materialProperties);
			}
		}

		private static void DrawQuadOutline(Matrix4x4 transform, QuadSettings settings)
		{
			Vector3[] worldVertices;

			if (settings.vertices != null && settings.vertices.Length == 4)
			{
				worldVertices = new Vector3[4];
				for (int i = 0; i < 4; i++)
				{
					worldVertices[i] = transform.MultiplyPoint3x4(settings.vertices[i]);
				}
			}
			else
			{
				// Default quad corners
				Vector3 center = transform.GetColumn(3);
				Vector3 halfSize = transform.lossyScale * 0.5f;
				Quaternion rotation = transform.rotation;

				worldVertices = new Vector3[]
				{
					center + rotation * new Vector3(-halfSize.x, -halfSize.y, 0),
					center + rotation * new Vector3( halfSize.x, -halfSize.y, 0),
					center + rotation * new Vector3( halfSize.x,  halfSize.y, 0),
					center + rotation * new Vector3(-halfSize.x,  halfSize.y, 0)
				};
			}

			PathSettings pathSettings = new PathSettings
			{
				baseSettings = settings.baseSettings,
				closed = true,
			};

			Path(worldVertices, pathSettings);
		}

		private static void DrawPolygonFilled(Matrix4x4 transform, PolygonSettings settings)
		{
			materialProperties = VisualizationRenderFeature.GetNewMaterialProperties();
			materialProperties.SetColor(DrawMaterials.colorID, settings.baseSettings.color);
			Mesh mesh = VisualizationRenderFeature.GetMesh();
			PolygonMeshGenerator.GeneratePolygonMesh(mesh, settings.vertices);
			VisualizationRenderFeature.DrawMesh(mesh, transform, DrawMaterials.unlitMat, materialProperties);
		}

		private static void DrawCircleFilled(Matrix4x4 transform, CircleSettings settings)
		{
			materialProperties = VisualizationRenderFeature.GetNewMaterialProperties();
			materialProperties.SetColor(DrawMaterials.colorID, settings.baseSettings.color);
			materialProperties.SetColor(DrawMaterials.outlineColorID, Color.clear);
			materialProperties.SetFloat(DrawMaterials.outlineWidthID, 0.0f);
			materialProperties.SetFloat(DrawMaterials.usePixelOutlineID, 0.0f);
			transform *= Matrix4x4.Scale(new Vector3(2 * settings.radius, 2 * settings.radius, 1));
			VisualizationRenderFeature.DrawMesh(QuadMeshGenerator.GetQuadMesh(), transform, DrawMaterials.circleMat, materialProperties);
		}

		private static void DrawCircleOutline(Matrix4x4 transform, CircleSettings settings)
		{
			materialProperties = VisualizationRenderFeature.GetNewMaterialProperties();
			materialProperties.SetColor(DrawMaterials.colorID, Color.clear);
			materialProperties.SetColor(DrawMaterials.outlineColorID, settings.baseSettings.color);
			materialProperties.SetFloat(DrawMaterials.outlineWidthID, settings.baseSettings.lineThickness);
			materialProperties.SetFloat(DrawMaterials.usePixelOutlineID, settings.baseSettings.drawMode == DrawMode.PixelOutline ? 1.0f : 0.0f);
			transform *= Matrix4x4.Scale(new Vector3(2 * settings.radius, 2 * settings.radius, 1));
			VisualizationRenderFeature.DrawMesh(QuadMeshGenerator.GetQuadMesh(), transform, DrawMaterials.circleMat, materialProperties);
		}

		private static void DrawPolygonOutline(Matrix4x4 transform, PolygonSettings settings)
		{
			Vector3[] worldVertices = new Vector3[settings.vertices.Length];
			for (int i = 0; i < settings.vertices.Length; i++)
			{
				worldVertices[i] = transform.MultiplyPoint3x4(settings.vertices[i]);
			}

			PathSettings pathSettings = new PathSettings
			{
				baseSettings = settings.baseSettings,
				closed = settings.closed,
			};

			Path(worldVertices, pathSettings);
		}

		private static void DrawLineThick(Matrix4x4 transform, LineSettings settings)
		{
			materialProperties = VisualizationRenderFeature.GetNewMaterialProperties();
			materialProperties.SetColor(DrawMaterials.colorID, settings.baseSettings.color);
			materialProperties.SetVector(DrawMaterials.sizeID, new Vector3(1, settings.baseSettings.lineThickness, 1));

			// transform to get correct thickness
			transform *= Matrix4x4.Scale(new Vector3(1, settings.baseSettings.lineThickness, 1));

			Material mat = settings.roundedCaps ? DrawMaterials.lineMatRoundedEdge : DrawMaterials.lineMat;
			VisualizationRenderFeature.DrawMesh(QuadMeshGenerator.GetQuadMesh(), transform, mat, materialProperties);
		}

		private static void DrawLinePixel(Matrix4x4 transform, LineSettings settings)
		{
			Vector3 start = transform.MultiplyPoint3x4(new Vector3(-0.5f, 0, 0));
			Vector3 end = transform.MultiplyPoint3x4(new Vector3(0.5f, 0, 0));
			end = Vector3.Lerp(start, end, settings.baseSettings.t);

			materialProperties = VisualizationRenderFeature.GetNewMaterialProperties();
			materialProperties.SetColor(DrawMaterials.colorID, settings.baseSettings.color);
			materialProperties.SetVector(DrawMaterials.quadPointA, start);
			materialProperties.SetVector(DrawMaterials.quadPointB, end);
			VisualizationRenderFeature.DrawMesh(LineMeshGenerator.GetIdentityMesh(), Matrix4x4.identity, DrawMaterials.linePixelMat, materialProperties);
		}

		#endregion

		#region Convenience Overloads

		public static void Sphere(Vector3 center, float radius, Color color, bool lit = false)
		{
			Matrix4x4 transform = Matrix4x4.TRS(center, Quaternion.identity, Vector3.one * radius);
			SphereSettings settings = SphereSettings.Default;
			settings.baseSettings.color = color;
			settings.baseSettings.lit = lit;
			Sphere(transform, settings);
		}

		public static void Box(Vector3 center, Quaternion rotation, Vector3 size, Color color)
		{
			Matrix4x4 transform = Matrix4x4.TRS(center, rotation, size);
			BoxSettings settings = BoxSettings.Default;
			settings.baseSettings.color = color;
			Box(transform, settings);
		}

		public static void Bounds(Bounds bounds, Color color, bool pixelLines = true)
		{
			Matrix4x4 transform = Matrix4x4.TRS(bounds.center, Quaternion.identity, bounds.size);
			BoxSettings settings = BoxSettings.Default;
			settings.baseSettings.color = color;
			settings.baseSettings.drawMode = pixelLines ? DrawMode.PixelOutline : DrawMode.Outline;
			Box(transform, settings);
		}

		public static void Cylinder(Vector3 center, Vector3 direction, float height, float radius, Color color)
		{
			Quaternion rotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);
			Matrix4x4 transform = Matrix4x4.TRS(center, rotation, new Vector3(radius, height, radius));
			CylinderSettings settings = CylinderSettings.Default;
			settings.baseSettings.color = color;
			Cylinder(transform, settings);
		}

		// cylinder from start to end
		public static void Cylinder(Vector3 start, Vector3 end, CylinderSettings settings)
		{
			Vector3 direction = end - start;
			float height = direction.magnitude;
			if (height < float.Epsilon) return;

			Quaternion rotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);
			Matrix4x4 transform = Matrix4x4.TRS((start + end) * 0.5f, rotation, new Vector3(settings.radius, height, settings.radius));
			Cylinder(transform, settings);
		}

		public static void Cone(Vector3 center, Vector3 direction, float height, float radius, Color color)
		{
			Quaternion rotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);
			Matrix4x4 transform = Matrix4x4.TRS(center, rotation, new Vector3(radius, height, radius));
			ConeSettings settings = ConeSettings.Default;
			settings.baseSettings.color = color;
			Cone(transform, settings);
		}

		public static void ArrowRay(Vector3 start, Vector3 direction, float length, ArrowSettings settings)
		{
			Arrow(start, start + direction.normalized * length, settings);
		}

		#endregion
	}
}