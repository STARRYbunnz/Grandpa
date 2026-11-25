using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class DropZone : MonoBehaviour
{
    public static readonly List<DropZone> ActiveZones = new List<DropZone>();

    [Header("Accept filter")]
    [Tooltip("Tick to let this zone take only one kind of money (e.g. a cashier stack of 20s).")]
    public bool onlyAcceptDenomination = false;
    public MoneyStack.Denomination acceptedDenomination = MoneyStack.Denomination.Twenty;

    [Header("Drop detection")]
    [Tooltip("Extra pixels around this zone where a drop still counts. " +
             "Use 20-40 on small zones (like coin slots) so you don't have to drop exactly on them.")]
    public float dropTolerance = 0f;

    [Header("Tilt")]
    public float minTiltAngle = 6f;
    public float maxTiltAngle = 22f;

    [Header("Placement (UI pixels)")]
    public float minDistanceBetweenStacks = 40f;
    public int maxPlacementAttempts = 15;
    public int maxStacksInZone = 20;
    public float edgePadding = 20f;

    public System.Action<int> OnTotalValueChanged;

    public int TotalValue;
    public RectTransform Rect { get; internal set; }
    public static IEnumerable<DropZone> All => ActiveZones;

    internal string acceptId;
    public readonly List<MoneyStack> stacks = new List<MoneyStack>();

    void Awake() => Rect = GetComponent<RectTransform>();
    void OnEnable() => ActiveZones.Add(this);
    void OnDisable() => ActiveZones.Remove(this);

    static readonly Vector3[] zoneCorners = new Vector3[4];

    internal static bool FindZoneAtScreenPoint(Vector2 position, Camera pressEventCamera, out DropZone zone, MoneyStack stack = null)
    {
        zone = null;
        float bestDist = float.MaxValue;

        for (int i = ActiveZones.Count - 1; i >= 0; i--)
        {
            DropZone z = ActiveZones[i];
            if (stack != null && !z.CanAccept(stack)) continue;   

            float d = z.ScreenDistance(position, pressEventCamera);
            if (d > z.dropTolerance) continue;

            if (d < bestDist)
            {
                bestDist = d;
                zone = z;
            }
        }
        return zone != null;
    }

    float ScreenDistance(Vector2 p, Camera cam)
    {
        Rect.GetWorldCorners(zoneCorners);
        Vector2 min = RectTransformUtility.WorldToScreenPoint(cam, zoneCorners[0]);
        Vector2 max = RectTransformUtility.WorldToScreenPoint(cam, zoneCorners[2]);

        float dx = Mathf.Max(Mathf.Max(min.x - p.x, 0f), p.x - max.x);
        float dy = Mathf.Max(Mathf.Max(min.y - p.y, 0f), p.y - max.y);
        return Mathf.Sqrt(dx * dx + dy * dy);
    }

    internal bool CanAccept(MoneyStack moneyStack)
    {
        if (moneyStack == null || stacks.Count >= maxStacksInZone) return false;

        if (onlyAcceptDenomination && moneyStack.denomination != acceptedDenomination)
            return false;

        return true;
    }

    internal void PlaceMoney(MoneyStack moneyStack)
    {
        RectTransform stackRect = (RectTransform)moneyStack.transform;
        stackRect.SetParent(Rect, false);
        stackRect.localScale = moneyStack.OriginalScale;

        stackRect.anchoredPosition = GetRandomFreePosition(stackRect);

        float angle = UnityEngine.Random.Range(minTiltAngle, maxTiltAngle);
        if (UnityEngine.Random.value < 0.5f) angle = -angle;
        stackRect.localRotation = Quaternion.Euler(0f, 0f, angle);

        moneyStack.CurrentZone = this;

        stacks.Add(moneyStack);
        TotalValue += moneyStack.Value;
        OnTotalValueChanged?.Invoke(TotalValue);
    }

    internal void RemoveStack(MoneyStack moneyStack)
    {
        if (stacks.Remove(moneyStack))
        {
            TotalValue -= moneyStack.Value;
            OnTotalValueChanged?.Invoke(TotalValue);
        }
    }

    Vector2 GetRandomFreePosition(RectTransform stackRect)
    {
        Rect r = Rect.rect;
        Vector2 half = stackRect.rect.size * 0.5f;

        float minX = r.xMin + edgePadding + half.x;
        float maxX = r.xMax - edgePadding - half.x;
        float minY = r.yMin + edgePadding + half.y;
        float maxY = r.yMax - edgePadding - half.y;

        if (minX > maxX || minY > maxY) return r.center;

        Vector2 best = r.center;
        float bestDist = -1f;

        for (int i = 0; i < maxPlacementAttempts; i++)
        {
            Vector2 candidate = new Vector2(
                UnityEngine.Random.Range(minX, maxX),
                UnityEngine.Random.Range(minY, maxY));

            float nearest = float.MaxValue;
            foreach (MoneyStack s in stacks)
            {
                float d = Vector2.Distance(candidate, ((RectTransform)s.transform).anchoredPosition);
                if (d < nearest) nearest = d;
            }

            if (nearest >= minDistanceBetweenStacks) return candidate;

            if (nearest > bestDist)
            {
                bestDist = nearest;
                best = candidate;
            }
        }
        return best;
    }
}