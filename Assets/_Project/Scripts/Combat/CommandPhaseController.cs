using System.Collections.Generic;
using UnityEngine;

public class CommandPhaseController : MonoBehaviour
{
    public static CommandPhaseController Instance { get; private set; }

    // ← XÓA field moveRange cũ, không cần nữa

    private enum InputStep { SelectUnit, SelectMove, SelectAttack }
    private InputStep _step = InputStep.SelectUnit;

    private BattleEntity _selectedEntity;
    private GridPos _pendingMove;
    private List<GridPos> _validMoveCells = new();
    private List<GridPos> _validAttackCells = new();

    private List<BattleEntity> _playerEntities = new();
    private int _currentUnitIndex = 0;

    void Awake() => Instance = this;

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
        ShowMoveHighlight(entity.GridPos);
        Debug.Log($"[Command] Chọn {entity.name} tại {entity.GridPos} (MoveRange={entity.MoveRange}) — Chọn ô di chuyển");
    }

    void ShowMoveHighlight(GridPos origin)
    {
        // ← đọc MoveRange từ entity thay vì field cứng
        _validMoveCells = GetReachableCells(origin, _selectedEntity.MoveRange, _selectedEntity.TeamId);
        BattleGridManager.Instance.ShowHighlight(_validMoveCells);
    }

    List<GridPos> GetReachableCells(GridPos origin, int range, int teamId)
    {
        var result = new List<GridPos>();
        var config = BattleGridManager.Instance.config;

        for (int dc = -range; dc <= range; dc++)
            for (int dr = -range; dr <= range; dr++)
            {
                // ← HÌNH VUÔNG: dùng Chebyshev (max của |dc|,|dr|) thay vì Manhattan
                // Chebyshev = tự nhiên khi dùng vòng lặp dc/dr không có điều kiện lọc thêm
                // → bỏ dòng Manhattan filter cũ:
                // if (Mathf.Abs(dc) + Mathf.Abs(dr) > range) continue; ← XÓA

                int c = origin.col + dc;
                int r = origin.row + dr;
                if (!config.IsWalkable(c, r)) continue;
                if (config.GetTeam(c) != teamId) continue;
                var pos = new GridPos(c, r);
                if (BattleGridManager.Instance.IsOccupied(pos) && pos != origin) continue;
                result.Add(pos);
            }
        return result;
    }

    void ShowAttackHighlight(GridPos from)
    {
        _validAttackCells = GetAttackableCells(from, _selectedEntity.TeamId);
        BattleGridManager.Instance.ShowHighlight(_validAttackCells);
    }

    List<GridPos> GetAttackableCells(GridPos from, int teamId)
    {
        var result = new List<GridPos>();
        var config = BattleGridManager.Instance.config;
        int colStart = teamId == 0 ? config.RightMinCol : 0;
        int colEnd = teamId == 0 ? config.TotalCols - 1 : config.LeftMaxCol;

        for (int c = colStart; c <= colEnd; c++)
            for (int r = 0; r < config.boardRows; r++)
            {
                var pos = new GridPos(c, r);
                if (BattleGridManager.Instance.GetEntityAt(pos) != null)
                    result.Add(pos);
            }
        return result;
    }

    void Update()
    {
        if (BattlePhaseManager.Instance.CurrentPhase != BattlePhase.CommandPhase) return;
        if (!Input.GetMouseButtonDown(0)) return;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        GridPos clickedPos = BattleGridManager.Instance.WorldToGrid(worldPos);

        switch (_step)
        {
            case InputStep.SelectMove: HandleMoveSelection(clickedPos); break;
            case InputStep.SelectAttack: HandleAttackSelection(clickedPos); break;
        }
    }

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

    void HandleAttackSelection(GridPos clicked)
    {
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

        BattleGridManager.Instance.ClearHighlight();
        BattlePhaseManager.Instance.SubmitCommand(_selectedEntity, cmd);

        _currentUnitIndex++;
        if (_currentUnitIndex < _playerEntities.Count)
            SelectUnit(_playerEntities[_currentUnitIndex]);
        SubmitEnemyCommand();
    }

    void SubmitEnemyCommand()
    {
        foreach (var e in FindObjectsByType<BattleEntity>(FindObjectsInactive.Exclude))
        {
            if (e.TeamId == 1)
            {
                var cmd = BattleCommand.MoveOnly(e.GridPos, e.GridPos);
                BattlePhaseManager.Instance.SubmitCommand(e, cmd);
                break;
            }
        }
    }
}