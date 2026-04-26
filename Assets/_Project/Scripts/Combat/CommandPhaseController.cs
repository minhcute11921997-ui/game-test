using System.Collections.Generic;
using UnityEngine;

public class CommandPhaseController : MonoBehaviour
{
    public static CommandPhaseController Instance { get; private set; }

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

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
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
        Debug.Log($"[Command] Chọn {entity.name} — Chọn ô di chuyển");
    }

    void ShowMoveHighlight(GridPos origin)
    {
        if (_selectedEntity.IsImmobilized())
        {
            _validMoveCells = new List<GridPos> { origin };
            BattleGridManager.Instance.ShowHighlightColored(_validMoveCells, new Color(1f, 0.2f, 0.2f, 0.6f));
            Debug.Log($"[Command] {_selectedEntity.name} đang bị trói bởi địa hình/thời tiết!");
        }
        else
        {
            _validMoveCells = GetReachableCells(origin, _selectedEntity.MoveRange, _selectedEntity.TeamId);
            BattleGridManager.Instance.ShowHighlight(_validMoveCells);
        }
    }

    List<GridPos> GetReachableCells(GridPos origin, int range, int teamId)
    {
        var result = new List<GridPos>();
        var cfg = BattleGridManager.Instance.config;

        for (int dc = -range; dc <= range; dc++)
            for (int dr = -range; dr <= range; dr++)
            {
                if (Mathf.Max(Mathf.Abs(dc), Mathf.Abs(dr)) > range) continue;

                int c = origin.col + dc;
                int r = origin.row + dr;

                if (!cfg.IsWalkable(c, r)) continue;
                if (cfg.GetTeam(c) != teamId) continue;

                var pos = new GridPos(c, r);
                if (BattleGridManager.Instance.IsOccupied(pos) && pos != origin) continue;

                result.Add(pos);
            }

        return result;
    }

    void ShowAttackHighlightColored(GridPos from)
    {
        _validAttackCells = GetAttackableCellsForCurrentMove(from);

        MoveData move = _selectedEntity.GetMove();
        Color hlColor = move?.category switch
        {
            MoveCategory.Physical => new Color(1f, 0.55f, 0.2f, 0.6f),
            MoveCategory.Special => new Color(0.3f, 0.55f, 1f, 0.6f),
            MoveCategory.Status when move.statusSubType == StatusSubType.Buff
                => new Color(0.3f, 1f, 0.5f, 0.6f),
            MoveCategory.Status when move.statusSubType == StatusSubType.Debuff
                => new Color(0.8f, 0.3f, 1f, 0.6f),
            MoveCategory.Status => new Color(1f, 0.5f, 0.8f, 0.6f),
            MoveCategory.Weather => new Color(0.55f, 0.85f, 1f, 0.6f),  // xanh trời
            MoveCategory.Terrain => new Color(0.6f, 0.2f, 1f, 0.55f),   // tím
            _ => new Color(0.8f, 0.8f, 0.8f, 0.6f),
        };

        BattleGridManager.Instance.ShowHighlightColored(_validAttackCells, hlColor);
        _lastHoverPos = new GridPos(-999, -999);
    }

    List<GridPos> GetAttackableCellsForCurrentMove(GridPos from)
    {
        MoveData move = _selectedEntity.GetMove();
        var cfg = BattleGridManager.Instance.config;
        var result = new List<GridPos>();

        if (move == null) return result;

        // NoTarget → submit luôn, không highlight gì
        if (move.targetScope == TargetScope.NoTarget)
            return result;

        int cStart, cEnd;
        switch (move.targetScope)
        {
            case TargetScope.OwnSide:
                cStart = _selectedEntity.TeamId == 0 ? 0 : cfg.RightMinCol;
                cEnd = _selectedEntity.TeamId == 0 ? cfg.LeftMaxCol : cfg.TotalCols - 1;
                break;
            case TargetScope.BothSides:
                cStart = 0;
                cEnd = cfg.TotalCols - 1;
                break;
            case TargetScope.EnemySide:
            default:
                cStart = _selectedEntity.TeamId == 0 ? cfg.RightMinCol : 0;
                cEnd = _selectedEntity.TeamId == 0 ? cfg.TotalCols - 1 : cfg.LeftMaxCol;
                break;
        }

        for (int c = cStart; c <= cEnd; c++)
            for (int r = 0; r < cfg.boardRows; r++)
                if (cfg.IsWalkable(c, r))
                    result.Add(new GridPos(c, r));

        return result;
    }

    void Update()
    {
        if (BattlePhaseManager.Instance.CurrentPhase != BattlePhase.CommandPhase) return;

        GridPos hoverPos = GetMouseGridPos();

        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            StepBack();
            return;
        }

        if (_step == InputStep.SelectAttack && !hoverPos.Equals(_lastHoverPos))
        {
            _lastHoverPos = hoverPos;
            UpdateAoEPreview(hoverPos);
        }

        if (_step == InputStep.SelectSkill) return;
        if (!Input.GetMouseButtonDown(0)) return;

        switch (_step)
        {
            case InputStep.SelectMove:
                HandleMoveSelection(hoverPos);
                break;
            case InputStep.SelectAttack:
                HandleAttackSelection(hoverPos);
                break;
        }
    }

    public void StepBack()
    {
        BattleGridManager.Instance.ClearHighlight();

        if (_step == InputStep.SelectAttack)
        {
            Debug.Log("<color=yellow>[Command] Lùi: Hủy chọn ô mục tiêu → Chọn lại chiêu</color>");
            _step = InputStep.SelectSkill;
            _selectedEntity.SetChosenMove(null);
            MoveSelectionUI.Instance.Show(_selectedEntity.Data.AllMoves, OnMoveChosen);
        }
        else if (_step == InputStep.SelectSkill)
        {
            Debug.Log("<color=yellow>[Command] Lùi: Hủy chọn chiêu → Chọn lại ô di chuyển</color>");
            _step = InputStep.SelectMove;
            MoveSelectionUI.Instance.Hide();
            _pendingMove = new GridPos(-999, -999);
            ShowMoveHighlight(_selectedEntity.GridPos);
        }
        else if (_step == InputStep.SelectMove)
        {
            if (_currentUnitIndex > 0)
            {
                Debug.Log($"<color=yellow>[Command] Lùi: Hủy lượt {_selectedEntity.name} → Quay lại nhân vật trước</color>");
                _currentUnitIndex--;
                SelectUnit(_playerEntities[_currentUnitIndex]);
            }
            else
            {
                Debug.Log("[Command] Đang ở nhân vật đầu tiên, không thể lùi thêm.");
                ShowMoveHighlight(_selectedEntity.GridPos);
            }
        }
    }

    void UpdateAoEPreview(GridPos hoverPos)
    {
        MoveData move = _selectedEntity.GetMove();
        if (move == null) return;

        var grid = BattleGridManager.Instance;

        // Preview Địa Hình
        if (move.category == MoveCategory.Terrain)
        {
            if (_validAttackCells.Contains(hoverPos))
            {
                var cells = grid.GetAoECells(hoverPos, move.terrainShape, _pendingMove, move.aoeRadius);
                grid.ShowHighlightColored(cells, new Color(1f, 0.6f, 0f, 0.75f));
            }
            else
            {
                ShowTerrainHighlight();
            }
            return;
        }

        WeatherManager.Instance.GetEffectiveAoE(
            _selectedEntity.TeamId, move.shape, move.aoeRadius,
            out AttackShape effShape, out int effRadius);

        if (_validAttackCells.Contains(hoverPos))
            grid.ShowAoEPreview(grid.GetAoECells(hoverPos, effShape, _pendingMove, effRadius));
        else
            grid.ShowHighlight(_validAttackCells);
    }

    void HandleMoveSelection(GridPos clicked)
    {
        if (!_validMoveCells.Contains(clicked))
        {
            Debug.Log($"[Command] Ô {clicked} không hợp lệ");
            return;
        }

        _pendingMove = clicked;
        _step = InputStep.SelectSkill;
        BattleGridManager.Instance.ClearHighlight();

        Debug.Log($"<color=green>[Player Command]</color> {_selectedEntity.Data.thingName} chuẩn bị di chuyển đến ô: {_pendingMove}");

        var moves = _selectedEntity.Data.AllMoves;
        if (moves == null || moves.Count == 0)
        {
            GoToAttackStep();
            return;
        }

        MoveSelectionUI.Instance.Show(moves, OnMoveChosen);
        Debug.Log($"[Command] Đến {_pendingMove} — Đang chọn chiêu...");
    }

    void OnMoveChosen(MoveData move)
    {
        _selectedEntity.SetChosenMove(move);
        MoveSelectionUI.Instance.Hide();

        Debug.Log($"<color=green>[Player Command]</color> {_selectedEntity.Data.thingName} chọn chiêu: {move.moveName} (Category: {move.category})");

        // Địa Hình → vào bước chọn ô đặt
        if (move.category == MoveCategory.Terrain)
        {
            GoToTerrainStep();
            return;
        }

        // Thời Tiết cả 2 sân → submit luôn không cần chọn ô
        if (move.category == MoveCategory.Weather && move.weatherTarget == WeatherTarget.Both)
        {
            var envCmd = BattleCommand.MoveOnly(_selectedEntity.GridPos, _pendingMove);
            BattlePhaseManager.Instance.SubmitCommand(_selectedEntity, envCmd);

            _currentUnitIndex++;
            if (_currentUnitIndex < _playerEntities.Count)
                SelectUnit(_playerEntities[_currentUnitIndex]);
            else
                SubmitEnemyCommands();
            return;
        }

        GoToAttackStep();
        Debug.Log($"[Command] Chiêu: {move.moveName}");
    }

    void GoToAttackStep()
    {
        _step = InputStep.SelectAttack;
        ShowAttackHighlightColored(_pendingMove);
    }

    void GoToTerrainStep()
    {
        _step = InputStep.SelectAttack;
        ShowTerrainHighlight();
        Debug.Log("[Command] Chiêu Terrain — chọn ô đặt địa hình");
    }

    void ShowTerrainHighlight()
    {
        var cfg = BattleGridManager.Instance.config;
        _validAttackCells = new List<GridPos>();

        for (int c = 0; c < cfg.TotalCols; c++)
            for (int r = 0; r < cfg.boardRows; r++)
                if (cfg.IsWalkable(c, r))
                    _validAttackCells.Add(new GridPos(c, r));

        BattleGridManager.Instance.ShowHighlightColored(
            _validAttackCells, new Color(0.6f, 0.2f, 1f, 0.55f));
        _lastHoverPos = new GridPos(-999, -999);
    }

    void HandleAttackSelection(GridPos clicked)
    {
        BattleGridManager.Instance.ClearHighlight();

        BattleCommand cmd;
        if (_validAttackCells.Contains(clicked))
        {
            cmd = BattleCommand.MoveAndAttack(_pendingMove, clicked);
            Debug.Log($"<color=green>[Player Command] CHỐT LỆNH:</color> {_selectedEntity.Data.thingName} đi đến ô {_pendingMove} VÀ chọn ô {clicked}");
        }
        else
        {
            cmd = BattleCommand.MoveOnly(_selectedEntity.GridPos, _pendingMove);
            Debug.Log($"<color=green>[Player Command] CHỐT LỆNH:</color> {_selectedEntity.Data.thingName} CHỈ đi đến ô {_pendingMove}");
        }

        BattlePhaseManager.Instance.SubmitCommand(_selectedEntity, cmd);

        _currentUnitIndex++;
        if (_currentUnitIndex < _playerEntities.Count)
            SelectUnit(_playerEntities[_currentUnitIndex]);
        else
            SubmitEnemyCommands();
    }

    void SubmitEnemyCommands()
    {
        var all = FindObjectsByType<BattleEntity>(FindObjectsInactive.Exclude);
        var players = new List<BattleEntity>();
        var enemies = new List<BattleEntity>();

        foreach (var e in all)
            (e.TeamId == 0 ? players : enemies).Add(e);

        foreach (var enemy in enemies)
            BattlePhaseManager.Instance.SubmitCommand(enemy, EnemyAIBrain.Decide(enemy, players));
    }

    GridPos GetMouseGridPos()
    {
        Vector3 mp = Input.mousePosition;
        mp.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 wp = Camera.main.ScreenToWorldPoint(mp);
        wp.z = 0;
        return BattleGridManager.Instance.WorldToGrid(wp);
    }
}