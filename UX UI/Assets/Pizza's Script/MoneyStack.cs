using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Attach to each money stack UI element (an Image, typically).
/// Handles picking the stack up from the cashier, dragging it smoothly
/// while held, dropping it into a DropZone, and returning it to the
/// cashier if picked up again from a zone and released elsewhere.
///
/// SETUP: put this on a UI GameObject (child of a Canvas) that has an
/// Image/RectTransform. No manual drop-zone reference needed - DropZones
/// register themselves automatically (see DropZone.cs).
/// </summary>
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class MoneyStack : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    /// <summary>
    /// Add more values here if you need other notes. Make one prefab
    /// variant per denomination with the matching sprite + this enum set,
    /// so "Value" always matches what the note looks like.
    /// </summary>
    public enum Denomination
    {
        Ten = 10,
        Twenty = 20,
        Fifty = 50,
        Hundred = 100,
        Thousand = 1000
    }

    [Header("Note Value")]
    public Denomination denomination = Denomination.Ten;

    /// <summary>Cash value of this note/stack.</summary>
    public int Value => (int)denomination;

    /// <summary>The note's correct, undistorted local scale (captured on Awake).</summary>
    public Vector3 OriginalScale { get; private set; }

    [Header("Drag Feel")]
    [Tooltip("If true, the stack smoothly glides to the cursor instead of snapping instantly.")]
    public bool smoothDrag = true;
    [Tooltip("Higher = snappier follow, lower = more floaty/laggy.")]
    public float dragFollowSpeed = 20f;

    private RectTransform _rect;
    private CanvasGroup _canvasGroup;
    private Canvas _canvas;

    private Vector2 _originalAnchoredPosition;
    private Quaternion _originalRotation;
    private Vector3 _originalScale;
    private Transform _originalParent;
    private int _originalSiblingIndex;

    private bool _isDragging;
    private Vector2 _dragTargetPosition;

    /// <summary>The DropZone this stack is currently placed in, if any.</summary>
    public DropZone CurrentZone { get; private set; }

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvas = GetComponentInParent<Canvas>();

        _originalAnchoredPosition = _rect.anchoredPosition;
        _originalRotation = _rect.localRotation;
        _originalScale = _rect.localScale;
        OriginalScale = _originalScale;
        _originalParent = _rect.parent;
        _originalSiblingIndex = _rect.GetSiblingIndex();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _isDragging = true;
        _dragTargetPosition = _rect.anchoredPosition;

        // Picking the stack back up out of a drop zone frees its spot there.
        if (CurrentZone != null)
        {
            CurrentZone.RemoveStack(this);
            CurrentZone = null;
        }

        _rect.localRotation = Quaternion.identity; // hold it straight while carrying
        _rect.SetAsLastSibling();                  // draw above other UI while dragging
        _canvasGroup.blocksRaycasts = false;        // let the drop-zone check see through this
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;

        // eventData.delta is already in screen pixels; divide by canvas scale
        // so movement feels 1:1 with the cursor regardless of UI scaling.
        float scaleFactor = _canvas != null ? _canvas.scaleFactor : 1f;
        _dragTargetPosition += eventData.delta / scaleFactor;

        if (!smoothDrag)
            _rect.anchoredPosition = _dragTargetPosition;
    }

    private void Update()
    {
        // Smoothly glide toward the cursor every frame while held.
        if (_isDragging && smoothDrag)
        {
            _rect.anchoredPosition = Vector2.Lerp(
                _rect.anchoredPosition,
                _dragTargetPosition,
                1f - Mathf.Exp(-dragFollowSpeed * Time.deltaTime));
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _isDragging = false;
        _canvasGroup.blocksRaycasts = true;

        DropZone zone = DropZone.FindZoneAtScreenPoint(eventData.position, eventData.pressEventCamera);

        if (zone != null && zone.CanAccept(this))
        {
            zone.PlaceMoney(this);
            CurrentZone = zone;
        }
        else
        {
            ReturnToCashier();
        }
    }

    /// <summary>Snaps the stack back to its original cashier position.</summary>
    public void ReturnToCashier()
    {
        // "false" stops SetParent from rescaling the object to preserve its
        // world-space size under the new parent - that rescale is what was
        // making the note look bigger/smaller after being reparented.
        _rect.SetParent(_originalParent, false);
        _rect.SetSiblingIndex(_originalSiblingIndex);
        _rect.anchoredPosition = _originalAnchoredPosition;
        _rect.localRotation = _originalRotation;
        _rect.localScale = _originalScale; // force the size back explicitly too, just in case
        CurrentZone = null;
    }
}