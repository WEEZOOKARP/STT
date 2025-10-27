using UnityEngine;

public static class DamageIndicatorMath
{
    /// <summary>
    /// Computes the signed planar angle (in degrees) between the observer's forward vector and the source direction.
    /// Positive values indicate sources on the observer's right side, negative values on the left.
    /// </summary>
    public static float CalculateSignedAngle(Vector3 observerForward, Vector3 directionToSource, Vector3 upAxis)
    {
        Vector3 flatForward = Vector3.ProjectOnPlane(observerForward, upAxis).normalized;
        Vector3 flatDirection = Vector3.ProjectOnPlane(directionToSource, upAxis).normalized;

        if (flatForward.sqrMagnitude < 0.0001f || flatDirection.sqrMagnitude < 0.0001f)
        {
            return 0f;
        }

        return Vector3.SignedAngle(flatForward, flatDirection, upAxis);
    }

    /// <summary>
    /// Returns a 0-1 value that represents how far the source is within the configured max distance.
    /// </summary>
    public static float NormalizeDistance(float distance, float maxRelevantDistance)
    {
        if (maxRelevantDistance <= 0f)
        {
            return 0f;
        }

        return Mathf.Clamp01(distance / maxRelevantDistance);
    }

    /// <summary>
    /// Provides a quick helper for blending two colors based on the provided weight.
    /// </summary>
    public static Color Blend(Color from, Color to, float weight)
    {
        return Color.Lerp(from, to, Mathf.Clamp01(weight));
    }
}
