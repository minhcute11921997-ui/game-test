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
    public Tilemap tilemapGrid;

    [Header("Tiles — kéo vào từ Project")]
    public TileBase tileLeft;
    public TileBase tileRight;
    public TileBase tileGap;
    public TileBase tileHighlight;
    public TileBase tileGrid;



    private readonly Dictionary<GridPos, BattleEntity> _occupied = new();
    private readonly HashSet<GridPos> _highlightedCells = new();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (tileGrid == null) tileGrid = CreateGridTile();

        BuildGrid();
        FitCamera();
    }
    TileBase CreateGridTile()
    {
        // Tạo texture 16x16 với border 1px màu trắng mờ
        int size = 16;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        Color fill = new Color(1, 1, 1, 0f);    // trong suốt bên trong
        Color border = new Color(1, 1, 1, 0.25f); // viền trắng mờ 25%

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                bool isBorder = x == 0 || y == 0 || x == size - 1 || y == size - 1;
                tex.SetPixel(x, y, isBorder ? border : fill);
            }
        tex.Apply();

        var sprite = Sprite.Create(tex,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            size); // pixels per unit = size để khớp với cell

        var tile = ScriptableObject.CreateInstance<UnityEngine.Tilemaps.Tile>();
        tile.sprite = sprite;
        tile.color = Color.white;
        return tile;
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


                // ← THÊM: vẽ grid line lên trên tất cả ô (trừ gap)
                if (team != -1 && tilemapGrid != null && tileGrid != null)
                    tilemapGrid.SetTile(cell, tileGrid);
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
        entity.transform.position = GridToWorld(to);
    }
    public void ShowMovableRange(BattleEntity entity, int moveRange)
    {
        var cells = new List<GridPos>();
        GridPos origin = entity.GridPos;

        int minCol = entity.TeamId == 0 ? 0 : config.RightMinCol;
        int maxCol = entity.TeamId == 0 ? config.LeftMaxCol : config.TotalCols - 1;

        for (int dc = -moveRange; dc <= moveRange; dc++)
            for (int dr = -moveRange; dr <= moveRange; dr++)
            {
                if (Mathf.Abs(dc) + Mathf.Abs(dr) > moveRange) continue;
                int c = origin.col + dc;
                int r = origin.row + dr;
                if (!config.IsWalkable(c, r)) continue;
                if (c < minCol || c > maxCol) continue;
                if (c == origin.col && r == origin.row) continue;
                if (IsOccupied(new GridPos(c, r))) continue;
                cells.Add(new GridPos(c, r));
            }

        ShowHighlight(cells);
    }

    public void RemoveEntity(GridPos pos) => _occupied.Remove(pos);

    public void ShowHighlight(IEnumerable<GridPos> cells)
    {
        tilemapHighlight.ClearAllTiles();
        _highlightedCells.Clear();
        foreach (var p in cells)
        {
            tilemapHighlight.SetTile(new Vector3Int(p.col, p.row, 0), tileHighlight);
            _highlightedCells.Add(p);
        }
    }

    public void ClearHighlight()
    {
        tilemapHighlight.ClearAllTiles();
        _highlightedCells.Clear();
    }

    public bool IsHighlighted(GridPos pos) => _highlightedCells.Contains(pos);


    public System.Collections.IEnumerator MoveEntitySmooth(
    BattleEntity entity, GridPos to, System.Action onDone = null, float duration = 0.25f)
    {
        GridPos from = entity.GridPos;
        Vector3 startPos = entity.transform.position;
        Vector3 endPos = GridToWorld(to);

        // Update data ngay
        _occupied.Remove(from);
        _occupied[to] = entity;
        entity.GridPos = to;

        // Lerp visual
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            entity.transform.position = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }
        entity.transform.position = endPos;

        onDone?.Invoke();
    }

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

    public List<GridPos> GetAoECells(GridPos centerPos, AttackShape shape, GridPos attackerPos, int aoeRadius = 1)
    {
        var result = new List<GridPos>();
        var cfg = config;
        switch (shape)
        {
            case AttackShape.Single:
                result.Add(centerPos);
                break;

            case AttackShape.Cross:
                result.Add(centerPos); // tâm
                for (int i = 1; i <= aoeRadius; i++)  // mỗi hướng đi xa i ô
                {
                    result.Add(new GridPos(centerPos.col + i, centerPos.row)); // Đông
                    result.Add(new GridPos(centerPos.col - i, centerPos.row)); // Tây
                    result.Add(new GridPos(centerPos.col, centerPos.row + i)); // Bắc
                    result.Add(new GridPos(centerPos.col, centerPos.row - i)); // Nam
                }
                break;

            case AttackShape.Square2x2:
                // 4 ô: tâm, phải, trên, phải-trên
                for (int dc = 0; dc <= 1; dc++)
                    for (int dr = 0; dr <= 1; dr++)
                        result.Add(new GridPos(centerPos.col + dc, centerPos.row + dr));
                break;

            case AttackShape.Square3x3:
                for (int dc = -1; dc <= 1; dc++)
                    for (int dr = -1; dr <= 1; dr++)
                        result.Add(new GridPos(centerPos.col + dc, centerPos.row + dr));
                break;

            case AttackShape.Line:
                // Hướng từ attacker đến tâm, kéo dài đến hết bảng
                int dirCol = centerPos.col > attackerPos.col ? 1 :
                             centerPos.col < attackerPos.col ? -1 : 0;
                int dirRow = centerPos.row > attackerPos.row ? 1 :
                             centerPos.row < attackerPos.row ? -1 : 0;
                var cur = centerPos;
                for (int i = 0; i < 18; i++)
                {
                    if (!cfg.IsInBounds(cur.col, cur.row)) break;
                    result.Add(cur);
                    cur = new GridPos(cur.col + dirCol, cur.row + dirRow);
                }
                break;
        }

        // Lọc bỏ ô nằm ngoài bảng
        result.RemoveAll(p => !cfg.IsInBounds(p.col, p.row));
        return result;
    }

    /// <summary>Highlight AoE preview (màu riêng, không đè highlight di chuyển)</summary>
    public void ShowAoEPreview(List<GridPos> cells)
    {
        // Dùng lại tilemapHighlight nhưng với tileHighlight — đủ dùng cho preview
        // Sprint sau có thể tách tilemap riêng cho AoE
        tilemapHighlight.ClearAllTiles();
        foreach (var p in cells)
            tilemapHighlight.SetTile(new Vector3Int(p.col, p.row, 0), tileHighlight);
    }

public List<BattleEntity> GetAllEntities()
{
    return new List<BattleEntity>(_occupied.Values);
}



}