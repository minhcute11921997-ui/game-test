using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
public class TerrainCell
{
    public GridPos pos;
    public TerrainEffectType effectType;
    public int turnsLeft;
    public int maxCount;       // giới hạn ô của chiêu này
    public ElementType element;
    public bool isNewThisTurn = true;
}

public class TerrainManager : MonoBehaviour
{
    public static TerrainManager Instance;

    // Key = GridPos, Value = TerrainCell đang chiếm ô đó
    private readonly Dictionary<GridPos, TerrainCell> _cells = new();

    // Lưu danh sách ô theo từng loại để dễ kiểm tra giới hạn
    private readonly Dictionary<TerrainEffectType, List<GridPos>> _byType = new();
    private readonly HashSet<BattleEntity> _enteredThisTurn = new();
    void RefreshTerrainVisual(GridPos pos, TerrainEffectType type)
    {
        var grid = BattleGridManager.Instance;
        if (grid.tilemapTerrain == null) return;

        TileBase tile = type switch
        {
            TerrainEffectType.BurnMark => grid.tileTerrainBurn,
            TerrainEffectType.ThornTrap => grid.tileTerrainThorn,
            _ => null
        };
        if (tile == null) return;

        var cell = new Vector3Int(pos.col, pos.row, 0);
        grid.tilemapTerrain.SetTile(cell, tile);

        // Đặt màu có alpha để phân biệt với highlight
        Color c = type switch
        {
            TerrainEffectType.BurnMark => new Color(1f, 0.3f, 0f, 0.55f),   // cam đỏ
            TerrainEffectType.ThornTrap => new Color(0.1f, 0.7f, 0.1f, 0.55f), // xanh lá
            _ => Color.white
        };
        grid.tilemapTerrain.SetColor(cell, c);
    }

    void ClearTerrainVisual(GridPos pos)
    {
        var grid = BattleGridManager.Instance;
        if (grid.tilemapTerrain == null) return;
        grid.tilemapTerrain.SetTile(new Vector3Int(pos.col, pos.row, 0), null);
    }

    // Thêm method ClearAllTerrain (gọi từ ClearAll):
    public void ClearAll()
    {
        foreach (var pos in _cells.Keys)
            ClearTerrainVisual(pos);
        _cells.Clear();
        _byType.Clear();
    }
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }
    void OnDestroy() { if (Instance == this) Instance = null; }
    // ─── Đặt terrain ────────────────────────────────────────────────────────
    public void PlaceTerrain(GridPos pos, MoveData move)
    {
        var effectType = move.terrainEffect;
        int maxCount = move.terrainMaxCount;

        // Khởi danh sách nếu chưa có
        if (!_byType.ContainsKey(effectType))
            _byType[effectType] = new List<GridPos>();

        // Nếu đã đủ giới hạn → xóa ô cũ nhất của loại này
        if (_byType[effectType].Count >= maxCount)
        {
            GridPos oldest = _byType[effectType][0];
            RemoveTerrain(oldest);
        }

        // Nếu ô đã có terrain khác → xóa luôn (đè)
        if (_cells.ContainsKey(pos))
            RemoveTerrain(pos);

        // Đặt terrain mới
        var cell = new TerrainCell
        {
            pos = pos,
            effectType = effectType,
            turnsLeft = move.envDuration,
            maxCount = maxCount,
            element = move.elementType,
        };
        cell.isNewThisTurn = true;
        _cells[pos] = cell;
        RefreshTerrainVisual(pos, cell.effectType);
        _byType[effectType].Add(pos);

        Debug.Log($"[Terrain] Đặt {effectType} tại {pos}, còn {move.envDuration} lượt");
    }

    // ─── Xóa terrain ────────────────────────────────────────────────────────
    public void RemoveTerrain(GridPos pos)
    {
        if (!_cells.TryGetValue(pos, out var cell)) return;
        _byType[cell.effectType].Remove(pos);
        _cells.Remove(pos);
        ClearTerrainVisual(pos);
    }

    // ─── Lấy terrain tại ô ──────────────────────────────────────────────────
    public TerrainCell GetCell(GridPos pos)
        => _cells.TryGetValue(pos, out var cell) ? cell : null;

    public bool HasTerrain(GridPos pos) => _cells.ContainsKey(pos);

    // ─── Gọi khi entity BƯỚC VÀO ô ─────────────────────────────────────────
    public void OnEntityEnterCell(BattleEntity entity, GridPos pos)
    {
        var cell = GetCell(pos);
        if (cell == null) return;

        switch (cell.effectType)
        {
            case TerrainEffectType.BurnMark:
                _enteredThisTurn.Add(entity);
                int dmg = Mathf.Max(1, Mathf.FloorToInt(entity.MaxHp * 0.10f));
                // Áp tương khắc hệ Hỏa
                float typeMult = CombatCalculator.GetTypeMultiplier(ElementType.Fire, entity.Data.elementType);
                int finalDmg = Mathf.Max(1, Mathf.FloorToInt(dmg * typeMult));
                entity.TakeDamage(finalDmg);
                Debug.Log($"<color=yellow>[TerrainEnter DEBUG]</color> {entity.name} bước vào {pos} | " +
          $"HasTerrain={HasTerrain(pos)} | type={(GetCell(pos)?.effectType.ToString() ?? "NONE")}");
                break;
        }
    }

    // ─── Gọi cuối mỗi lượt ──────────────────────────────────────────────────
    public void OnTurnEnd(List<BattleEntity> allEntities)
    {

        Debug.Log($"<color=orange>[TerrainEnd]</color> Tổng ô terrain: {_cells.Count}");
        foreach (var kvp in _cells)
            Debug.Log($"  Ô {kvp.Key} | type={kvp.Value.effectType} | " +
                      $"turnsLeft={kvp.Value.turnsLeft} | isNew={kvp.Value.isNewThisTurn}");

        // 1. Xử lý hiệu ứng cuối lượt
        foreach (var entity in allEntities)
        {
            if (entity == null) continue; // guard phòng thủ

            var cell = GetCell(entity.GridPos);
            if (cell == null) continue;

            switch (cell.effectType)
            {
                case TerrainEffectType.ThornTrap:
                    if (entity.CanMove)
                    {
                        entity.LockMovementNextTurn();
                        Debug.Log($"[Terrain] {entity.name} bị Bẫy Gai, mất di chuyển lượt tới");
                    }
                    break;

                case TerrainEffectType.BurnMark:
                    if (!_enteredThisTurn.Contains(entity)) // ← chỉ damage nếu KHÔNG vừa bước vào
                    {
                        int dotDmg = Mathf.Max(1, Mathf.FloorToInt(entity.MaxHp * 0.10f));
                        float typeMult = CombatCalculator.GetTypeMultiplier(ElementType.Fire, entity.Data.elementType);
                        entity.TakeDamage(Mathf.Max(1, Mathf.FloorToInt(dotDmg * typeMult)));
                        Debug.Log($"[Terrain] {entity.name} nhận DoT từ Vết Cháy");
                    }
                    break;
            }
        }
        _enteredThisTurn.Clear();
        // 2. Giảm lượt tồn tại và xoá ô hết hạn
        var expired = new List<GridPos>();
        foreach (var kvp in _cells)
        {
            kvp.Value.turnsLeft--;
            // Bỏ qua đếm lượt nếu vừa đặt lượt này
            if (kvp.Value.isNewThisTurn)
            {
                kvp.Value.isNewThisTurn = false; // reset flag
                kvp.Value.turnsLeft++;           // hoàn lại 1 lượt
            }
            if (kvp.Value.turnsLeft <= 0)
                expired.Add(kvp.Key);
        }
        foreach (var pos in expired)
        {
            Debug.Log($"[Terrain] Ô {pos} hết hạn, xoá");
            RemoveTerrain(pos);
        }
    }

    public void OnTurnStart()
    {
        _enteredThisTurn.Clear();
    }
}