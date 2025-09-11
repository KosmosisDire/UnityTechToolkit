using System;
using UnityEngine;

public static class MathTools
{
    public static int LayerMaskToInt(LayerMask layerMask)
    {
        return (int)Mathf.Log(layerMask.value, 2);
    }

    // taken from https://gist.github.com/Davidblkx/e12ab0bb2aff7fd8072632b396538560
    /// <summary>
    ///     Calculate the difference between 2 strings using the Levenshtein distance algorithm
    /// </summary>
    /// <param name="source1">First string</param>
    /// <param name="source2">Second string</param>
    /// <returns></returns>
    public static int LevenshteinDistance(string source1, string source2) //O(n*m)
    {
        var source1Length = source1.Length;
        var source2Length = source2.Length;

        var matrix = new int[source1Length + 1, source2Length + 1];

        // First calculation, if one entry is empty return full length
        if (source1Length == 0)
            return source2Length;

        if (source2Length == 0)
            return source1Length;

        // Initialization of matrix with row size source1Length and columns size source2Length
        for (var i = 0; i <= source1Length; matrix[i, 0] = i++){}
        for (var j = 0; j <= source2Length; matrix[0, j] = j++){}

        // Calculate rows and collumns distances
        for (var i = 1; i <= source1Length; i++)
        {
            for (var j = 1; j <= source2Length; j++)
            {
                var cost = (source2[j - 1] == source1[i - 1]) ? 0 : 1;

                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost);
            }
        }
        // return result
        return matrix[source1Length, source2Length];
    }

    #region IK Utilities

    /// <summary>
    /// Clamp an angle to specified limits, handling wrap-around
    /// </summary>
    /// <param name="angle">Angle in degrees</param>
    /// <param name="minAngle">Minimum angle in degrees</param>
    /// <param name="maxAngle">Maximum angle in degrees</param>
    /// <returns>Clamped angle in degrees</returns>
    public static float ClampAngle(float angle, float minAngle, float maxAngle)
    {
        // Normalize angle to [-180, 180] range
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        
        // Normalize limits to same range
        while (minAngle > 180f) minAngle -= 360f;
        while (minAngle < -180f) minAngle += 360f;
        while (maxAngle > 180f) maxAngle -= 360f;
        while (maxAngle < -180f) maxAngle += 360f;
        
        return Mathf.Clamp(angle, minAngle, maxAngle);
    }

    /// <summary>
    /// Calculate the signed angle between two vectors around a given axis
    /// </summary>
    /// <param name="from">From vector</param>
    /// <param name="to">To vector</param>
    /// <param name="axis">Rotation axis</param>
    /// <returns>Signed angle in degrees</returns>
    public static float SignedAngle(Vector3 from, Vector3 to, Vector3 axis)
    {
        float unsignedAngle = Vector3.Angle(from, to);
        float sign = Mathf.Sign(Vector3.Dot(axis, Vector3.Cross(from, to)));
        return unsignedAngle * sign;
    }

    /// <summary>
    /// Rotate a vector around an arbitrary axis by a given angle
    /// </summary>
    /// <param name="vector">Vector to rotate</param>
    /// <param name="axis">Rotation axis (normalized)</param>
    /// <param name="angle">Angle in degrees</param>
    /// <returns>Rotated vector</returns>
    public static Vector3 RotateAroundAxis(Vector3 vector, Vector3 axis, float angle)
    {
        return Quaternion.AngleAxis(angle, axis) * vector;
    }

    /// <summary>
    /// Calculate the rotation needed to align one vector with another around a specific axis
    /// </summary>
    /// <param name="from">Current direction</param>
    /// <param name="to">Target direction</param>
    /// <param name="axis">Rotation axis</param>
    /// <returns>Rotation angle in degrees</returns>
    public static float CalculateRotationAngle(Vector3 from, Vector3 to, Vector3 axis)
    {
        // Project both vectors onto the plane perpendicular to the axis
        Vector3 fromProjected = Vector3.ProjectOnPlane(from, axis).normalized;
        Vector3 toProjected = Vector3.ProjectOnPlane(to, axis).normalized;
        
        // Calculate the signed angle between the projected vectors
        return SignedAngle(fromProjected, toProjected, axis);
    }

    /// <summary>
    /// Apply damping to an angle change to prevent oscillation
    /// </summary>
    /// <param name="angleChange">Desired angle change</param>
    /// <param name="dampingFactor">Damping factor (0-1, where 1 = no damping)</param>
    /// <returns>Damped angle change</returns>
    public static float DampAngleChange(float angleChange, float dampingFactor = 0.5f)
    {
        return angleChange * Mathf.Clamp01(dampingFactor);
    }

    /// <summary>
    /// Check if a point is within reach of a joint chain
    /// </summary>
    /// <param name="jointPositions">Array of joint positions</param>
    /// <param name="target">Target position</param>
    /// <returns>True if target is within reach</returns>
    public static bool IsTargetReachable(Vector3[] jointPositions, Vector3 target)
    {
        if (jointPositions.Length < 2) return false;
        
        // Calculate total reach distance
        float totalReach = 0f;
        for (int i = 0; i < jointPositions.Length - 1; i++)
        {
            totalReach += Vector3.Distance(jointPositions[i], jointPositions[i + 1]);
        }
        
        // Check if target is within reach from the base
        float distanceToTarget = Vector3.Distance(jointPositions[0], target);
        return distanceToTarget <= totalReach;
    }

    /// <summary>
    /// Linear interpolation with easing for smooth IK convergence
    /// </summary>
    /// <param name="from">Start value</param>
    /// <param name="to">End value</param>
    /// <param name="t">Interpolation factor (0-1)</param>
    /// <param name="easeType">Type of easing to apply</param>
    /// <returns>Interpolated value</returns>
    public static float LerpWithEasing(float from, float to, float t, EaseType easeType = EaseType.None)
    {
        t = Mathf.Clamp01(t);
        
        switch (easeType)
        {
            case EaseType.EaseInOut:
                t = t * t * (3f - 2f * t); // Smoothstep
                break;
            case EaseType.EaseOut:
                t = 1f - (1f - t) * (1f - t);
                break;
            case EaseType.EaseIn:
                t = t * t;
                break;
        }
        
        return Mathf.Lerp(from, to, t);
    }

    public enum EaseType
    {
        None,
        EaseIn,
        EaseOut,
        EaseInOut
    }

    /// <summary>
    /// Calculate the error between current and target position/rotation
    /// </summary>
    /// <param name="currentPos">Current position</param>
    /// <param name="targetPos">Target position</param>
    /// <param name="currentRot">Current rotation</param>
    /// <param name="targetRot">Target rotation</param>
    /// <param name="positionWeight">Weight for position error (0-1)</param>
    /// <param name="rotationWeight">Weight for rotation error (0-1)</param>
    /// <returns>Total weighted error</returns>
    public static float CalculatePoseError(Vector3 currentPos, Vector3 targetPos, 
                                         Quaternion currentRot, Quaternion targetRot,
                                         float positionWeight = 1f, float rotationWeight = 0.1f)
    {
        float positionError = Vector3.Distance(currentPos, targetPos) * positionWeight;
        float rotationError = Quaternion.Angle(currentRot, targetRot) * Mathf.Deg2Rad * rotationWeight;
        
        return positionError + rotationError;
    }

    #endregion
}