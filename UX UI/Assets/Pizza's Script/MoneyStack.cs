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

    [Header("Note Value")]
    public Denomination denomination = Denomination.Ten;

   
    public int Value => (int)denomination;

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
    private Transform _originalParent;
    private int _originalSiblingIndex;

    private bool _isDragging;
    private Vector2 _dragTargetPosition;

   
    public DropZone CurrentZone { get; private set; }

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvas = GetComponentInParent<Canvas>();

        _originalAnchoredPosition = _rect.anchoredPosition;
        _originalRotation = _rect.localRotation;
        _originalParent = _rect.parent;
        _originalSiblingIndex = _rect.GetSiblingIndex();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _isDragging = true;
        _dragTargetPosition = _rect.anchoredPosition;

       
        if (CurrentZone != null)
        {
            CurrentZone.RemoveStack(this);
            CurrentZone = null;
        }

        _rect.localRotation = Quaternion.identity; 
        _rect.SetAsLastSibling();                  
        _canvasGroup.blocksRaycasts = false;       
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

    
    public void ReturnToCashier()
    {
        _rect.SetParent(_originalParent);
        _rect.SetSiblingIndex(_originalSiblingIndex);
        _rect.anchoredPosition = _originalAnchoredPosition;
        _rect.localRotation = _originalRotation;
        CurrentZone = null;
    }
}