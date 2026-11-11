using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class DropZone : MonoBehaviour
{
    public static readonly List<DropZone> ActiveZones = new List<DropZone>();

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

    // Find the topmost zone under the screen point
    internal static bool FindZoneAtScreenPoint(Vector2 position, Camera pressEventCamera, out DropZone zone)
    {
        zone = null;
        for (int i = ActiveZones.Count - 1; i >= 0; i--)
        {
            DropZone z = ActiveZones[i];
            if (RectTransformUtility.RectangleContainsScreenPoint(z.Rect, position, pressEventCamera))
            {
                zone = z;
                return true;
            }
        }
        return false;
    }

    internal bool CanAccept(MoneyStack moneyStack)
    {
        return moneyStack != null && stacks.Count < maxStacksInZone;
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

        // Zone too small for padding: fall back to center
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

            // Keep the best attempt in case none are far enough
            if (nearest > bestDist)
            {
                bestDist = nearest;
                best = candidate;
            }
        }
        return best;
    }
}