using System;
using UnityEngine;

namespace Toolkit.MeshGeneration
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [ExecuteAlways]
    public class ConeShape : MonoBehaviour
    {
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;

        private Mesh lastMesh;
        public ConeShapeData lastConeData;
        public ConeShapeData coneData;

        public Mesh LastMesh => lastMesh;

        void Update()
        {
            if (!meshFilter || !meshRenderer)
            {
                meshFilter = GetComponent<MeshFilter>();
                meshRenderer = GetComponent<MeshRenderer>();
            }

            if (!meshFilter || !meshRenderer)
                return;

            if (lastMesh == null || lastConeData != coneData)
            {
                lastConeData = coneData;
                lastMesh = ConeMeshGenerator.Generate(coneData);
                meshFilter.sharedMesh = lastMesh;
            }
        }
    }

}