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
}

public class TerrainManager : MonoBehaviour
{
    public static TerrainManager Instance;

    private readonly Dictionary<GridPos, TerrainCell> _cells = new();
    private readonly Dictionary<TerrainEffectType, List<GridPos>> _byType = new();
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
    public void PlaceTerrain(GridPos pos, MoveData move)
    {
        var effectType = move.terrainEffect;
        int maxCount = move.terrainMaxCount;

        if (!_byType.ContainsKey(effectType))
            _byType[effectType] = new List<GridPos>();

        if (_byType[effectType].Count >= maxCount)
        {
            GridPos oldest = _byType[effectType][0];
            RemoveTerrain(oldest);
        }

        if (_cells.ContainsKey(pos))
            RemoveTerrain(pos);

        var cell = new TerrainCell
        {
            pos = pos,
            effectType = effectType,
            turnsLeft = move.terrainDuration,   // ← dùng terrainDuration
            maxCount = maxCount,
            element = move.elementType,
        };
        cell.isNewThisTurn = true;
        _cells[pos] = cell;
        RefreshTerrainVisual(pos, cell.effectType);
        _byType[effectType].Add(pos);

        Debug.Log($"[Terrain] Đặt {effectType} tại {pos}, còn {move.terrainDuration} lượt");
    }

    // ─── Xóa terrain ────────────────────────────────────────────────────────
    public void RemoveTerrain(GridPos pos)
    {
        if (!_cells.TryGetValue(pos, out var cell)) return;
        _byType[cell.effectType].Remove(pos);
        _cells.Remove(pos);
        ClearTerrainVisual(pos);
    }

    // ─── Getters ────────────────────────────────────────────────────────────
    public TerrainCell GetCell(GridPos pos)
        => _cells.TryGetValue(pos, out var cell) ? cell : null;

    public bool HasTerrain(GridPos pos) => _cells.ContainsKey(pos);

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
            Debug.Log($"  Ô {kvp.Key} | type={kvp.Value.effectType} | turnsLeft={kvp.Value.turnsLeft} | isNew={kvp.Value.isNewThisTurn}");

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