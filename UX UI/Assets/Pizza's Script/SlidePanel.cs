using System.Collections;
using UnityEngine;

public class SlidePanel : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The panel that slides. Defaults to this object's RectTransform if left empty.")]
    public RectTransform panel;

    [Header("Slide Settings")]
    [Tooltip("How far down (in UI pixels) the panel slides when it slides out.")]
    public float slideDistance = 400f;
    public float slideDuration = 0.4f;
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Vector2 _openPosition;   // original visible position
    private Vector2 _closedPosition; // slid down / hidden position
    private bool _hasSlidOut;
    private Coroutine _slideRoutine;

    private void Awake()
    {
        if (panel == null)
            panel = GetComponent<RectTransform>();

        _openPosition = panel.anchoredPosition;
        _closedPosition = _openPosition + new Vector2(0f, -slideDistance);
    }


    public void Toggle()
    {
        if (_hasSlidOut)
            SlideIn();
        else
            SlideOut();
    }

    /// <summary>Slides the panel down and out of view. Does nothing if it's already out.</summary>
    public void SlideOut()
    {
        if (_hasSlidOut) return;
        _hasSlidOut = true;

        if (_slideRoutine != null)
            StopCoroutine(_slideRoutine);
        _slideRoutine = StartCoroutine(SlideRoutine(_closedPosition));
    }

    /// <summary>Slides the panel back up into view. Does nothing if it's already in.</summary>
    public void SlideIn()
    {
        if (!_hasSlidOut) return;
        _hasSlidOut = false;

        if (_slideRoutine != null)
            StopCoroutine(_slideRoutine);
        _slideRoutine = StartCoroutine(SlideRoutine(_openPosition));
    }

    private IEnumerator SlideRoutine(Vector2 target)
    {
        Vector2 start = panel.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = easeCurve.Evaluate(Mathf.Clamp01(elapsed / slideDuration));
            panel.anchoredPosition = Vector2.LerpUnclamped(start, target, t);
            yield return null;
        }

        panel.anchoredPosition = target; // snap exactly to the target at the end
    }
}