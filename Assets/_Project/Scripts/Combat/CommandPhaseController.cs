using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CommandPhaseController : MonoBehaviour
{
    public static CommandPhaseController Instance { get; private set; }

    private enum InputStep { SelectMove, SelectSkill, SelectAttack }
    private InputStep _step = InputStep.SelectMove;

    private bool _inputActive = false;

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
        _inputActive = false;           // ← KHÓA: chờ player bấm Chiến Đấu
        BattleActionPanel.Instance.Show();
    }

    List<BattleEntity> FindPlayerEntities()
    {
        var result = new List<BattleEntity>();
        foreach (var e in FindObjectsByType<BattleEntity>(FindObjectsInactive.Exclude))
            if (e.TeamId == 0) result.Add(e);
        return result;
    }

    public void StartFightFlow()
    {
        _inputActive = true;            // ← MỞ: player chọn Chiến Đấu
        SelectUnit(_playerEntities.Count > 0 ? _playerEntities[0] : null);
    }

    public void SkipPlayerTurn()
    {
        _inputActive = false;           // ← KHÓA: bỏ lượt
        foreach (var entity in _playerEntities)
        {
            var skipCmd = BattleCommand.MoveOnly(entity.GridPos, entity.GridPos);
            BattlePhaseManager.Instance.SubmitCommand(entity, skipCmd);
        }
        SubmitEnemyCommands();
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
                var footprintAtDest = BattleGridManager.Instance
    .GetFootprintCells(pos, _selectedEntity.Data.footprint);
                bool blocked = false;
                foreach (var fp in footprintAtDest)
                {
                    if (fp.Equals(origin)) continue; // ô gốc của chính mình thì bỏ qua
                    var occupant = BattleGridManager.Instance.GetEntityAt(fp);
                    if (occupant != null && occupant != _selectedEntity) { blocked = true; break; }
                }
                if (blocked) continue;

                result.Add(pos);
            }

        return result;
    }

    void ShowAttackHighlightColored(GridPos from)
    {
        _validAttackCells = GetAttackableCellsForCurrentMove(from);

        MoveData move = _selectedEntity.GetMove();
        Color hlColor;
        if (move.GetTerrain() != null)
            hlColor = new Color(0.6f, 0.2f, 1f, 0.55f);
        else if (move.GetWeather() != null)
            hlColor = new Color(0.55f, 0.85f, 1f, 0.6f);
        else
        {
            var dmgEffect = move.GetDamage();
            if (dmgEffect == null)
            {
                var statEffect = move.effects.OfType<StatStageEffect>().FirstOrDefault();
                hlColor = (statEffect != null && statEffect.delta > 0)
                    ? new Color(0.3f, 1f, 0.5f, 0.6f)
                    : new Color(0.8f, 0.3f, 1f, 0.6f);
            }
            else
                hlColor = dmgEffect.damageCategory == MoveCategory.Physical
                    ? new Color(1f, 0.55f, 0.2f, 0.6f)
                    : new Color(0.3f, 0.55f, 1f, 0.6f);
        }

        BattleGridManager.Instance.ShowHighlightColored(_validAttackCells, hlColor);
        _lastHoverPos = new GridPos(-999, -999);
    }

    List<GridPos> GetAttackableCellsForCurrentMove(GridPos from)
    {
        MoveData move = _selectedEntity.GetMove();
        var cfg = BattleGridManager.Instance.config;
        var result = new List<GridPos>();

        if (move == null) return result;
        if (move.hasNoTarget) return result;

        int cStart, cEnd;
        switch (move.primaryScope)
        {
            case TargetScope.OwnSide:
                cStart = _selectedEntity.TeamId == 0 ? 0 : cfg.RightMinCol;
                cEnd = _selectedEntity.TeamId == 0 ? cfg.LeftMaxCol : cfg.TotalCols - 1;
                break;
            case TargetScope.BothSides:
                cStart = 0; cEnd = cfg.TotalCols - 1;
                break;
            case TargetScope.EnemySide:
            default:
                cStart = _selectedEntity.TeamId == 0 ? cfg.RightMinCol : 0;
                cEnd = _selectedEntity.TeamId == 0 ? cfg.TotalCols - 1 : cfg.LeftMaxCol;
                break;
        }

        var dmg = move.GetDamage();

        // ★ Với Line: chỉ lấy ô trên cùng hàng HOẶC cùng cột với from (ô sau di chuyển)
        if (dmg != null && dmg.aoeShape == AttackShape.Line)
        {
            for (int c = cStart; c <= cEnd; c++)
                for (int r = 0; r < cfg.boardRows; r++)
                {
                    if (!cfg.IsWalkable(c, r)) continue;
                    bool sameRow = (r == from.row);
                    bool sameCol = (c == from.col);
                    if (!sameRow && !sameCol) continue; // loại ô chéo
                    if (c == from.col && r == from.row) continue; // loại chính mình
                    result.Add(new GridPos(c, r));
                }
            return result;
        }

        // Mọi shape khác: giữ nguyên logic cũ
        for (int c = cStart; c <= cEnd; c++)
            for (int r = 0; r < cfg.boardRows; r++)
                if (cfg.IsWalkable(c, r))
                    result.Add(new GridPos(c, r));

        return result;
    }

    void Update()
    {
        if (BattlePhaseManager.Instance.CurrentPhase != BattlePhase.CommandPhase) return;
        if (!_inputActive) return;      // ← GUARD: block hoàn toàn nếu chưa mở

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
            MoveSelectionUI.Instance.Show(_selectedEntity.Data.AllMoves, OnMoveChosen, _selectedEntity);
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
                // Back về màn hình đầu (Chiến Đấu / Bỏ Lượt)
                Debug.Log("<color=yellow>[Command] Lùi: Về màn hình chọn hành động</color>");
                _inputActive = false;
                _selectedEntity = null;
                BattleGridManager.Instance.ClearHighlight();
                BattleActionPanel.Instance.Show();
            }
        }
    }

    void UpdateAoEPreview(GridPos hoverPos)
    {
        MoveData move = _selectedEntity.GetMove();
        if (move == null) return;

        var grid = BattleGridManager.Instance;
        var dmg = move.GetDamage();

        if (dmg != null && dmg.aoeShape == AttackShape.Line)
        {
            if (hoverPos.Equals(_pendingMove))
            {
                grid.ShowHighlightColored(_validAttackCells, /* màu highlight */ new Color(1f, 0.55f, 0.2f, 0.6f));
                return;
            }
        }

        if (move.GetTerrain() != null)
        {
            if (_validAttackCells.Contains(hoverPos))
            {
                var te = move.GetTerrain();
                var cells = grid.GetAoECells(hoverPos, te.terrainShape, _pendingMove, te.aoeRadius);
                grid.ShowHighlightColored(cells, new Color(1f, 0.6f, 0f, 0.75f));
            }
            else
                ShowTerrainHighlight();
            return;
        }

        AttackShape shapeToUse = dmg != null ? dmg.aoeShape : AttackShape.Single;
        int radiusToUse = dmg != null ? dmg.aoeRadius : 1;
        WeatherManager.Instance.GetEffectiveAoE(
            _selectedEntity.TeamId, shapeToUse, radiusToUse,
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

        MoveSelectionUI.Instance.Show(moves, OnMoveChosen, _selectedEntity);
        Debug.Log($"[Command] Đến {_pendingMove} — Đang chọn chiêu...");
    }

    void OnMoveChosen(MoveData move)
    {
        _selectedEntity.SetChosenMove(move);
        MoveSelectionUI.Instance.Hide();

        Debug.Log($"<color=green>[Player Command]</color> {_selectedEntity.Data.thingName} chọn chiêu: {move.moveName} (Category: {move.category})");

        if (move.GetTerrain() != null)
        {
            GoToTerrainStep();
            return;
        }

        if (move.primaryScope == TargetScope.NoTarget)
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
        _inputActive = false;           // ← KHÓA ngay khi bắt đầu submit enemy
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