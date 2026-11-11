using System.Collections;
using UnityEngine;

public class SideMenuController : MonoBehaviour
{
    public RectTransform panel;

    public float slideDistance = 600f;  
    public float slideDuration = 0.3f;
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    Vector2 shownPos;
    Vector2 hiddenPos;
    bool isOpen;
    Coroutine routine;

    void Awake()
    {
        if (!panel) panel = GetComponent<RectTransform>();

        shownPos = panel.anchoredPosition;
        hiddenPos = shownPos + Vector2.left * slideDistance;

        panel.anchoredPosition = hiddenPos;
        isOpen = false;
    }

    public void ToggleMenu()
    {
        if (isOpen) CloseMenu();
        else OpenMenu();
    }

    public void OpenMenu()
    {
        if (isOpen) return;
        isOpen = true;
        Slide(shownPos);
    }

    public void CloseMenu()
    {
        if (!isOpen) return;
        isOpen = false;
        Slide(hiddenPos);
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