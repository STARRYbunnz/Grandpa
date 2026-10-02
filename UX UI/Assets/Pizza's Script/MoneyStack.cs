using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class MoneyStack : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public enum Denomination
    {
        Ten = 10,
        Twenty = 20,
        Fifty = 50,
        Hundred = 100,
        Thousand = 1000
    }

    public Denomination denomination = Denomination.Ten;
    public bool smoothDrag = true;
    public float dragFollowSpeed = 20f;

    public int Value => (int)denomination;
    public Vector3 OriginalScale { get; private set; }
    public DropZone CurrentZone { get; private set; }

    RectTransform rect;
    CanvasGroup group;
    Canvas canvas;

    Transform startParent;
    int startIndex;
    Vector2 startPos;
    Quaternion startRot;

    bool dragging;
    Vector2 target;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        group = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();

        startParent = rect.parent;
        startIndex = rect.GetSiblingIndex();
        startPos = rect.anchoredPosition;
        startRot = rect.localRotation;
        OriginalScale = rect.localScale;
    }

    public void OnBeginDrag(PointerEventData e)
    {
        dragging = true;
        target = rect.anchoredPosition;

        if (CurrentZone)
        {
            CurrentZone.RemoveStack(this);
            CurrentZone = null;
        }

        rect.localRotation = Quaternion.identity;
        rect.SetAsLastSibling();
        group.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData e)
    {
        if (!dragging) return;

        float scale = canvas ? canvas.scaleFactor : 1f;
        target += e.delta / scale;

        if (!smoothDrag) rect.anchoredPosition = target;
    }

    void Update()
    {
        if (!dragging || !smoothDrag) return;

        float t = 1f - Mathf.Exp(-dragFollowSpeed * Time.deltaTime);
        rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, target, t);
    }

    public void OnEndDrag(PointerEventData e)
    {
        dragging = false;
        group.blocksRaycasts = true;

        var zone = DropZone.FindZoneAtScreenPoint(e.position, e.pressEventCamera);
        if (zone && zone.CanAccept(this))
        {
            zone.PlaceMoney(this);
            CurrentZone = zone;
            return;
        }

        rect.SetParent(startParent, false);
        rect.SetSiblingIndex(startIndex);
        rect.anchoredPosition = startPos;
        rect.localRotation = startRot;
        rect.localScale = OriginalScale;
        CurrentZone = null;
    }
}