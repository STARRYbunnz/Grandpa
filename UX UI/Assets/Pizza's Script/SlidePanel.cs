using System.Collections;
using UnityEngine;

public class SlidePanel : MonoBehaviour
{
    public RectTransform panel;

    public float slideDistance = 400f;
    public float slideDuration = 0.4f;
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    Vector2 openPos;
    Vector2 closedPos;
    bool slidOut;
    Coroutine routine;

    void Awake()
    {
        if (!panel) panel = GetComponent<RectTransform>();

        openPos = panel.anchoredPosition;
        closedPos = openPos + Vector2.down * slideDistance;
    }

    public void Toggle()
    {
        if (slidOut) SlideIn();
        else SlideOut();
    }

    public void SlideOut()
    {
        if (slidOut) return;
        slidOut = true;
        Slide(closedPos);
    }

    public void SlideIn()
    {
        if (!slidOut) return;
        slidOut = false;
        Slide(openPos);
    }

    void Slide(Vector2 target)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(SlideRoutine(target));
    }

    IEnumerator SlideRoutine(Vector2 target)
    {
        Vector2 start = panel.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = easeCurve.Evaluate(elapsed / slideDuration);
            panel.anchoredPosition = Vector2.LerpUnclamped(start, target, t);
            yield return null;
        }

        panel.anchoredPosition = target;
    }
}