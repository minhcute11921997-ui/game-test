using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class GrassZoneManager : MonoBehaviour
{
    [Header("Zone Info")]
    public string zoneID;

    [Header("Thing Pool")]
    public List<ThingData> thingPool = new List<ThingData>();

    [Header("Spawn Config")]
    public LayerMask grassLayer;
    public LayerMask blockLayer;
    public int maxThings = 2;
    public float minSpawnDelay = 3f;
    public float maxSpawnDelay = 5f;

    private int currentActiveThings = 0;
    private bool isPlayerInside = false;

    private Collider2D zoneCollider;

    void Awake()
    {
        zoneCollider = GetComponent<Collider2D>();
    }

    void Start()
    {
        StartCoroutine(SpawnHeartbeat());
    }

    IEnumerator SpawnHeartbeat()
    {
        while (true)
        {
            if (isPlayerInside &&
                currentActiveThings < maxThings &&
                thingPool.Count > 0)
            {
                float wait = Random.Range(minSpawnDelay, maxSpawnDelay);
                yield return new WaitForSeconds(wait);

                if (isPlayerInside &&
                    currentActiveThings < maxThings)
                {
                    SpawnThing();
                }
            }

            yield return new WaitForSeconds(1f);
        }
    }

    void SpawnThing()
    {
        if (!TryFindSpawnPoint(out Vector2 spawnPos))
            return;

        ThingData selected = GetWeightedRandomThing();
        if (selected == null || selected.prefab == null)
            return;

        GameObject go = Instantiate(selected.prefab, spawnPos, Quaternion.identity);

        ShadowRoaming roaming = go.GetComponent<ShadowRoaming>();
        if (roaming != null)
        {
            roaming.parentManager = this;
            roaming.myData = selected;
        }

        currentActiveThings++;
    }

    public void OnThingRemoved()
    {
        currentActiveThings = Mathf.Max(0, currentActiveThings - 1);
    }

    bool TryFindSpawnPoint(out Vector2 result)
    {
        Bounds b = zoneCollider.bounds;

        for (int i = 0; i < 30; i++)
        {
            Vector2 point = new Vector2(
                Random.Range(b.min.x, b.max.x),
                Random.Range(b.min.y, b.max.y)
            );

            // phải nằm thật trong collider zone
            if (!zoneCollider.OverlapPoint(point))
                continue;

            // phải là ground cỏ
            if (!Physics2D.OverlapCircle(point, 0.1f, grassLayer))
                continue;

            // không đè monster khác
            if (Physics2D.OverlapCircle(point, 0.5f, LayerMask.GetMask("Monster")))
                continue;

            // không đè vật cản
            if (Physics2D.OverlapCircle(point, 0.4f, blockLayer))
                continue;

            result = point;
            return true;
        }

        result = Vector2.zero;
        return false;
    }

    ThingData GetWeightedRandomThing()
    {
        if (thingPool.Count == 0) return null;

        float total = 0f;

        foreach (var t in thingPool)
            total += t.spawnWeight;

        float roll = Random.Range(0f, total);
        float sum = 0f;

        foreach (var t in thingPool)
        {
            sum += t.spawnWeight;
            if (roll <= sum)
                return t;
        }

        return thingPool[0];
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isPlayerInside = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isPlayerInside = false;
    }
}