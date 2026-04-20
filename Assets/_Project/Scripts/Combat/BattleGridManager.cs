using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BattleGridManager : MonoBehaviour
{
    public static BattleGridManager Instance { get; private set; }

    [Header("Cấu hình")]
    public BattleGridConfig config;

    [Header("Tilemaps — kéo vào từ Hierarchy")]
    public Tilemap tilemapLeft;
    public Tilemap tilemapRight;
    public Tilemap tilemapGap;
    public Tilemap tilemapHighlight;

    [Header("Tiles — kéo vào từ Project")]
    public TileBase tileLeft;
    public TileBase tileRight;
    public TileBase tileGap;
    public TileBase tileHighlight;

    private readonly Dictionary<GridPos, BattleEntity> _occupied = new();

    void Awake()
    {
        Instance = this;
        BuildGrid();
        FitCamera();
    }

    void BuildGrid()
    {
        for (int col = 0; col < config.TotalCols; col++)
            for (int row = 0; row < config.boardRows; row++)
            {
                var cell = new Vector3Int(col, row, 0);
                int team = config.GetTeam(col);

                if (team == 0) tilemapLeft.SetTile(cell, tileLeft);
                else if (team == 1) tilemapRight.SetTile(cell, tileRight);
                else tilemapGap.SetTile(cell, tileGap);
            }
    }

    void FitCamera()
    {
        // Đọc cell size thực tế từ Grid thay vì hardcode 0.32
        Grid grid = tilemapLeft.layoutGrid;
        float cellW = grid.cellSize.x;
        float cellH = grid.cellSize.y;

        float gridWidth = config.TotalCols * cellW;
        float gridHeight = config.boardRows * cellH;

        Camera cam = Camera.main;
        if (cam == null) { Debug.LogError("Không tìm thấy Main Camera!"); return; }

        float vertSize = gridHeight / 2f;
        float horizSize = (gridWidth / 2f) / cam.aspect;
        cam.orthographicSize = Mathf.Max(vertSize, horizSize) * 1.1f; // +10% margin

        cam.transform.position = new Vector3(
            gridWidth / 2f,
            gridHeight / 2f,
            -10f
        );

        Debug.Log($"[FitCamera] cellW={cellW} gridW={gridWidth} gridH={gridHeight} size={cam.orthographicSize}");
    }

    public Vector3 GridToWorld(GridPos pos)
        => tilemapLeft.GetCellCenterWorld(new Vector3Int(pos.col, pos.row, 0));

    public GridPos WorldToGrid(Vector3 world)
    {
        var cell = tilemapLeft.WorldToCell(world);
        return new GridPos(cell.x, cell.y);
    }

    public bool IsOccupied(GridPos pos) => _occupied.ContainsKey(pos);

    public BattleEntity GetEntityAt(GridPos pos)
        => _occupied.TryGetValue(pos, out var e) ? e : null;

    public void PlaceEntity(BattleEntity entity, GridPos pos)
    {
        _occupied[pos] = entity;
        entity.GridPos = pos;
        entity.transform.position = GridToWorld(pos);
    }

    public void MoveEntity(BattleEntity entity, GridPos from, GridPos to)
    {
        _occupied.Remove(from);
        _occupied[to] = entity;
        entity.GridPos = to;
    }

    public void RemoveEntity(GridPos pos) => _occupied.Remove(pos);

    public void ShowHighlight(IEnumerable<GridPos> cells)
    {
        tilemapHighlight.ClearAllTiles();
        foreach (var p in cells)
            tilemapHighlight.SetTile(new Vector3Int(p.col, p.row, 0), tileHighlight);
    }

    public void ClearHighlight() => tilemapHighlight.ClearAllTiles();

    public List<GridPos> GetLine(GridPos origin, GridPos dir, int maxRange = 18)
    {
        var result = new List<GridPos>();
        var cur = origin.Step(dir);
        for (int i = 0; i < maxRange; i++)
        {
            if (!config.IsInBounds(cur.col, cur.row)) break;
            result.Add(cur);   // ← chỉ add 1 lần
            cur = cur.Step(dir);
        }
        return result;
    }
    public void ShowMovableRange(BattleEntity entity, int moveRange)
    {
        var cells = new List<GridPos>();
        GridPos origin = entity.GridPos; // ← đúng tên property

        int minCol = entity.TeamId == 0 ? 0 : 10;
        int maxCol = entity.TeamId == 0 ? 7 : 17;

        for (int dc = -moveRange; dc <= moveRange; dc++)
            for (int dr = -moveRange; dr <= moveRange; dr++)
            {
                if (Mathf.Abs(dc) + Mathf.Abs(dr) > moveRange) continue;
                int c = origin.col + dc;
                int r = origin.row + dr;
                if (c < minCol || c > maxCol || r < 0 || r > config.boardRows - 1) continue;
                if (c == origin.col && r == origin.row) continue;
                if (IsOccupied(new GridPos(c, r))) continue; // bỏ qua ô đã có entity

                cells.Add(new GridPos(c, r));
            }

    }
    }