using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailRenderer_Local : MonoBehaviour
{
    // Source: https://stackoverflow.com/questions/74230206/how-to-change-trailrenderers-space-to-localspace

    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private Transform player;

    private Vector3 lastFramePosition;

    private void Start()
    {
        lastFramePosition = player.position;
    }

    private void Update()
    {
        var delta = player.position - lastFramePosition;
        lastFramePosition = player.position;

        var positions = new Vector3[trailRenderer.positionCount];
        trailRenderer.GetPositions(positions);

        for (var i = 0; i < trailRenderer.positionCount; i++)
        {
            positions[i] += delta;
        }

        trailRenderer.SetPositions(positions);
    }
}