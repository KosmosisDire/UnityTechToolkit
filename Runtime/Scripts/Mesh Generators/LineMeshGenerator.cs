using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Toolkit.MeshGeneration
{
	public static class LineMeshGenerator
	{
		static Mesh cachedMesh;

		public static Mesh GetIdentityMesh()
		{
			if (cachedMesh == null)
			{
				cachedMesh = GenerateLineMesh();
			}
			return cachedMesh;
		}

        static Mesh GenerateLineMesh()
        {
            Mesh mesh = new Mesh();
            mesh.SetVertices(new List<Vector3>() { new Vector3(0, 0, 0), new Vector3(1, 0, 0) });
            mesh.SetIndices(new int[] { 0, 1 }, MeshTopology.Lines, 0);
            return mesh;
        }
	}
}