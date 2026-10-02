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
    public static IEnumerable<DropZone> All { get; internal set; }

    internal string acceptId;
    public readonly List<MoneyStack> stacks = new List<MoneyStack>();

    void Awake() => Rect = GetComponent<RectTransform>();
    void OnEnable() => ActiveZones.Add(this);
    void OnDisable() => ActiveZones.Remove(this);

    internal void ClearOccupant(DraggableItem draggableItem)
    {
        throw new System.NotImplementedException();
    }

    internal float GetTopEdgeInParentSpace(RectTransform rectTransform, Camera camera)
    {
        throw new System.NotImplementedException();
    }
}