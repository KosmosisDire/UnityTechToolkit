using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(LineRenderer))]
public class ScreenspaceLineRenderer : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public float widthMultiplier = 1f;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        lineRenderer.widthMultiplier = (Mathf.Abs(transform.lossyScale.x) + Mathf.Abs(transform.lossyScale.y) + Mathf.Abs(transform.lossyScale.z)) / 3f * widthMultiplier;
    }
}
