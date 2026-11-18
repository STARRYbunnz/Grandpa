using System.Collections.Generic;
using UnityEngine;

public class CustomerPayment : MonoBehaviour
{
    [Header("References")]
    public UISpawner candySpawner;    // spawns the candies
    public DropZone dropZone;         // the zone where the customer puts the money
    public RectTransform spawnParent; // optional: where money is created before going into the zone (default: Canvas)

    [Header("Money prefabs (1, 5, 10, 20, 50, 100)")]
    [Tooltip("Each prefab needs a MoneyStack component with its Value set.")]
    public List<MoneyStack> moneyPrefabs = new List<MoneyStack>();

    [Header("Price")]
    public int pricePerCandy = 20;

    [Header("Debug (read only)")]
    public int candyCount;
    public int totalPrice;
    public int moneyGiven;

    // True when the money in the drop zone is bigger than the price
    public bool IsPaymentEnough => dropZone != null && dropZone.TotalValue > totalPrice;

    void OnEnable()
    {
        if (candySpawner) candySpawner.OnSpawned += PutDownMoney;
        else Debug.LogError("CustomerPayment: Candy Spawner is not assigned.");
    }

    void OnDisable()
    {
        if (candySpawner) candySpawner.OnSpawned -= PutDownMoney;
    }

    void Start()
    {
        Debug.Log("CustomerPayment started. Waiting for candies to spawn...");

        // In case candies were already spawned before we subscribed
        if (candySpawner && candySpawner.LastSpawnCount > 0 && candyCount == 0)
            PutDownMoney(candySpawner.LastSpawnCount);
    }

    // Right-click the component title in the Inspector > "Test Put Down Money"
    // (use while the game is playing) to test without waiting for the spawner.
    [ContextMenu("Test Put Down Money")]
    void TestPutDownMoney()
    {
        PutDownMoney(Random.Range(1, 6));
    }

    void SortPrefabs()
    {
        moneyPrefabs.RemoveAll(m => m == null);
        moneyPrefabs.Sort((a, b) => a.Value.CompareTo(b.Value));
    }

    // Called automatically when candies are spawned
    public void PutDownMoney(int candies)
    {
        Debug.Log("CustomerPayment: " + candies + " candies spawned, putting down money...");
        SortPrefabs();
        if (!dropZone)
        {
            Debug.LogError("CustomerPayment: Drop Zone is not assigned.");
            return;
        }
        if (moneyPrefabs.Count == 0)
        {
            Debug.LogError("CustomerPayment: Money Prefabs list is empty (prefabs must have a MoneyStack component).");
            return;
        }

        candyCount = candies;
        totalPrice = candies * pricePerCandy;
        moneyGiven = 0;

        // 1) add random money while staying at or below the price
        while (true)
        {
            int remaining = totalPrice - moneyGiven;
            List<MoneyStack> fits = moneyPrefabs.FindAll(m => m.Value <= remaining);
            if (fits.Count == 0) break;

            // 50% chance take the biggest that fits (fewer items), else random
            MoneyStack pick = Random.value < 0.5f
                ? fits[fits.Count - 1]
                : fits[Random.Range(0, fits.Count)];

            PlaceInZone(pick);
            moneyGiven += pick.Value;
        }

        // 2) add the smallest money that pushes the total ABOVE the price
        foreach (MoneyStack m in moneyPrefabs)
        {
            if (moneyGiven + m.Value > totalPrice)
            {
                PlaceInZone(m);
                moneyGiven += m.Value;
                break;
            }
        }

        Debug.Log("Candies: " + candyCount + " | Price: " + totalPrice +
                  " | Money in zone: " + dropZone.TotalValue +
                  " | Change: " + (dropZone.TotalValue - totalPrice));
    }

    void PlaceInZone(MoneyStack prefab)
    {
        if (!dropZone.CanAccept(prefab)) return;   // zone is full

        // Spawn OUTSIDE the zone first: MoneyStack remembers its spawn parent
        // as its "return" parent if the player drags it out and drops it nowhere.
        Transform parent = spawnParent;
        if (!parent)
        {
            Canvas canvas = dropZone.GetComponentInParent<Canvas>();
            if (!canvas)
            {
                Debug.LogError("CustomerPayment: DropZone is not inside a Canvas.");
                return;
            }
            parent = canvas.transform;
        }
        MoneyStack stack = Instantiate(prefab, parent, false);

        dropZone.PlaceMoney(stack);                // moves it into the zone, random position, tilt, adds to TotalValue
    }
}