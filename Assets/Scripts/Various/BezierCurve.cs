using UnityEngine;

public class BezierCurve : MonoBehaviour
{
    private Vector2 controlPointA;
    private Vector2 controlPointB;

    public void Curve(Vector2 pointA, Vector2 pointB, float angleA, float angleB, float intensity, float overshoot, int resolution)
    {
        CalculateControlPoints(pointA, pointB, angleA, angleB, intensity, overshoot);
        DrawBezierCurve(pointA, pointB, resolution);
    }

    void CalculateControlPoints(Vector2 pointA, Vector2 pointB, float angleA, float angleB, float intensity, float overshoot)
    {
        float distance = (pointB - pointA).magnitude;

        // Apply overshoot if points are very close or equal
        if (distance < 0.01f)
        {
            distance = 1.0f * overshoot;  // Add overshoot for better visual effect
        }

        float handleLengthA = distance * 0.3f * intensity * overshoot;  // Adjust handle length with overshoot
        float handleLengthB = distance * 0.3f * intensity * overshoot;

        controlPointA = pointA + new Vector2(Mathf.Cos(angleA * Mathf.Deg2Rad), Mathf.Sin(angleA * Mathf.Deg2Rad)) * handleLengthA;
        controlPointB = pointB + new Vector2(Mathf.Cos(angleB * Mathf.Deg2Rad), Mathf.Sin(angleB * Mathf.Deg2Rad)) * handleLengthB;
    }

    Vector2 GetBezierPoint(float t, Vector2 pointA, Vector2 pointB)
    {
        return Mathf.Pow(1 - t, 3) * pointA +
               3 * Mathf.Pow(1 - t, 2) * t * controlPointA +
               3 * (1 - t) * Mathf.Pow(t, 2) * controlPointB +
               Mathf.Pow(t, 3) * pointB;
    }

    void DrawBezierCurve(Vector2 pointA, Vector2 pointB, int resolution)
    {
        Vector2 previousPoint = pointA;

        for (int i = 1; i <= resolution; i++)
        {
            float t = i / (float)resolution;
            Vector2 currentPoint = GetBezierPoint(t, pointA, pointB);
            Gizmos.DrawLine(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }
    }
}
