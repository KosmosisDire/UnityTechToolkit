using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Toolkit.Visualization.Internal
{
	public static class VisMath
	{
		public static float RayLineIntersectionDistance(Vector3 rayOrigin, Vector3 rayDir, Vector3 lineA, Vector3 lineB)
		{
			var res = Math3d.LineLineIntersection(out var intersection, rayOrigin, rayDir, lineA, lineB - lineA);
			if (res)
			{
				return Vector3.Distance(rayOrigin, intersection);
			}
			return Mathf.Infinity;
		}
	}
}