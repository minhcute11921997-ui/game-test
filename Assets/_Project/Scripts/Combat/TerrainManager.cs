using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TerrainCell
{
    public GridPos pos;
    public TerrainEffectType effectType;
    public int turnsLeft;
    public int maxCount;
    public ElementType element;
    public bool isNewThisTurn = true;
    public int ownerTeam; // ← THÊM: team đặt terrain này
}

public class TerrainManager : MonoBehaviour
{
    public static TerrainManager Instance;

    private readonly Dictionary<GridPos, TerrainCell> _cells = new();

    // ← SỬA: key = (type, teamId) để tính riêng từng team
    private readonly Dictionary<(TerrainEffectType, int), List<GridPos>> _byType = new();

    private readonly HashSet<BattleEntity> _enteredThisTurn = new();

    // ─── Visual helpers ─────────────────────────────────────────────────────
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

        Color c = type switch
        {
            TerrainEffectType.BurnMark => new Color(1f, 0.3f, 0f, 0.55f),
            TerrainEffectType.ThornTrap => new Color(0.1f, 0.7f, 0.1f, 0.55f),
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

    public void ClearAll()
    {
        foreach (var pos in _cells.Keys)
            ClearTerrainVisual(pos);
        _cells.Clear();
        _byType.Clear();
    }

    // ─── Lifecycle ──────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    // ─── Đặt terrain ────────────────────────────────────────────────────────
    public void PlaceTerrain(GridPos pos, MoveData move, int ownerTeam = 0)
    {
        var te = move.GetTerrain();
        if (te == null) return;

        var effectType = te.terrainType;
        int maxCount = te.maxCount;
        int duration = te.duration;

        // key riêng cho từng team
        var key = (effectType, ownerTeam);

        if (!_byType.ContainsKey(key))
            _byType[key] = new List<GridPos>();

        // Xóa ô cũ nhất của CÙNG TEAM nếu vượt maxCount
        if (_byType[key].Count >= maxCount)
        {
            GridPos oldest = _byType[key][0];
            RemoveTerrain(oldest);
        }

        if (_cells.ContainsKey(pos))
            RemoveTerrain(pos);

        var cell = new TerrainCell
        {
            pos = pos,
            effectType = effectType,
            turnsLeft = duration,
            maxCount = maxCount,
            element = move.elementType,
            isNewThisTurn = true,
            ownerTeam = ownerTeam  // ← lưu team
        };
        _cells[pos] = cell;
        RefreshTerrainVisual(pos, cell.effectType);
        _byType[key].Add(pos);

        Debug.Log($"[Terrain] Team {ownerTeam} đặt {effectType} tại {pos}, còn {duration} lượt");
    }

    // ─── Xóa terrain ────────────────────────────────────────────────────────
    public void RemoveTerrain(GridPos pos)
    {
        if (!_cells.TryGetValue(pos, out var cell)) return;

        var key = (cell.effectType, cell.ownerTeam);
        if (_byType.TryGetValue(key, out var list))
            list.Remove(pos);

        _cells.Remove(pos);
        ClearTerrainVisual(pos);
    }

    // ─── Getters ────────────────────────────────────────────────────────────
    public TerrainCell GetCell(GridPos pos)
        => _cells.TryGetValue(pos, out var cell) ? cell : null;

    public bool HasTerrain(GridPos pos) => _cells.ContainsKey(pos);

    public int CountByType(TerrainEffectType type, int teamId)
        => _byType.TryGetValue((type, teamId), out var list) ? list.Count : 0;

    // Overload cũ — tổng 2 team (dùng nếu cần)
    public int CountByType(TerrainEffectType type)
        => CountByType(type, 0) + CountByType(type, 1);

    // ─── Gọi khi entity bước vào ô ─────────────────────────────────────────
    public void OnEntityEnterCell(BattleEntity entity, GridPos pos)
    {
        var cell = GetCell(pos);
        if (cell == null) return;

        switch (cell.effectType)
        {
            case TerrainEffectType.BurnMark:
                _enteredThisTurn.Add(entity);
                int dmg = Mathf.Max(1, Mathf.FloorToInt(entity.MaxHp * 0.10f));
                float typeMult = CombatCalculator.GetTypeMultiplier(ElementType.Fire, entity.Data.elementType);
                int finalDmg = Mathf.Max(1, Mathf.FloorToInt(dmg * typeMult));
                entity.TakeDamage(finalDmg);
                Debug.Log($"<color=yellow>[TerrainEnter]</color> {entity.name} bước vào {pos} | type={cell.effectType} | dmg={finalDmg}");
                break;
        }
    }

    // ─── Cuối lượt ──────────────────────────────────────────────────────────
    public void OnTurnEnd(List<BattleEntity> allEntities)
    {
        Debug.Log($"<color=orange>[TerrainEnd]</color> Tổng ô terrain: {_cells.Count}");
        foreach (var kvp in _cells)
            Debug.Log($"  Ô {kvp.Key} | type={kvp.Value.effectType} | team={kvp.Value.ownerTeam} | turnsLeft={kvp.Value.turnsLeft}");

        foreach (var entity in allEntities)
        {
            if (entity == null) continue;
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
                    if (!_enteredThisTurn.Contains(entity))
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

        var expired = new List<GridPos>();
        foreach (var kvp in _cells)
        {
            kvp.Value.turnsLeft--;
            if (kvp.Value.isNewThisTurn)
            {
                kvp.Value.isNewThisTurn = false;
                kvp.Value.turnsLeft++;
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