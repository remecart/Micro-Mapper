using UnityEngine;

public class TrailRendererHighFrequency : MonoBehaviour
{
    public TrailRenderer trailRenderer;
    public float emissionInterval = 0.002f; // Interval between trail points (500 times per second)

    private float accumulatedTime = 0f;

    void FixedUpdate()
    {
        if (trailRenderer == null)
            return;

        accumulatedTime += Time.fixedDeltaTime;

        while (accumulatedTime >= emissionInterval)
        {
            trailRenderer.emitting = true; // Enable trail emission
            accumulatedTime -= emissionInterval; // Deduct the interval
        }

        trailRenderer.emitting = false; // Disable emission between intervals
    }
}