using UnityEngine;
using System.Collections.Generic;

public class CustomTrail : MonoBehaviour
{
    public float trailLifetime = 1.0f; // Time in seconds for each segment to last
    public float minSegmentDistance = 0.1f; // Minimum distance between trail points
    public LineRenderer lineRenderer; // Assign a LineRenderer in the Inspector

    private List<Vector3> points = new List<Vector3>();
    private List<float> pointTimes = new List<float>();

    void Update()
    {
        float currentTime = Time.time;

        // Add a new point if the object has moved enough
        if (points.Count == 0 || Vector3.Distance(points[points.Count - 1], transform.position) > minSegmentDistance)
        {
            points.Add(transform.position);
            pointTimes.Add(currentTime);
        }

        // Remove old points that are past their lifetime
        while (pointTimes.Count > 0 && currentTime - pointTimes[0] > trailLifetime)
        {
            points.RemoveAt(0);
            pointTimes.RemoveAt(0);
        }

        // Update the LineRenderer
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }
}