using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public GameObject fallbackPrefab;

    void Start()
    {
        ThingData player = RuntimeGameState.ActiveThing;
        ThingData enemy = RuntimeGameState.CurrentEnemy;

        if (player == null) { Debug.LogWarning("[Battle] Không có player Thing!"); return; }
        if (enemy == null) { Debug.LogWarning("[Battle] Không có enemy Thing!"); return; }

        var grid = BattleGridManager.Instance;
        SpawnEntity(grid, player, teamId: 0, col: 3, row: 4);
        SpawnEntity(grid, enemy, teamId: 1, col: 14, row: 4);
    }

    void SpawnEntity(BattleGridManager grid, ThingData data, int teamId, int col, int row)
    {
        // THÊM 3 DÒNG NÀY ĐỂ DEBUG:
        Debug.Log($"[Spawn] data={data?.thingName} battlePrefab={data?.battlePrefab} fallback={fallbackPrefab}");

        GameObject prefab = data.battlePrefab != null ? data.battlePrefab : fallbackPrefab;

        Debug.Log($"[Spawn] prefab chọn = {prefab}");  // ← xem cái gì được chọn

        if (prefab == null)
        {
            Debug.LogError($"[Spawn] PREFAB NULL cho {data.thingName}!");
            return;
        }

        var go = Instantiate(prefab);
        var entity = go.GetComponent<BattleEntity>() ?? go.AddComponent<BattleEntity>();
        entity.Init(data, teamId);
        grid.PlaceEntity(entity, new GridPos(col, row));
    }
}