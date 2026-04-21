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
        GameObject prefab = data.battlePrefab != null ? data.battlePrefab : fallbackPrefab;
        if (prefab == null) return;

        // 1. Tạo Entity
        var go = Instantiate(prefab);
        var entity = go.GetComponent<BattleEntity>() ?? go.AddComponent<BattleEntity>();
        entity.Init(data, teamId);
        grid.PlaceEntity(entity, new GridPos(col, row));

        // 2. Khởi tạo HP Bar ngay trong hàm này
        var hpBarGo = Instantiate(Resources.Load<GameObject>("HpBar"));
        var hpBar = hpBarGo.GetComponent<EntityHpBar>();

        // Lưu ý: Đảm bảo class BattleEntity có property Data hoặc bạn có thể dùng trực tiếp data.hp
        hpBar.Init(entity.transform, entity.Data.hp);
        entity.hpBar = hpBar;
    }
}