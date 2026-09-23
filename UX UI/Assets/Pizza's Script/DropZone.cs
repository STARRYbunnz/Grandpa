using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(RectTransform))]
public class DropZone : MonoBehaviour
{
   
    private static readonly List<DropZone> ActiveZones = new List<DropZone>();

    [Header("Tilt Settings")]
    public float minTiltAngle = 6f;
    public float maxTiltAngle = 22f;

    [Header("Placement Settings (values in UI pixels)")]
    public float minDistanceBetweenStacks = 40f;
    public int maxPlacementAttempts = 15;
    public int maxStacksInZone = 20;
    [Tooltip("Keeps stacks from spawning too close to the zone's edges.")]
    public float edgePadding = 20f;

    
    public event System.Action<int> OnTotalValueChanged;

   
    public int TotalValue { get; private set; }

    private RectTransform _rect;
    private readonly List<MoneyStack> _stacksInZone = new List<MoneyStack>();

    private void Awake() => _rect = GetComponent<RectTransform>();
    private void OnEnable() => ActiveZones.Add(this);
    private void OnDisable() => ActiveZones.Remove(this);

    
    public static DropZone FindZoneAtScreenPoint(Vector2 screenPoint, Camera cam)
    {
        foreach (var zone in ActiveZones)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(zone._rect, screenPoint, cam))
                return zone;
        }
        return null;
    }

    public bool CanAccept(MoneyStack stack) => _stacksInZone.Count < maxStacksInZone;

    
    public void PlaceMoney(MoneyStack stack)
    {
        RectTransform stackRect = stack.GetComponent<RectTransform>();
        Vector2 point = FindFreeLocalPosition();

        stackRect.SetParent(_rect, false);
        stackRect.anchoredPosition = point;
        stackRect.localRotation = GetRandomTiltRotation();

        _stacksInZone.Add(stack);
        TotalValue += stack.Value;
        OnTotalValueChanged?.Invoke(TotalValue);
    }

    
    public void RemoveStack(MoneyStack stack)
    {
        if (_stacksInZone.Remove(stack))
        {
            TotalValue -= stack.Value;
            OnTotalValueChanged?.Invoke(TotalValue);
        }
    }

    private Vector2 FindFreeLocalPosition()
    {
        for (int i = 0; i < maxPlacementAttempts; i++)
        {
            Vector2 candidate = GetRandomLocalPoint();
            if (IsFarEnoughFromOthers(candidate))
                return candidate;
        }
        
        return GetRandomLocalPoint();
    }

    private Vector2 GetRandomLocalPoint()
    {
        Rect r = _rect.rect;
        float halfX = Mathf.Max(0f, r.width * 0.5f - edgePadding);
        float halfY = Mathf.Max(0f, r.height * 0.5f - edgePadding);
        return new Vector2(Random.Range(-halfX, halfX), Random.Range(-halfY, halfY));
    }

    private bool IsFarEnoughFromOthers(Vector2 point)
    {
        foreach (var s in _stacksInZone)
        {
            if (s == null) continue;
            RectTransform sr = s.GetComponent<RectTransform>();
            if (Vector2.Distance(sr.anchoredPosition, point) < minDistanceBetweenStacks)
                return false;
        }
        return true;
    }

    private Quaternion GetRandomTiltRotation()
    {
       
        float sign = Random.value < 0.5f ? -1f : 1f;
        float angle = sign * Random.Range(minTiltAngle, maxTiltAngle);
        return Quaternion.Euler(0f, 0f, angle);
    }
}