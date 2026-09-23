using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach to the drop-zone UI element (an Image or plain panel with a
/// RectTransform works fine - it doesn't need to be raycast-targetable).
/// That's the only setup required: the zone registers itself automatically,
/// so MoneyStack doesn't need any manual reference to find it.
///
/// Places incoming money at a random, non-overlapping spot within the
/// zone's rect, with a random left/right tilt, so stacks never land
/// perfectly on top of each other.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class DropZone : MonoBehaviour
{
    // Every enabled DropZone registers here so MoneyStack can find "whatever
    // zone is under this point" without any manual wiring.
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

    /// <summary>Fires whenever the total cash value in this zone changes (e.g. to update a HUD).</summary>
    public event System.Action<int> OnTotalValueChanged;

    /// <summary>Running total value of every note currently placed in this zone.</summary>
    public int TotalValue { get; private set; }

    private RectTransform _rect;
    private readonly List<MoneyStack> _stacksInZone = new List<MoneyStack>();

    private void Awake() => _rect = GetComponent<RectTransform>();
    private void OnEnable() => ActiveZones.Add(this);
    private void OnDisable() => ActiveZones.Remove(this);

    /// <summary>Finds whichever registered DropZone contains this screen point, if any.</summary>
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

    /// <summary>Places the stack at a free spot inside the zone with a random tilt.</summary>
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

    /// <summary>Called by MoneyStack when it's picked back up from this zone.</summary>
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
        // Fallback if the zone is crowded: use whatever random point we get.
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
        // 50/50 left or right, magnitude randomized within range.
        float sign = Random.value < 0.5f ? -1f : 1f;
        float angle = sign * Random.Range(minTiltAngle, maxTiltAngle);
        return Quaternion.Euler(0f, 0f, angle);
    }
}