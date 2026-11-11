using UnityEngine;

public class UISpawner : MonoBehaviour
{
    [Header("What to spawn")]
    public RectTransform prefab;            

    [Header("Where to spawn")]
    public RectTransform spawnArea;         
    public RectTransform container;         

    [Header("Amount (random, inclusive)")]
    public int minSpawn = 1;
    public int maxSpawn = 5;

    [Header("Behaviour")]
    public bool spawnOnStart = true;
    public bool spawnOnlyOnce = true;       

    static readonly Vector3[] corners = new Vector3[4];
    bool hasSpawned;

    void Start()
    {
        if (!container)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas) container = canvas.transform as RectTransform;
        }

        if (!spawnArea) spawnArea = transform as RectTransform;

        if (spawnOnStart) Spawn();
    }

    
    public void Spawn()
    {
        if (!prefab || !container) return;
        if (spawnOnlyOnce && hasSpawned) return;
        hasSpawned = true;

        int lo = Mathf.Max(0, Mathf.Min(minSpawn, maxSpawn));
        int hi = Mathf.Max(minSpawn, maxSpawn);
        int count = Random.Range(lo, hi + 1);   

        for (int i = 0; i < count; i++)
        {
            RectTransform item = Instantiate(prefab, container, false);
            item.position = RandomPointInSpawnArea(item.position.z);
        }

        Debug.Log("Spawned " + count + " items");
    }

    Vector3 RandomPointInSpawnArea(float z)
    {
        spawnArea.GetWorldCorners(corners);
        float x = Random.Range(corners[0].x, corners[2].x);
        float y = Random.Range(corners[0].y, corners[2].y);
        return new Vector3(x, y, z);
    }
}