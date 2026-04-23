using System.Collections.Generic;
using UnityEngine;

public class CommandPhaseController : MonoBehaviour
{
    public static CommandPhaseController Instance { get; private set; }

    private enum InputStep { SelectMove, SelectAttack }
    private InputStep _step = InputStep.SelectMove;

    private BattleEntity _selectedEntity;
    private GridPos _pendingMove;
    private List<GridPos> _validMoveCells = new();
    private List<GridPos> _validAttackCells = new();

    private List<BattleEntity> _playerEntities = new();
    private int _currentUnitIndex = 0;

    // Theo dõi ô chuột đang hover để cập nhật AoE preview
    private GridPos _lastHoverPos = new GridPos(-999, -999);
    

    void Awake() => Instance = this;

    // ── Public API ────────────────────────────────────────────────
    public void BeginInput()
    {
        _playerEntities = FindPlayerEntities();
        _currentUnitIndex = 0;
        SelectUnit(_playerEntities.Count > 0 ? _playerEntities[0] : null);
    }

    // ── Tìm entities của player ───────────────────────────────────
    List<BattleEntity> FindPlayerEntities()
    {
        var result = new List<BattleEntity>();
        foreach (var e in FindObjectsByType<BattleEntity>(FindObjectsInactive.Exclude))
            if (e.TeamId == 0) result.Add(e);
        return result;
    }

    // ── Chọn unit để input ────────────────────────────────────────
    void SelectUnit(BattleEntity entity)
    {
        if (entity == null) return;
        _selectedEntity = entity;
        _step = InputStep.SelectMove;
        _lastHoverPos = new GridPos(-999, -999);
        ShowMoveHighlight(entity.GridPos);
        Debug.Log($"[Command] Chọn {entity.name} tại {entity.GridPos} — Chọn ô di chuyển");
    }

    // ── Highlight di chuyển ───────────────────────────────────────
    void ShowMoveHighlight(GridPos origin)
    {
        _validMoveCells = GetReachableCells(origin, _selectedEntity.MoveRange, _selectedEntity.TeamId);
        BattleGridManager.Instance.ShowHighlight(_validMoveCells);
    }

    List<GridPos> GetReachableCells(GridPos origin, int range, int teamId)
    {
        var result = new List<GridPos>();
        var cfg = BattleGridManager.Instance.config;
        for (int dc = -range; dc <= range; dc++)
            for (int dr = -range; dr <= range; dr++)
            {
                int c = origin.col + dc, r = origin.row + dr;
                if (!cfg.IsWalkable(c, r)) continue;
                if (cfg.GetTeam(c) != teamId) continue;
                var pos = new GridPos(c, r);
                if (BattleGridManager.Instance.IsOccupied(pos) && pos != origin) continue;
                result.Add(pos);
            }
        return result;
    }

    // ── Highlight tấn công (ô hợp lệ để nhắm) ───────────────────
    void ShowAttackHighlight(GridPos from)
    {
        _validAttackCells = GetAttackableCells(from, _selectedEntity.TeamId);
        BattleGridManager.Instance.ShowHighlight(_validAttackCells);
        _lastHoverPos = new GridPos(-999, -999); // reset để force refresh hover
    }

    List<GridPos> GetAttackableCells(GridPos from, int teamId)
    {
        var result = new List<GridPos>();
        var cfg = BattleGridManager.Instance.config;
        int colStart = teamId == 0 ? cfg.RightMinCol : 0;
        int colEnd = teamId == 0 ? cfg.TotalCols - 1 : cfg.LeftMaxCol;
        for (int c = colStart; c <= colEnd; c++)
            for (int r = 0; r < cfg.boardRows; r++)
                result.Add(new GridPos(c, r)); // highlight tất cả ô địch có thể nhắm
        return result;
    }

    // ── Update: xử lý input + hover ──────────────────────────────
    void Update()
    {
        if (BattlePhaseManager.Instance.CurrentPhase != BattlePhase.CommandPhase) return;

        GridPos hoverPos = GetMouseGridPos();

        // === HOVER: cập nhật AoE preview khi di chuột ===
        if (_step == InputStep.SelectAttack && !hoverPos.Equals(_lastHoverPos))
        {
            _lastHoverPos = hoverPos;
            UpdateAoEPreview(hoverPos);
        }

        if (!Input.GetMouseButtonDown(0)) return;

        switch (_step)
        {
            case InputStep.SelectMove: HandleMoveSelection(hoverPos); break;
            case InputStep.SelectAttack: HandleAttackSelection(hoverPos); break;
        }
    }

    // ── Hover AoE preview ─────────────────────────────────────────
    void UpdateAoEPreview(GridPos hoverPos)
    {
        MoveData move = _selectedEntity.GetMove();
        if (move == null) return;

        var grid = BattleGridManager.Instance;

        // Nếu chuột nằm trong vùng tấn công hợp lệ → hiện AoE shape
        if (_validAttackCells.Contains(hoverPos))
        {
            var aoeCells = grid.GetAoECells(hoverPos, move.shape, _pendingMove);
            grid.ShowAoEPreview(aoeCells);
        }
        else
        {
            // Chuột ra ngoài vùng → hiện lại highlight vùng tấn công bình thường
            grid.ShowHighlight(_validAttackCells);
        }
    }

    // ── Click: chọn ô di chuyển ───────────────────────────────────
    void HandleMoveSelection(GridPos clicked)
    {
        if (!_validMoveCells.Contains(clicked))
        {
            Debug.Log($"[Command] Ô {clicked} không hợp lệ để di chuyển");
            return;
        }
        _pendingMove = clicked;
        _step = InputStep.SelectAttack;
        ShowAttackHighlight(_pendingMove);
        Debug.Log($"[Command] Di chuyển đến {_pendingMove} — Chọn ô tấn công");
    }

    // ── Click: chọn ô tấn công ───────────────────────────────────
    void HandleAttackSelection(GridPos clicked)
    {
        BattleGridManager.Instance.ClearHighlight();

        BattleCommand cmd;
        if (_validAttackCells.Contains(clicked))
        {
            cmd = BattleCommand.MoveAndAttack(_pendingMove, clicked);
            Debug.Log($"[Command] Tấn công {clicked}");
        }
        else
        {
            cmd = BattleCommand.MoveOnly(_selectedEntity.GridPos, _pendingMove);
            Debug.Log($"[Command] Bỏ qua tấn công, chỉ di chuyển");
        }

        BattlePhaseManager.Instance.SubmitCommand(_selectedEntity, cmd);

        _currentUnitIndex++;
        if (_currentUnitIndex < _playerEntities.Count)
            SelectUnit(_playerEntities[_currentUnitIndex]);

        _currentUnitIndex++;
if (_currentUnitIndex < _playerEntities.Count)
    SelectUnit(_playerEntities[_currentUnitIndex]);
else
    SubmitEnemyCommand();
    }

    // ── Enemy AI (placeholder) ────────────────────────────────────
    void SubmitEnemyCommand()
{
    var players = new List<BattleEntity>();
    foreach (var e in FindObjectsByType<BattleEntity>(FindObjectsInactive.Exclude))
        if (e.TeamId == 0) players.Add(e);

    foreach (var enemy in FindObjectsByType<BattleEntity>(FindObjectsInactive.Exclude))
    {
        if (enemy.TeamId != 1) continue;
        var cmd = EnemyAIBrain.Decide(enemy, players);
        BattlePhaseManager.Instance.SubmitCommand(enemy, cmd);
    }
}

BattleCommand BuildEnemyCommand(BattleEntity enemy, List<BattleEntity> players, AIPersonality personality)
{
    if (players.Count == 0)
        return BattleCommand.MoveOnly(enemy.GridPos, enemy.GridPos);

    switch (personality)
    {
        case AIPersonality.Aggressive:  return AI_Aggressive(enemy, players);
        case AIPersonality.Defensive:   return AI_Defensive(enemy, players);
        case AIPersonality.Random:      return AI_Random(enemy, players);
        default:                        return AI_Aggressive(enemy, players);
    }
}

// ── Tấn Công: tiến gần nhất, ưu tiên target HP thấp nhất ────────
BattleCommand AI_Aggressive(BattleEntity enemy, List<BattleEntity> players)
{
    // Target: player HP thấp nhất
    BattleEntity target = null;
    foreach (var p in players)
        if (target == null || p.CurrentHp < target.CurrentHp) target = p;

    // Di chuyển: ô gần target nhất
    var moveCells = GetReachableCells(enemy.GridPos, enemy.MoveRange, enemy.TeamId);
    GridPos bestMove = enemy.GridPos;
    int bestDist = int.MaxValue;
    foreach (var cell in moveCells)
    {
        int dist = Mathf.Abs(cell.col - target.GridPos.col) + Mathf.Abs(cell.row - target.GridPos.row);
        if (dist < bestDist) { bestDist = dist; bestMove = cell; }
    }

    var attackCells = GetAttackableCells(bestMove, enemy.TeamId);
    return attackCells.Contains(target.GridPos)
        ? BattleCommand.MoveAndAttack(bestMove, target.GridPos)
        : BattleCommand.MoveOnly(enemy.GridPos, bestMove);
}

// ── Phòng Thủ: lùi xa nhất khỏi player, chỉ tấn công nếu bắt buộc ──
BattleCommand AI_Defensive(BattleEntity enemy, List<BattleEntity> players)
{
    // Target: player gần nhất (để tránh)
    BattleEntity nearest = null;
    int nearestDist = int.MaxValue;
    foreach (var p in players)
    {
        int dist = Mathf.Abs(p.GridPos.col - enemy.GridPos.col) + Mathf.Abs(p.GridPos.row - enemy.GridPos.row);
        if (dist < nearestDist) { nearestDist = dist; nearest = p; }
    }

    // Di chuyển: ô xa nearest nhất
    var moveCells = GetReachableCells(enemy.GridPos, enemy.MoveRange, enemy.TeamId);
    GridPos bestMove = enemy.GridPos;
    int bestDist = 0;
    foreach (var cell in moveCells)
    {
        int dist = Mathf.Abs(cell.col - nearest.GridPos.col) + Mathf.Abs(cell.row - nearest.GridPos.row);
        if (dist > bestDist) { bestDist = dist; bestMove = cell; }
    }

    // Tấn công nếu có thể (target HP thấp nhất trong tầm)
    var attackCells = GetAttackableCells(bestMove, enemy.TeamId);
    BattleEntity attackTarget = null;
    foreach (var p in players)
        if (attackCells.Contains(p.GridPos))
            if (attackTarget == null || p.CurrentHp < attackTarget.CurrentHp) attackTarget = p;

    return attackTarget != null
        ? BattleCommand.MoveAndAttack(bestMove, attackTarget.GridPos)
        : BattleCommand.MoveOnly(enemy.GridPos, bestMove);
}

// ── Ngẫu Nhiên: di chuyển và tấn công ngẫu nhiên ────────────────
BattleCommand AI_Random(BattleEntity enemy, List<BattleEntity> players)
{
    var moveCells = GetReachableCells(enemy.GridPos, enemy.MoveRange, enemy.TeamId);
    GridPos bestMove = moveCells.Count > 0
        ? moveCells[UnityEngine.Random.Range(0, moveCells.Count)]
        : enemy.GridPos;

    var attackCells = GetAttackableCells(bestMove, enemy.TeamId);
    // Lọc ô có player đứng
    var validTargets = new List<GridPos>();
    foreach (var p in players)
        if (attackCells.Contains(p.GridPos)) validTargets.Add(p.GridPos);

    return validTargets.Count > 0
        ? BattleCommand.MoveAndAttack(bestMove, validTargets[UnityEngine.Random.Range(0, validTargets.Count)])
        : BattleCommand.MoveOnly(enemy.GridPos, bestMove);
}
    // ── Helper: lấy GridPos từ vị trí chuột ─────────────────────
    GridPos GetMouseGridPos()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        worldPos.z = 0;
        return BattleGridManager.Instance.WorldToGrid(worldPos);
    }
}