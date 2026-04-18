using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GrassZoneManager : MonoBehaviour
{
    [Header("Dữ liệu Things")]
    public string zoneID;
    public List<ThingData> thingPool; // Đây là phần Setup bạn đang tìm

    [Header("Cấu hình vùng cỏ")]
    public LayerMask grassLayer;    // Để chọn Layer "Grass"
    public float spawnRadius = 5f;  // Bán kính xuất hiện
    public int maxThings = 2;       // Giới hạn 1-2 con

    private int currentActiveThings = 0;
    private bool isPlayerInside = false;

    void Start()
    {
        StartCoroutine(SpawnHeartbeat());
    }

    IEnumerator SpawnHeartbeat()
    {
        while (true)
        {
            if (isPlayerInside && currentActiveThings < maxThings)
            {
                // Đợi ngẫu nhiên 20-40 giây như thống nhất
                float waitTime = Random.Range(1f, 5f);
                yield return new WaitForSeconds(waitTime);

                if (isPlayerInside && currentActiveThings < maxThings)
                {
                    SpawnThing();
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }

    void SpawnThing()
    {
        Vector2 spawnPos = FindRandomGrassPosition();
        Debug.Log("Đang thử tìm vị trí cỏ...");
        if (spawnPos != Vector2.zero)
        {
            ThingData selected = GetWeightedRandomThing();
            GameObject go = Instantiate(selected.prefab, spawnPos, Quaternion.identity);

            ShadowRoaming script = go.GetComponent<ShadowRoaming>();
            script.parentManager = this;
            script.myData = selected;

            currentActiveThings++;
        }
    }

    // Hàm này phải trùng tên với lời gọi trong ShadowRoaming
    public void OnThingRemoved()
    {
        currentActiveThings--;
    }

    Vector2 FindRandomGrassPosition()
    {
        for (int i = 0; i < 20; i++)
        {
            Vector2 randomPoint = (Vector2)transform.position + Random.insideUnitCircle * spawnRadius;
            if (Physics2D.OverlapCircle(randomPoint, 0.1f, grassLayer))
            {
                if (!Physics2D.OverlapCircle(randomPoint, 0.4f, LayerMask.GetMask("Monster")))
                    return randomPoint;
            }
        }
        return Vector2.zero;
    }

    ThingData GetWeightedRandomThing()
    {
        float totalWeight = 0;
        // Đảm bảo trong ThingData bạn đặt tên là spawnWeight
        foreach (var t in thingPool) totalWeight += t.spawnWeight;

        float roll = Random.Range(0, totalWeight);
        float s = 0;
        foreach (var t in thingPool)
        {
            s += t.spawnWeight;
            if (roll <= s) return t;
        }
        return thingPool[0];
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            Debug.Log("<color=green>Đã vào cỏ! Bắt đầu đếm 20-40 giây...</color>");
        }
    }
    private void OnTriggerExit2D(Collider2D other) { if (other.CompareTag("Player")) isPlayerInside = false; }
}