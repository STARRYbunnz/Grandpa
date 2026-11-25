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

    [Header("Dialogue")]
    [Tooltip("If ticked, money only appears after OnDialogueFinished() is called.")]
    public bool waitForDialogue = true;

    [Header("Debug (read only)")]
    public int candyCount;
    public int totalPrice;
    public int moneyGiven;

    // True once the customer has put the money down (until the next customer)
    public bool MoneyPutDown { get; private set; }
    public event System.Action OnMoneyPutDown;

    // The money stacks the customer put down (so we can tell them from the change we give)
    public readonly List<MoneyStack> customerStacks = new List<MoneyStack>();

    int pendingCandies = -1;      // candies spawned, money not put down yet
    bool dialogueFinished;

    // True when the money in the drop zone is bigger than the price
    public bool IsPaymentEnough => dropZone != null && dropZone.TotalValue > totalPrice;

    void OnEnable()
    {
        if (candySpawner) candySpawner.OnSpawned += OnCandiesSpawned;
        else Debug.LogError("CustomerPayment: Candy Spawner is not assigned.");
    }

    void OnDisable()
    {
        if (candySpawner) candySpawner.OnSpawned -= OnCandiesSpawned;
    }

    void Start()
    {
        // In case candies were already spawned before we subscribed
        if (candySpawner && candySpawner.LastSpawnCount > 0 && candyCount == 0)
            OnCandiesSpawned(candySpawner.LastSpawnCount);
    }

    // ---------------------------------------------------------------
    // Call this from your dialogue script when the customer finishes
    // speaking:   FindObjectOfType<CustomerPayment>().OnDialogueFinished();
    // (You can also hook it to a UnityEvent / Button in the Inspector.)
    // ---------------------------------------------------------------
    public void OnDialogueFinished()
    {
        dialogueFinished = true;

        if (pendingCandies > 0)
        {
            int candies = pendingCandies;
            pendingCandies = -1;
            dialogueFinished = false;      // reset for the next customer
            PutDownMoney(candies);
        }
    }

    // Call this when a NEW customer arrives, so the next dialogue is waited for
    public void ResetForNewCustomer()
    {
        pendingCandies = -1;
        dialogueFinished = false;
        MoneyPutDown = false;
    }

    // Candies just spawned
    void OnCandiesSpawned(int candies)
    {
        if (!waitForDialogue || dialogueFinished)
        {
            dialogueFinished = false;
            PutDownMoney(candies);
            return;
        }

        // remember how many candies, and wait for the dialogue to finish
        pendingCandies = candies;
        Debug.Log("CustomerPayment: " + candies + " candies spawned, waiting for dialogue to finish...");
    }

    // Right-click the component title in the Inspector > "Test Put Down Money"
    // (use while the game is playing) to test without waiting for anything.
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

    public void PutDownMoney(int candies)
    {
        Debug.Log("CustomerPayment: " + candies + " candies, putting down money...");
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

        MoneyPutDown = false;
        customerStacks.Clear();
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

        MoneyPutDown = true;
        OnMoneyPutDown?.Invoke();
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
        customerStacks.Add(stack);

        dropZone.PlaceMoney(stack);                // moves it into the zone, random position, tilt, adds to TotalValue
    }
}