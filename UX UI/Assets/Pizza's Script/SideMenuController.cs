using System.Collections;
using UnityEngine;

public class SideMenuController : MonoBehaviour
{
    [Header("References")]
    public RectTransform panel;

    [Header("Settings")]
    public float animationDuration = 0.3f;
    public bool startOpen = false;

    private Vector2 hiddenPos;
    private Vector2 shownPos;
    private bool isOpen;
    private Coroutine currentAnim;

    void Awake()
    {
        float width = panel.rect.width;

        shownPos = new Vector2(0, panel.anchoredPosition.y);
        hiddenPos = new Vector2(-width, panel.anchoredPosition.y);

        isOpen = startOpen;
        panel.anchoredPosition = isOpen ? shownPos : hiddenPos;
    }

    public void ToggleMenu()
    {
        if (currentAnim != null)
            StopCoroutine(currentAnim);

        Vector2 target = isOpen ? hiddenPos : shownPos;
        currentAnim = StartCoroutine(AnimatePanel(target));
        isOpen = !isOpen;
    }

    private IEnumerator AnimatePanel(Vector2 target)
    {
        Vector2 start = panel.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);
            // ease-out for a nice smooth feel
            t = 1f - Mathf.Pow(1f - t, 3f);
            panel.anchoredPosition = Vector2.Lerp(start, target, t);
            yield return null;
        }

        panel.anchoredPosition = target;
    }
}