using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public GameObject fallbackPrefab;
    [SerializeField] private GameObject hpBarPrefab;

    void Start()
    {
        ThingData player = RuntimeGameState.ActiveThing;
        ThingData enemy = RuntimeGameState.CurrentEnemy;

        if (player == null) { Debug.LogWarning("[Battle] Không có player Thing!"); return; }
        if (enemy == null) { Debug.LogWarning("[Battle] Không có enemy Thing!"); return; }

        var grid = BattleGridManager.Instance;
        SpawnEntity(grid, player, teamId: 0, col: 3, row: 4);
        SpawnEntity(grid, enemy, teamId: 1, col: 14, row: 4);

        grid.RefreshAllFootprints();

        // Hiển thị footprint của tất cả entity sau khi spawn xong
        ShowAllFootprints();
    }

    void SpawnEntity(BattleGridManager grid, ThingData data, int teamId, int col, int row)
    {
        GameObject prefab = data.battlePrefab != null ? data.battlePrefab : fallbackPrefab;
        if (prefab == null) { Debug.LogError($"[Spawn] Không có prefab cho {data.thingName}!"); return; }

        var go = Instantiate(prefab);
        var entity = go.GetComponent<BattleEntity>() ?? go.AddComponent<BattleEntity>();
        entity.Init(data, teamId);
        grid.PlaceEntity(entity, new GridPos(col, row));

        if (hpBarPrefab != null)
        {
            var hpBarGo = Instantiate(hpBarPrefab);
            var hpBar = hpBarGo.GetComponent<EntityHpBar>();
            hpBar.Init(entity.transform);
            hpBar.SetHp(data.hp, data.hp);
            entity.hpBar = hpBar;
        }
        else
        {
            Debug.LogWarning("[HpBar] Prefab chưa có trong Resources/ — bỏ qua");
        }
    }

    void ShowAllFootprints()
    {
        var grid = BattleGridManager.Instance;

        // Màu team 0 (player) = xanh lá nhạt
        // Màu team 1 (enemy)  = đỏ nhạt
        Color colorPlayer = new Color(0.2f, 1f, 0.3f, 0.45f);
        Color colorEnemy = new Color(1f, 0.25f, 0.25f, 0.45f);

        // Dùng HashSet để dedup entity (footprint lớn có nhiều ô trong _occupied)
        var seen = new System.Collections.Generic.HashSet<BattleEntity>();

        foreach (var entity in grid.GetAllEntities())
        {
            if (!seen.Add(entity)) continue;

            Color c = entity.TeamId == 0 ? colorPlayer : colorEnemy;
            var cells = grid.GetFootprintCells(entity.GridPos, entity.Data.footprint);

            foreach (var cell in cells)
            {
                var tilePos = new UnityEngine.Vector3Int(cell.col, cell.row, 0);
                grid.tilemapHighlight.SetTile(tilePos, grid.tileHighlight);
                grid.tilemapHighlight.SetColor(tilePos, c);
            }
        }
    }
}