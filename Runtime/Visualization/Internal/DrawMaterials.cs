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

		public static readonly int colorID = Shader.PropertyToID("_Color");
		public static readonly int sizeID = Shader.PropertyToID("_Size");

		public static readonly int quadPointA = Shader.PropertyToID("PosA");
		public static readonly int quadPointB = Shader.PropertyToID("PosB");
		public static readonly int quadPointC = Shader.PropertyToID("PosC");
		public static readonly int quadPointD = Shader.PropertyToID("PosD");

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
		}
	}
}