using UnityEngine;

namespace Toolkit.Visualization
{
    public enum DrawMode
    {
        Filled,
        Outline,
        PixelOutline
    }

    // Base settings that all shapes share
    [System.Serializable]
    public struct BaseShapeSettings
    {
        public Color color;
        public DrawMode drawMode;
        public float lineThickness; // For outline modes
        public float t; // Animation parameter (0-1)
		public bool lit; // Whether to be affected by scene lighting (only for 3D shapes)

        public static BaseShapeSettings Default => new BaseShapeSettings
		{
			color = Color.white,
			drawMode = DrawMode.Filled,
			lineThickness = 0.02f,
			t = 1f,
			lit = false
		};
    }

    [System.Serializable]
    public struct SphereSettings
    {
        public BaseShapeSettings baseSettings;
        public float radius;
        public int segments; // For outline mode

        public static SphereSettings Default => new SphereSettings
        {
            baseSettings = BaseShapeSettings.Default,
            radius = 1f,
            segments = 32
        };
    }

    [System.Serializable]
    public struct BoxSettings
    {
        public BaseShapeSettings baseSettings;
        public Vector3 size;
        public bool roundedCorners;
        public float cornerRadius;

        public static BoxSettings Default => new BoxSettings
        {
            baseSettings = BaseShapeSettings.Default,
            size = Vector3.one,
            roundedCorners = false,
            cornerRadius = 0.1f
        };
    }

    [System.Serializable]
    public struct LineSettings
    {
		public BaseShapeSettings baseSettings;
        public bool roundedCaps;

        public static LineSettings Default => new LineSettings
        {
			baseSettings = BaseShapeSettings.Default,
            roundedCaps = false,
        };
    }

    [System.Serializable]
    public struct TriangleSettings
    {
        public BaseShapeSettings baseSettings;
        public Vector3[] vertices; // 3 vertices
        public bool roundedCorners;
        public float cornerRadius;

        public static TriangleSettings Default => new TriangleSettings
        {
            baseSettings = BaseShapeSettings.Default,
            vertices = new Vector3[] {
                new Vector3(-0.5f, 0, 0),
                new Vector3(0.5f, 0, 0),
                new Vector3(0, 1, 0)
            },
            roundedCorners = false,
            cornerRadius = 0.1f
        };
    }

    [System.Serializable]
    public struct QuadSettings
    {
        public BaseShapeSettings baseSettings;
        public Vector3[] vertices; // 4 vertices, null means use size/center
        public bool roundedCorners;
        public float cornerRadius;

        public static QuadSettings Default => new QuadSettings
        {
            baseSettings = BaseShapeSettings.Default,
            vertices = null,
            roundedCorners = false,
            cornerRadius = 0.1f
        };
    }

	[System.Serializable]
	public struct CircleSettings
	{
		public BaseShapeSettings baseSettings;
		public float radius;

		public static CircleSettings Default => new CircleSettings
		{
			baseSettings = BaseShapeSettings.Default,
			radius = 1f
		};
	}

    [System.Serializable]
    public struct PolygonSettings
    {
        public BaseShapeSettings baseSettings;
        public Vector3[] vertices;
        public bool closed;

        public static PolygonSettings Default => new PolygonSettings
        {
            baseSettings = BaseShapeSettings.Default,
            vertices = new Vector3[0],
            closed = true
        };
    }

    [System.Serializable]
    public struct PathSettings
    {
		public BaseShapeSettings baseSettings;
        public bool closed;
        public bool roundedJoints;

        public static PathSettings Default => new PathSettings
        {
			baseSettings = BaseShapeSettings.Default,
            closed = false,
            roundedJoints = false
        };
    }

    [System.Serializable]
    public struct ArrowSettings
    {
		public BaseShapeSettings baseSettings;
        public float headLength;
        public float headAngleDegrees;

        public static ArrowSettings Default => new ArrowSettings
        {
			baseSettings = BaseShapeSettings.Default,
            headLength = 0.24f,
            headAngleDegrees = 30f,
        };
    }

    [System.Serializable]
    public struct CylinderSettings
    {
        public BaseShapeSettings baseSettings;
        public float height;
        public float radius;
        public int segments; // For outline mode

        public static CylinderSettings Default => new CylinderSettings
        {
            baseSettings = BaseShapeSettings.Default,
            height = 2f,
            radius = 1f,
            segments = 32
        };
    }

    [System.Serializable]
    public struct ConeSettings
    {
        public BaseShapeSettings baseSettings;
        public float height;
        public float radius;
        public int segments; // For outline mode

        public static ConeSettings Default => new ConeSettings
        {
            baseSettings = BaseShapeSettings.Default,
            height = 2f,
            radius = 1f,
            segments = 32
        };
    }

    [System.Serializable]
    public struct CapsuleSettings
    {
        public BaseShapeSettings baseSettings;
        public float height;
        public float radius;
        public int segments; // For outline mode

        public static CapsuleSettings Default => new CapsuleSettings
        {
            baseSettings = BaseShapeSettings.Default,
            height = 2f,
            radius = 0.5f,
            segments = 32
        };
    }

    [System.Serializable]
    public struct TorusSettings
    {
        public BaseShapeSettings baseSettings;
        public float majorRadius;
        public float minorRadius;
        public int majorSegments;
        public int minorSegments;

        public static TorusSettings Default => new TorusSettings
        {
            baseSettings = BaseShapeSettings.Default,
            majorRadius = 1f,
            minorRadius = 0.3f,
            majorSegments = 32,
            minorSegments = 16
        };
    }
}