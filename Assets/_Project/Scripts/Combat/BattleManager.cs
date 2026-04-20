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
        SpawnEntity(grid, player, teamId: 0, col: 5, row: 4);
        SpawnEntity(grid, enemy, teamId: 1, col: 12, row: 4);

        // TEST: show movable range của player ngay khi vào battle
        var playerEntity = BattleGridManager.Instance.GetEntityAt(new GridPos(5, 4));
        if (playerEntity != null)
            BattleGridManager.Instance.ShowMovableRange(playerEntity, playerEntity.MoveRange);
    }

    void SpawnEntity(BattleGridManager grid, ThingData data, int teamId, int col, int row)
    {
        GameObject prefab = data.battlePrefab != null ? data.battlePrefab : fallbackPrefab;
        if (prefab == null) return;

        var go = Instantiate(prefab);
        var entity = go.GetComponent<BattleEntity>() ?? go.AddComponent<BattleEntity>();
        entity.Init(data, teamId);
        grid.PlaceEntity(entity, new GridPos(col, row));
    }
}