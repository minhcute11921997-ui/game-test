using UnityEngine;

public class BattleEnemyLoader : MonoBehaviour
{
    void Start()
    {
        if (RuntimeGameState.CurrentEnemy == null)
        {
            Debug.LogWarning("Không có dữ liệu quái.");
            return;
        }

        ThingData data = RuntimeGameState.CurrentEnemy;

        if (data.battlePrefab != null)
        {
            Instantiate(data.battlePrefab, transform.position, Quaternion.identity);
            Debug.Log("Spawn quái battle: " + data.thingName);
        }
        else
        {
            Debug.LogWarning("Thing chưa có battlePrefab.");
        }
    }
}