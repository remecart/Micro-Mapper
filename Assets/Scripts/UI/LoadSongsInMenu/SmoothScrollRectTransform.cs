using UnityEngine;
using System.Collections;

public class SmoothScrollRectTransform : MonoBehaviour
{
    public RectTransform targetRect;
    public float baseScrollSpeed = 5f;
    public float scollDuration = 0.1f;
    private Vector2 targetPosition;
    private Coroutine currentScrollCoroutine;
    private float timeSinceLastScroll;

    private void Update()
    {
        if (Input.mouseScrollDelta.y != 0)
        {
            float dynamicSpeed = baseScrollSpeed + (1f / Mathf.Max(0.01f, timeSinceLastScroll)); // Increase speed based on frequency
            targetPosition = targetRect.anchoredPosition + new Vector2(0, -Input.mouseScrollDelta.y * dynamicSpeed);

            if (currentScrollCoroutine != null) StopCoroutine(currentScrollCoroutine);
            currentScrollCoroutine = StartCoroutine(SmoothScrollCoroutine(targetPosition));

            timeSinceLastScroll = 0f;
        }

        timeSinceLastScroll += Time.deltaTime;
    }

    private IEnumerator SmoothScrollCoroutine(Vector2 target)
    {
        Vector2 startPosition = targetRect.anchoredPosition;
        float duration = scollDuration;
        if (duration > timeSinceLastScroll) duration = timeSinceLastScroll;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            targetRect.anchoredPosition = Vector2.Lerp(startPosition, target, t);
            yield return null;
        }

        targetRect.anchoredPosition = target;
    }
}
