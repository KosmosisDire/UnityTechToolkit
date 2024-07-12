using System.Collections.Generic;
using UnityEngine;

namespace Toolkit.MeshGeneration
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(LineRenderer))]
    [RequireComponent(typeof(ScreenspaceLineRenderer))]
    [ExecuteAlways]
    public class PlaneShape : MonoBehaviour
    {
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private LineRenderer lineRenderer;
        private ScreenspaceLineRenderer lineRendererLocalWidth;

        private Mesh lastMesh;
        private PlaneShapeData lastPlaneData;
        public PlaneShapeData planeData;


        public Mesh LastMesh => lastMesh;

        
        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            if (!meshFilter || !meshRenderer)
            {
                meshFilter = GetComponent<MeshFilter>();
                meshRenderer = GetComponent<MeshRenderer>();
                lineRenderer = GetComponent<LineRenderer>();
                lineRendererLocalWidth = GetComponent<ScreenspaceLineRenderer>();
            }

            if (!meshFilter || !meshRenderer)
                return;

            if (lastMesh == null || lastPlaneData != planeData)
            {
                lastMesh = PlaneMeshGenerator.Generate(planeData);
                meshFilter.sharedMesh = lastMesh;
                lastPlaneData = planeData;

                if (lineRenderer)
                {
                    lineRenderer.positionCount = 4;
                    lineRenderer.loop = true;
                    lineRendererLocalWidth.widthMultiplier = planeData.outlineWidth;
                    lineRenderer.useWorldSpace = false;

                    // use plane vertices to draw outline
                    lineRenderer.SetPositions(new Vector3[]
                    {
                        lastMesh.vertices[0],
                        lastMesh.vertices[1],
                        lastMesh.vertices[2],
                        lastMesh.vertices[3],
                    });
                }
            }
        }
    }
}