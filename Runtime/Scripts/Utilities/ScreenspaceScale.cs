using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ScreenspaceScale : MonoBehaviour
{
    public float scale = 1;
    public float scalePower = 1;

    void LateUpdate()
    {
        var camera = Camera.main;
        if (!Application.isPlaying)
        {
            camera = Camera.current;
        }

        if (camera != null)
        {
            // we project the object's position as if it was centered on the camera is screenspace
            // then take the distance to that point. 
            // This way the object doesn't change size when panning the camera

            var centerRay = camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            var projectedPosition = Math3d.ProjectPointOnLine(centerRay.origin, centerRay.direction, transform.position);
            var distance = Vector3.Distance(projectedPosition, camera.transform.position);

            transform.localScale = MathF.Pow(MathF.Sqrt(distance), scalePower) * scale * Vector3.one;
        }
    }
}
