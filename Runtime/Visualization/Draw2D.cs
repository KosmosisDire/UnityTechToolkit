using UnityEngine;

namespace Toolkit.Visualization
{
    public static class Draw2D
    {
        #region Shape Drawing

        public static void Circle(Vector2 center, float radius, Color color, DrawMode drawMode = DrawMode.Filled, float lineThickness = 0.02f, int segments = 32)
        {
            Vector3 center3D = new Vector3(center.x, center.y, 0);
            Matrix4x4 transform = Matrix4x4.TRS(center3D, Quaternion.identity, Vector3.one * radius);
            
            SphereSettings settings = SphereSettings.Default;
            settings.baseSettings.color = color;
            settings.baseSettings.drawMode = drawMode;
            settings.baseSettings.lineThickness = lineThickness;
            settings.baseSettings.lit = false;
            settings.segments = segments;
            
            Draw.Sphere(transform, settings);
        }

        public static void Rectangle(Vector2 center, Vector2 size, Color color, DrawMode drawMode = DrawMode.Filled, float lineThickness = 0.02f)
        {
            Vector3 center3D = new Vector3(center.x, center.y, 0);
            Vector3 size3D = new Vector3(size.x, size.y, 1);
            Matrix4x4 transform = Matrix4x4.TRS(center3D, Quaternion.identity, size3D);
            
            BoxSettings settings = BoxSettings.Default;
            settings.baseSettings.color = color;
            settings.baseSettings.drawMode = drawMode;
            settings.baseSettings.lineThickness = lineThickness;
            
            Draw.Box(transform, settings);
        }

        public static void Triangle(Vector2 a, Vector2 b, Vector2 c, Color color, DrawMode drawMode = DrawMode.Filled, float lineThickness = 0.02f, bool roundedCorners = false, float cornerRadius = 0.1f)
        {
            TriangleSettings settings = TriangleSettings.Default;
            settings.vertices = new Vector3[] {
                new Vector3(a.x, a.y, 0),
                new Vector3(b.x, b.y, 0),
                new Vector3(c.x, c.y, 0)
            };
            settings.baseSettings.color = color;
            settings.baseSettings.drawMode = drawMode;
            settings.baseSettings.lineThickness = lineThickness;
            settings.roundedCorners = roundedCorners;
            settings.cornerRadius = cornerRadius;
            
            Draw.Triangle(Matrix4x4.identity, settings);
        }

        public static void Quad(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color color, DrawMode drawMode = DrawMode.Filled, float lineThickness = 0.02f)
        {
            QuadSettings settings = QuadSettings.Default;
            settings.vertices = new Vector3[] {
                new Vector3(a.x, a.y, 0),
                new Vector3(b.x, b.y, 0),
                new Vector3(c.x, c.y, 0),
                new Vector3(d.x, d.y, 0)
            };
            settings.baseSettings.color = color;
            settings.baseSettings.drawMode = drawMode;
            settings.baseSettings.lineThickness = lineThickness;
            
            Draw.Quad(Matrix4x4.identity, settings);
        }

        public static void Polygon(Vector2[] points, Color color, DrawMode drawMode = DrawMode.Filled, float lineThickness = 0.02f, bool closed = true)
        {
            Vector3[] points3D = new Vector3[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                points3D[i] = new Vector3(points[i].x, points[i].y, 0);
            }
            
            PolygonSettings settings = PolygonSettings.Default;
            settings.vertices = points3D;
            settings.baseSettings.color = color;
            settings.baseSettings.drawMode = drawMode;
            settings.baseSettings.lineThickness = lineThickness;
            settings.closed = closed;
            
            Draw.Polygon(Matrix4x4.identity, settings);
        }

        #endregion

        #region Line Drawing

        public static void Line(Vector2 start, Vector2 end, float thickness, Color color, DrawMode drawMode = DrawMode.Filled, float t = 1f)
        {
            LineSettings settings = LineSettings.Default;
            settings.baseSettings.color = color;
            settings.baseSettings.drawMode = drawMode;
            settings.baseSettings.lineThickness = thickness;
            settings.baseSettings.t = t;
            settings.baseSettings.lit = false;
            
            Draw.Line(new Vector3(start.x, start.y, 0), new Vector3(end.x, end.y, 0), settings);
        }

        public static void LinePixel(Vector2 start, Vector2 end, Color color, float t = 1f)
        {
            Line(start, end, 0, color, DrawMode.PixelOutline, t);
        }

        public static void Path(Vector2[] points, float thickness, Color color, bool closed = false, DrawMode drawMode = DrawMode.Filled, float t = 1f)
        {
            Vector3[] points3D = new Vector3[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                points3D[i] = new Vector3(points[i].x, points[i].y, 0);
            }
            
            PathSettings settings = PathSettings.Default;
            settings.baseSettings.color = color;
            settings.baseSettings.lineThickness = thickness;
            settings.baseSettings.t = t;
            settings.closed = closed;
            
            Draw.Path(points3D, settings);
        }

        public static void PathPixel(Vector2[] points, Color color, bool closed = false, float t = 1f)
        {
            Path(points, 0, color, closed, DrawMode.PixelOutline, t);
        }

        public static void Arrow(Vector2 start, Vector2 end, float thickness, Color color, float headLength = 0.24f, float headAngle = 30f, DrawMode drawMode = DrawMode.Filled, float t = 1f)
        {
            ArrowSettings settings = ArrowSettings.Default;
            settings.baseSettings.color = color;
            settings.baseSettings.lineThickness = thickness;
            settings.headLength = headLength;
            settings.headAngleDegrees = headAngle;
            settings.baseSettings.drawMode = drawMode;
            settings.baseSettings.t = t;
            
            Draw.Arrow(new Vector3(start.x, start.y, 0), new Vector3(end.x, end.y, 0), settings);
        }

        public static void ArrowRay(Vector2 start, Vector2 direction, float length, float thickness, Color color, float headLength = 0.24f, float headAngle = 30f, DrawMode drawMode = DrawMode.Filled, float t = 1f)
        {
            Arrow(start, start + direction.normalized * length, thickness, color, headLength, headAngle, drawMode, t);
        }

        #endregion

        #region Convenience Methods

        public static void Point(Vector2 center, float radius, Color color)
        {
            Circle(center, radius, color, DrawMode.Filled);
        }

        public static void CircleOutline(Vector2 center, float radius, float thickness, Color color, int segments = 32, float t = 1f)
        {
            Circle(center, radius, color, DrawMode.Outline, thickness, segments);
        }

        public static void BoxOutline(Vector2 center, Vector2 size, float thickness, Color color, float t = 1f)
        {
            Rectangle(center, size, color, DrawMode.Outline, thickness);
        }

        public static void Ray(Vector2 start, Vector2 offset, float thickness, Color color, DrawMode drawMode = DrawMode.Filled, float t = 1f)
        {
            Line(start, start + offset, thickness, color, drawMode, t);
        }

        public static void RayDirection(Vector2 start, Vector2 direction, float length, float thickness, Color color, DrawMode drawMode = DrawMode.Filled, float t = 1f)
        {
            Line(start, start + direction.normalized * length, thickness, color, drawMode, t);
        }

        #endregion
    }
}