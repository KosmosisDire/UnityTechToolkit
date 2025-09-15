using UnityEngine;

namespace Toolkit.Visualization.Internal
{
	public static class DrawMaterials
	{

		public static Material unlitMat { get; private set; }
		public static Material shadedMat { get; private set; }
		public static Material pointMat { get; private set; }
		public static Material lineMatRoundedEdge { get; private set; }
		public static Material lineMat { get; private set; }
		public static Material linePixelMat { get; private set; }
		public static Material quadMat { get; private set; }
		public static Material triangleMat { get; private set; }
		public static Material circleMat { get; private set; }
		public static Material rectangleMat { get; private set; }

		public static readonly int colorID = Shader.PropertyToID("_Color");
		public static readonly int sizeID = Shader.PropertyToID("_Size");

		public static readonly int quadPointA = Shader.PropertyToID("PosA");
		public static readonly int quadPointB = Shader.PropertyToID("PosB");
		public static readonly int quadPointC = Shader.PropertyToID("PosC");
		public static readonly int quadPointD = Shader.PropertyToID("PosD");


		public static readonly int trianglePointA = Shader.PropertyToID("_TrianglePointA");
		public static readonly int trianglePointB = Shader.PropertyToID("_TrianglePointB");
		public static readonly int trianglePointC = Shader.PropertyToID("_TrianglePointC");
		public static readonly int roundedEdgesID = Shader.PropertyToID("_RoundedEdges");
		public static readonly int roundRadiusID = Shader.PropertyToID("_RoundRadius");


		public static readonly int outlineColorID = Shader.PropertyToID("_OutlineColor");
		public static readonly int outlineWidthID = Shader.PropertyToID("_OutlineWidth");
		public static readonly int usePixelOutlineID = Shader.PropertyToID("_UsePixelOutline");

		public static readonly int rectCenterID = Shader.PropertyToID("_RectCenter");
		public static readonly int rectSizeID = Shader.PropertyToID("_RectSize");
		public static readonly int rectRoundedCornersID = Shader.PropertyToID("_RoundedCorners");
		public static readonly int rectOutlinePixelsID = Shader.PropertyToID("_OutlineWidth");

		public static void Init()
		{
			if (unlitMat == null)
			{
				unlitMat = new Material(Shader.Find("Visualization/UnlitColorAlpha"));
			}
			if (shadedMat == null)
			{
				shadedMat = new Material(Shader.Find("Visualization/Shaded"));
			}
			if (pointMat == null)
			{
				pointMat = new Material(Shader.Find("Visualization/UnlitPoint"));
			}
			if (lineMatRoundedEdge == null)
			{
				lineMatRoundedEdge = new Material(Shader.Find("Visualization/LineRounded"));
			}
			if (quadMat == null)
			{
				quadMat = new Material(Shader.Find("Visualization/Quad"));
			}
			if (lineMat == null)
			{
				lineMat = new Material(Shader.Find("Visualization/Line"));
			}
			if (linePixelMat == null)
			{
				linePixelMat = new Material(Shader.Find("Visualization/LinePixel"));
			}
			if (triangleMat == null)
			{
				triangleMat = new Material(Shader.Find("Visualization/Triangle"));
			}
			if (circleMat == null)
			{
				circleMat = new Material(Shader.Find("Visualization/Circle"));
			}

		}
	}
}