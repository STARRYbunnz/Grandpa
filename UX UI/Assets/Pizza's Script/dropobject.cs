using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Attach this to a UI element (Image/Button with RectTransform).
/// - Grows a bit while being held/dragged.
/// - Shrinks back to normal size on release.
/// - If released while already overlapping "dropArea", it stays exactly where you placed it
///   (anywhere inside the area — top, middle, bottom, doesn't matter).
/// - If released outside the drop area, it automatically moves back toward the area
///   (from whichever direction it was dropped) until it enters it, then stops and stays.
/// - On landing, it does a quick vertical hop bounce before settling.
///
/// DRAW ORDER: this version never touches sibling order at runtime, so it will never
/// move in the Hierarchy panel and will never jump in front of anything. In Unity's UI
/// system, draw order = Hierarchy order (things lower in the list draw on top). So to
/// make sure this object always stays behind your scene/background object, just place
/// it ABOVE that object in the Hierarchy panel once, in the Editor. No code needed for that.
///
/// SETUP NOTES:
/// 1. Object needs a Graphic component (Image etc.) with "Raycast Target" checked so it can receive pointer events.
/// 2. Scene needs an EventSystem (GameObject > UI > Event System) and the Canvas needs a GraphicRaycaster.
/// 3. dropArea = a RectTransform marking the valid placement zone.
/// 4. In the Hierarchy panel, drag this object to sit ABOVE (before) whatever it should
///    never appear in front of. That's it — no toggle, no field, it just won't reorder.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class FallingDraggableUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("References")]
    [Tooltip("RectTransform marking the zone the object can be placed anywhere inside.")]
    public RectTransform dropArea;

    [Tooltip("Canvas this object lives on. Auto-found if left empty.")]
    public Canvas canvas;

    [Header("Return Settings")]
    [Tooltip("How fast the object travels back toward the drop area when released outside it (UI units/sec).")]
    public float returnSpeed = 1200f;

    [Header("Scale Settings")]
    [Tooltip("How much bigger the object gets while being held (1.2 = 20% bigger).")]
    public float heldScaleMultiplier = 1.2f;

    [Tooltip("How fast it grows/shrinks toward the target scale.")]
    public float scaleSpeed = 10f;

    [Header("Landing Bounce")]
    [Tooltip("How high the object hops up when it lands, in UI units.")]
    public float bounceHeight = 25f;

    [Tooltip("How long the landing bounce animation lasts, in seconds.")]
    public float bounceDuration = 0.3f;

    // --- internal state ---
    private RectTransform rect;
    private bool isHeld;
    private bool isResting;
    private bool isBouncing;
    private float bounceTimer;
    private Vector2 landedPosition;
    private Vector3 originalScale;
    private Vector3 targetScale;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        originalScale = rect.localScale;
        targetScale = originalScale;

        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();
    }

    void Update()
    {
        rect.localScale = Vector3.Lerp(rect.localScale, targetScale, Time.deltaTime * scaleSpeed);

        if (isBouncing)
        {
            RunBounce();
        }
        else if (!isHeld && !isResting)
        {
            MoveBackToArea();
        }
    }

    // Hop straight up in a smooth arc, then settle back down at the exact landing spot.
    private void RunBounce()
    {
        bounceTimer += Time.deltaTime;
        float t = Mathf.Clamp01(bounceTimer / bounceDuration);

        if (t >= 1f)
        {
            isBouncing = false;
            rect.anchoredPosition = landedPosition;
            return;
        }

        float offsetY = bounceHeight * Mathf.Sin(t * Mathf.PI); // rises then comes back down, once
        rect.anchoredPosition = landedPosition + new Vector2(0f, offsetY);
    }

    // Moves the object toward the nearest point on/in the drop area until it enters it, then stops.
    private void MoveBackToArea()
    {
        if (dropArea == null) return;

        if (OverlapsArea())
        {
            isResting = true;
            OnDropped();
            return;
        }

        Vector2 worldTarget = NearestPointInArea(rect.position);
        Vector3 newWorldPos = Vector3.MoveTowards(rect.position, worldTarget, returnSpeed * Time.deltaTime);
        rect.position = newWorldPos;
    }

    // Clamps a world-space point onto/inside the drop area's rectangle (its nearest edge/interior point).
    private Vector2 NearestPointInArea(Vector2 worldPoint)
    {
        Rect areaRect = WorldRect(dropArea);
        float x = Mathf.Clamp(worldPoint.x, areaRect.xMin, areaRect.xMax);
        float y = Mathf.Clamp(worldPoint.y, areaRect.yMin, areaRect.yMax);
        return new Vector2(x, y);
    }

    // Simple AABB overlap test between this object and the drop area, in world/canvas space.
    private bool OverlapsArea()
    {
        Rect objRect = WorldRect(rect);
        Rect areaRect = WorldRect(dropArea);
        return objRect.Overlaps(areaRect);
    }

    private Rect WorldRect(RectTransform target)
    {
        Vector3[] c = new Vector3[4];
        target.GetWorldCorners(c); // 0=bottom-left,1=top-left,2=top-right,3=bottom-right
        return new Rect(c[0].x, c[0].y, c[2].x - c[0].x, c[2].y - c[0].y);
    }

    // Called the moment the object lands/settles. Hook your own logic here.
    // NOTE: no SetAsLastSibling()/SetSiblingIndex() here on purpose — this object's
    // draw order stays exactly wherever you placed it in the Hierarchy, always.
    private void OnDropped()
    {
        landedPosition = rect.anchoredPosition;
        isBouncing = true;
        bounceTimer = 0f;
        Debug.Log(gameObject.name + " placed in the drop area!");
    }

    // --- Pointer / drag events ---

    public void OnPointerDown(PointerEventData eventData)
    {
        isHeld = true;
        isResting = false;
        isBouncing = false;
        targetScale = originalScale * heldScaleMultiplier;
        // No sibling reordering here either — see note on OnDropped().
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isHeld) return;

        float scaleFactor = (canvas != null) ? canvas.scaleFactor : 1f;
        rect.anchoredPosition += eventData.delta / scaleFactor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isHeld = false;
        targetScale = originalScale;

        // If it's already overlapping the drop area wherever you released it, it just stays there.
        // Otherwise it will smoothly travel back to the area (handled in Update -> MoveBackToArea).
        bool landedNow = dropArea != null && OverlapsArea();
        isResting = landedNow;

        if (landedNow)
        {
            OnDropped();
        }
    }
}