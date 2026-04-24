using System.Collections.Generic;
using UnityEngine;

public class CommandPhaseController : MonoBehaviour
{
    public static CommandPhaseController Instance { get; private set; }

    // ← THÊM SelectSkill vào enum
    private enum InputStep { SelectMove, SelectSkill, SelectAttack }
    private InputStep _step = InputStep.SelectMove;

    private BattleEntity _selectedEntity;
    private GridPos _pendingMove;
    private List<GridPos> _validMoveCells = new();
    private List<GridPos> _validAttackCells = new();

    private List<BattleEntity> _playerEntities = new();
    private int _currentUnitIndex = 0;

    private GridPos _lastHoverPos = new GridPos(-999, -999);

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void BeginInput()
    {
        _playerEntities = FindPlayerEntities();
        _currentUnitIndex = 0;
        SelectUnit(_playerEntities.Count > 0 ? _playerEntities[0] : null);
    }

    List<BattleEntity> FindPlayerEntities()
    {
        var result = new List<BattleEntity>();
        foreach (var e in FindObjectsByType<BattleEntity>(FindObjectsInactive.Exclude))
            if (e.TeamId == 0) result.Add(e);
        return result;
    }

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
                if (Mathf.Abs(dc) + Mathf.Abs(dr) > range) continue;
                int c = origin.col + dc, r = origin.row + dr;
                if (!cfg.IsWalkable(c, r)) continue;
                if (cfg.GetTeam(c) != teamId) continue;
                var pos = new GridPos(c, r);
                if (BattleGridManager.Instance.IsOccupied(pos) && pos != origin) continue;
                result.Add(pos);
            }
        return result;
    }

    // ── Highlight tấn công ───────────────────────────────────────
    void ShowAttackHighlight(GridPos from)
    {
        _validAttackCells = GetAttackableCells(from, _selectedEntity.TeamId);
        BattleGridManager.Instance.ShowHighlight(_validAttackCells);
        _lastHoverPos = new GridPos(-999, -999);
    }

    List<GridPos> GetAttackableCells(GridPos from, int teamId)
    {
        var result = new List<GridPos>();
        var cfg = BattleGridManager.Instance.config;
        int colStart = teamId == 0 ? cfg.RightMinCol : 0;
        int colEnd = teamId == 0 ? cfg.TotalCols - 1 : cfg.LeftMaxCol;
        for (int c = colStart; c <= colEnd; c++)
            for (int r = 0; r < cfg.boardRows; r++)
                result.Add(new GridPos(c, r));
        return result;
    }

    // ── Update ───────────────────────────────────────────────────
    void Update()
    {
        if (BattlePhaseManager.Instance.CurrentPhase != BattlePhase.CommandPhase) return;

        // ← THÊM: khi đang chờ chọn chiêu, không xử lý click chuột
        if (_step == InputStep.SelectSkill) return;

        GridPos hoverPos = GetMouseGridPos();

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

    void UpdateAoEPreview(GridPos hoverPos)
    {
        MoveData move = _selectedEntity.GetMove();
        if (move == null) return;

        var grid = BattleGridManager.Instance;
        if (_validAttackCells.Contains(hoverPos))
        {
            var aoeCells = grid.GetAoECells(hoverPos, move.shape, _pendingMove);
            grid.ShowAoEPreview(aoeCells);
        }
        else
        {
            grid.ShowHighlight(_validAttackCells);
        }
    }

    // ── Bước 1: Chọn ô di chuyển ─────────────────────────────────
    void HandleMoveSelection(GridPos clicked)
    {
        if (!_validMoveCells.Contains(clicked))
        {
            Debug.Log($"[Command] Ô {clicked} không hợp lệ");
            return;
        }

        _pendingMove = clicked;

        // ← THÊM: chuyển sang bước chọn chiêu, xoá highlight
        _step = InputStep.SelectSkill;
        BattleGridManager.Instance.ClearHighlight();

        var moves = _selectedEntity.Data.moves;

        // Nếu không có moves list, dùng defaultMove và bỏ qua bước chọn
        if (moves == null || moves.Count == 0)
        {
            _selectedEntity.SetChosenMove(_selectedEntity.Data.defaultMove);
            GoToAttackStep();
            return;
        }

        // Mở UI chọn chiêu
        MoveSelectionUI.Instance.Show(moves, OnMoveChosen);
        Debug.Log($"[Command] Di chuyển đến {_pendingMove} — Đang chọn chiêu...");
    }

    // ← THÊM: callback khi chọn xong chiêu
    void OnMoveChosen(MoveData move)
    {
        _selectedEntity.SetChosenMove(move);
        GoToAttackStep();
        Debug.Log($"[Command] Chiêu đã chọn: {move.moveName}");
    }

    // ← THÊM: chuyển sang bước chọn ô tấn công
    void GoToAttackStep()
    {
        _step = InputStep.SelectAttack;
        ShowAttackHighlight(_pendingMove);
        Debug.Log($"[Command] → Chọn ô tấn công");
    }

    // ── Bước 3: Chọn ô tấn công ──────────────────────────────────
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
        else
            SubmitEnemyCommands();
    }

    // ── Enemy AI ─────────────────────────────────────────────────
    void SubmitEnemyCommands()
    {
        var all = FindObjectsByType<BattleEntity>(FindObjectsInactive.Exclude);
        var players = new List<BattleEntity>();
        var enemies = new List<BattleEntity>();
        foreach (var e in all)
            (e.TeamId == 0 ? players : enemies).Add(e);

        foreach (var enemy in enemies)
        {
            var cmd = EnemyAIBrain.Decide(enemy, players);
            BattlePhaseManager.Instance.SubmitCommand(enemy, cmd);
        }
    }

    GridPos GetMouseGridPos()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        worldPos.z = 0;
        return BattleGridManager.Instance.WorldToGrid(worldPos);
    }
}