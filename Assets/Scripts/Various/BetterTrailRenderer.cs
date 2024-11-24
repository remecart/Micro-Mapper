using System;
using System.Collections;
using UnityEngine;

public class BetterTrailRenderer : MonoBehaviour
{
    public LineRenderer lr;
    public int verticies;
    private bool started;
    public float repeatInterval;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(RepeatAction());
    }

    private IEnumerator RepeatAction()
    {
        while (true)
        {
            if (lr == null)
            {
                Debug.LogError("LineRenderer component not found.");
            }
            else if (SpawnObjects.instance == null)
            {
                Debug.LogError("SpawnObjects.instance is not set.");
            }
            else
            {
                lr.positionCount++;
                Vector3 newPosition = new Vector3(
                    transform.position.x,
                    transform.position.y,
                    transform.position.z - SpawnObjects.instance.PositionFromBeat(SpawnObjects.instance.currentBeat) * SpawnObjects.instance.editorScale
                );

                lr.SetPosition(lr.positionCount - 1, newPosition);
            }
            // Wait for the specified interval before repeating
            yield return new WaitForSeconds(1f / repeatInterval);
        }
    }

    private void Update()
    {
        if (lr.positionCount > verticies)
        {
            RemoveFirstPosition();
        }
    }

    void RemoveFirstPosition()
    {
        if (lr.positionCount > 1)
        {
            for (int i = 0; i < lr.positionCount - 1; i++)
            {
                lr.SetPosition(i, lr.GetPosition(i + 1));
            }
            lr.positionCount--;
        }
        else if (lr.positionCount == 1)
        {
            lr.positionCount = 0;
        }
    }
}
