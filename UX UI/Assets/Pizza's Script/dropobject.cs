using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class FallingDraggableUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RectTransform dropArea;
    public Canvas canvas;

    public float returnSpeed = 1200f;

    public float heldScaleMultiplier = 1.2f;
    public float scaleSpeed = 10f;

    public float bounceHeight = 25f;
    public float bounceDuration = 0.3f;

    static readonly Vector3[] corners = new Vector3[4];

    RectTransform rect;
    bool isHeld;
    bool isResting;
    bool isBouncing;
    float bounceTimer;
    Vector2 landedPosition;
    Vector3 originalScale;
    Vector3 targetScale;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        originalScale = rect.localScale;
        targetScale = originalScale;

        if (!canvas) canvas = GetComponentInParent<Canvas>();
    }

    void Update()
    {
        rect.localScale = Vector3.Lerp(rect.localScale, targetScale, Time.deltaTime * scaleSpeed);

        if (isBouncing)
        {
            bounceTimer += Time.deltaTime;
            float t = bounceTimer / bounceDuration;

            if (t >= 1f)
            {
                isBouncing = false;
                rect.anchoredPosition = landedPosition;
            }
            else
            {
                rect.anchoredPosition = landedPosition + Vector2.up * (bounceHeight * Mathf.Sin(t * Mathf.PI));
            }
        }
        else if (!isHeld && !isResting && dropArea)
        {
            Rect area = WorldRect(dropArea);

            if (WorldRect(rect).Overlaps(area))
            {
                isResting = true;
                isBouncing = true;
                bounceTimer = 0f;
                landedPosition = rect.anchoredPosition;
                Debug.Log(gameObject.name + " placed in the drop area!");
                return;
            }

            Vector2 pos = rect.position;
            Vector2 closest = new Vector2(
                Mathf.Clamp(pos.x, area.xMin, area.xMax),
                Mathf.Clamp(pos.y, area.yMin, area.yMax));

            rect.position = Vector3.MoveTowards(rect.position, closest, returnSpeed * Time.deltaTime);
        }
    }

    Rect WorldRect(RectTransform target)
    {
        target.GetWorldCorners(corners);
        return new Rect(corners[0].x, corners[0].y, corners[2].x - corners[0].x, corners[2].y - corners[0].y);
    }

    public void OnPointerDown(PointerEventData e)
    {
        isHeld = true;
        isResting = false;
        isBouncing = false;
        targetScale = originalScale * heldScaleMultiplier;
    }

    public void OnDrag(PointerEventData e)
    {
        if (!isHeld) return;

        float scale = canvas ? canvas.scaleFactor : 1f;
        rect.anchoredPosition += e.delta / scale;
    }

    public void OnPointerUp(PointerEventData e)
    {
        isHeld = false;
        targetScale = originalScale;
    }
}