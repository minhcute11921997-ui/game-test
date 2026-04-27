// Assets/_Project/Scripts/Combat/BattlePhaseManager.cs
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum BattlePhase { Idle, CommandPhase, ExecutionPhase, JudgePhase, ResultPhase }

public class BattlePhaseManager : MonoBehaviour
{
    public static BattlePhaseManager Instance { get; private set; }
    public BattlePhase CurrentPhase { get; private set; } = BattlePhase.Idle;

    private readonly Dictionary<BattleEntity, BattleCommand> _commands = new();
    [SerializeField] BattleResultManager resultManager;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    void Start() => StartCoroutine(StartAfterSpawn());

    IEnumerator StartAfterSpawn() { yield return null; BeginCommandPhase(); }

    public void BeginCommandPhase()
    {
        CurrentPhase = BattlePhase.CommandPhase;
        _commands.Clear();
        Debug.Log("[BattlePhase] === COMMAND PHASE ===");

        if (WeatherManager.Instance != null)
        {
            var lw = WeatherManager.Instance.GetWeatherForTeam(0);
            var rw = WeatherManager.Instance.GetWeatherForTeam(1);
            Debug.Log($"<color=cyan>[THỜI TIẾT]</color> Sân Trái: <b>{lw}</b> | Sân Phải: <b>{rw}</b>");
        }

        foreach (var entity in BattleGridManager.Instance.GetAllEntities())
        {
            entity.OnTurnStart();
            entity.IncrementTurnCount();
        }

        TerrainManager.Instance.OnTurnStart();
        CommandPhaseController.Instance.BeginInput();
    }

    public void SubmitCommand(BattleEntity entity, BattleCommand cmd)
    {
        _commands[entity] = cmd;
        Debug.Log($"[Command] {entity.name} → Move:{cmd.moveTarget} Attack:{cmd.attackTarget}");
        if (_commands.Count >= GetActiveEntityCount())
            StartCoroutine(BeginExecutionPhase());
    }

    int GetActiveEntityCount() => BattleGridManager.Instance.GetAllEntities().Count;

    IEnumerator BeginExecutionPhase()
    {
        CurrentPhase = BattlePhase.ExecutionPhase;
        Debug.Log("[BattlePhase] === EXECUTION PHASE ===");

        var grid = BattleGridManager.Instance;
        var moveCoroutines = new List<Coroutine>();
        var entities = new List<BattleEntity>(_commands.Keys);

        foreach (var entity in entities)
        {
            var cmd = _commands[entity];
            GridPos targetPos = cmd.moveTarget;

            if (entity.IsForcedStillByMagnet())
            { Debug.Log($"[Weather] {entity.name} bị Từ Trường khoá"); targetPos = entity.GridPos; }

            if (!entity.CanMove)
            { Debug.Log($"[Terrain] {entity.name} bị Bẫy Gai khoá"); targetPos = entity.GridPos; }

            if (!targetPos.Equals(entity.GridPos))
            {
                entity.TrackMovement(targetPos);
                moveCoroutines.Add(StartCoroutine(grid.MoveEntitySmooth(entity, targetPos, null, 0.3f)));
            }
        }

        foreach (var co in moveCoroutines) yield return co;

        foreach (var entity in entities)
        {
            entity.OnMoved();
            TerrainManager.Instance.OnEntityEnterCell(entity, entity.GridPos);
        }

        StartCoroutine(BeginJudgePhase());
    }

    IEnumerator BeginJudgePhase()
    {
        CurrentPhase = BattlePhase.JudgePhase;
        Debug.Log("[BattlePhase] === JUDGE PHASE ===");

        var ordered = new List<KeyValuePair<BattleEntity, BattleCommand>>(_commands);
        ordered.Sort((a, b) =>
        {
            int cmp = b.Key.EffectiveSpeed.CompareTo(a.Key.EffectiveSpeed);
            return cmp != 0 ? cmp : b.Key.TieBreakRoll.CompareTo(a.Key.TieBreakRoll);
        });

        foreach (var kvp in ordered)
        {
            BattleEntity attacker = kvp.Key;
            BattleCommand cmd = kvp.Value;
            if (attacker == null || attacker.IsDead) continue;

            MoveData move = attacker.GetMove();
            if (move == null || move.effects.Count == 0) continue;

            if (!attacker.UseMove(move))
            {
                Debug.Log($"<color=red>[PP]</color> {attacker.name} hết PP {move.moveName}, bỏ lượt!");
                continue;
            }

            Debug.Log($"<color=cyan>[Judge]</color> {attacker.name} dùng {move.moveName}");
            yield return StartCoroutine(ExecuteMove(attacker, cmd, move));
        }

        yield return StartCoroutine(EndJudgePhase());
    }

    IEnumerator ExecuteMove(BattleEntity attacker, BattleCommand cmd, MoveData move)
    {
        foreach (var effect in move.effects)
        {
            if (Random.value > effect.triggerChance)
            {
                Debug.Log($"[Effect] {effect.GetType().Name} không kích hoạt (chance={effect.triggerChance:P0})");
                continue;
            }

            // ── Damage: xử lý riêng với falloff theo distType ──────
            if (effect is DamageEffect dmgEffect)
            {
                ApplyDamageEffect(attacker, cmd, dmgEffect);
                yield return StartCoroutine(effect.PlayAnimation(
                    new EffectResult { triggered = true, resultType = EffectResultType.Damage }, attacker));
            }
            else
            {
                var targets = ResolveTargets(attacker, cmd, effect);
                var result = effect.Resolve(attacker, targets);
                if (effect is KnockbackEffect)
                    foreach (var t in targets)
                        result.hits.Add((t, 0));
                if (!string.IsNullOrEmpty(result.logMessage))
                    Debug.Log($"[Effect] {result.logMessage}");
                ApplyEffectResult(result, attacker, cmd, move, effect);
                yield return StartCoroutine(effect.PlayAnimation(result, attacker));
            }
        }
    }

    // ── Damage với falloff theo khoảng cách ──────────────────────
    void ApplyDamageEffect(BattleEntity attacker, BattleCommand cmd, DamageEffect effect)
    {
        if (!cmd.HasAttack) return;

        var hitMap = ResolveTargetsWithDistance(attacker, cmd, effect);
        var logMsg = "";

        foreach (var kvp in hitMap)
        {
            BattleEntity target = kvp.Key;
            int distType = kvp.Value;

            float evasionRate = CombatCalculator.CalculateEvasionRate(target.EffectiveLuck);
            if (Random.value < evasionRate / 100f)
            {
                logMsg += $"{target.Data.thingName} né!\n";
                continue;
            }

            var dmgResult = CombatCalculator.CalculateDamage(attacker, target, effect, distType);
            target.TakeDamage(dmgResult.damage, dmgResult.isCritical);

            logMsg += $"{attacker.Data.thingName}→{target.Data.thingName}: {dmgResult.damage} dmg " +
                      $"(x{dmgResult.typeMultiplier:F2})" +
                      $"{(dmgResult.isCritical ? " CHÍ MẠNG!" : "")}" +
                      $" [dist={distType}]\n";
        }

        if (!string.IsNullOrEmpty(logMsg))
            Debug.Log($"[Damage] {logMsg}");
    }

    void ApplyEffectResult(EffectResult result, BattleEntity attacker,
                           BattleCommand cmd, MoveData move, MoveEffect effect)
    {
        switch (result.resultType)
        {
            case EffectResultType.Heal:
                foreach (var (target, amt) in result.hits)
                    target.Heal(amt);
                if (attacker.TeamId == 1) attacker.IncrementBuffCount();
                break;

            case EffectResultType.StatStage:
                if (attacker.TeamId == 1) attacker.IncrementBuffCount();
                break;

            case EffectResultType.Terrain:
                if (effect is TerrainEffect te && cmd.HasAttack)
                {
                    var cells = BattleGridManager.Instance.GetAoECells(
                        cmd.attackTarget, te.terrainShape, attacker.GridPos, te.aoeRadius);
                    foreach (var cell in cells)
                        TerrainManager.Instance.PlaceTerrain(cell, BuildTerrainMoveData(te, move), attacker.TeamId);
                    Debug.Log($"<color=magenta>[Terrain]</color> {attacker.name} đặt {te.terrainType} | {cells.Count} ô");
                }
                break;

            case EffectResultType.Weather:
                if (effect is WeatherEffect we)
                {
                    WeatherManager.Instance.ApplyWeather(
                        BuildWeatherMoveData(we, move), we.targetScope, attacker.TeamId, cmd.attackTarget);
                    Debug.Log($"[Weather] {attacker.name} tung {we.weatherType} → scope={we.targetScope}");
                }
                break;

            case EffectResultType.Knockback:
                if (effect is KnockbackEffect kb && cmd.HasAttack)
                {
                    // Tính hướng đẩy: từ attacker → target, đẩy target tiếp tục theo hướng đó
                    foreach (var (target, _) in result.hits)
                    {
                        if (target == null || target.IsDead) continue;

                        GridPos dir = CalcKnockbackDir(attacker.GridPos, target.GridPos);
                        GridPos dest = FindKnockbackDest(target.GridPos, dir, kb.pushDistance);

                        if (!dest.Equals(target.GridPos))
                        {
                            StartCoroutine(BattleGridManager.Instance
                                .MoveEntitySmooth(target, dest, null, 0.2f));
                            Debug.Log($"[Knockback] {target.Data.thingName} bị đẩy " +
                                      $"từ {target.GridPos} → {dest}");
                        }
                    }
                }
                break;
        }
    }

    // ── ResolveTargets (Heal / Stat / Terrain / Weather) ─────────
    List<BattleEntity> ResolveTargets(BattleEntity attacker, BattleCommand cmd, MoveEffect effect)
    {
        var result = new List<BattleEntity>();
        if (effect.targetScope == TargetScope.NoTarget) return result;
        if (!cmd.HasAttack) return result;

        var cells = BattleGridManager.Instance.GetAoECells(
            cmd.attackTarget, effect.aoeShape, attacker.GridPos, effect.aoeRadius);

        foreach (var cell in cells)
        {
            var entity = BattleGridManager.Instance.GetEntityAt(cell);
            if (entity == null) continue;

            switch (effect.targetScope)
            {
                case TargetScope.EnemySide: if (entity.TeamId != attacker.TeamId) result.Add(entity); break;
                case TargetScope.OwnSide: if (entity.TeamId == attacker.TeamId) result.Add(entity); break;
                case TargetScope.BothSides: result.Add(entity); break;
            }
        }
        return result;
    }

    // ── ResolveTargets với distType — Damage + hỗ trợ footprint lớn ─
    // Key = entity, Value = distType nhỏ nhất tìm được (dame to nhất)
    Dictionary<BattleEntity, int> ResolveTargetsWithDistance(
        BattleEntity attacker, BattleCommand cmd, MoveEffect effect)
    {
        var result = new Dictionary<BattleEntity, int>();
        if (!cmd.HasAttack) return result;

        var cells = BattleGridManager.Instance.GetAoECells(
            cmd.attackTarget, effect.aoeShape, attacker.GridPos, effect.aoeRadius);

        foreach (var cell in cells)
        {
            var entity = BattleGridManager.Instance.GetEntityAt(cell);
            if (entity == null) continue;

            bool valid = effect.targetScope switch
            {
                TargetScope.EnemySide => entity.TeamId != attacker.TeamId,
                TargetScope.OwnSide => entity.TeamId == attacker.TeamId,
                _ => true
            };
            if (!valid) continue;

            int distType = GetCellDistanceType(cell, cmd.attackTarget);
            if (!result.ContainsKey(entity) || distType < result[entity])
                result[entity] = distType;
        }
        return result;
    }

    // distType: 0 = tâm, 1 = cạnh (cùng hàng/cột), 2 = góc chéo
    int GetCellDistanceType(GridPos cell, GridPos center)
    {
        int dc = Mathf.Abs(cell.col - center.col);
        int dr = Mathf.Abs(cell.row - center.row);
        if (dc == 0 && dr == 0) return 0;
        if (dc == 0 || dr == 0) return 1;
        return 2;
    }

    MoveData BuildWeatherMoveData(WeatherEffect we, MoveData source)
    {
        var tmp = ScriptableObject.CreateInstance<MoveData>();
        tmp.moveName = source.moveName;
        tmp.elementType = source.elementType;
        tmp.effects = new System.Collections.Generic.List<MoveEffect> { we };
        return tmp;
    }

    MoveData BuildTerrainMoveData(TerrainEffect te, MoveData source)
    {
        var tmp = ScriptableObject.CreateInstance<MoveData>();
        tmp.moveName = source.moveName;
        tmp.elementType = source.elementType;
        tmp.effects = new System.Collections.Generic.List<MoveEffect> { te };
        return tmp;
    }

    IEnumerator EndJudgePhase()
    {
        yield return new WaitForSeconds(0.5f);

        var deadEntities = new List<BattleEntity>();
        foreach (var entity in BattleGridManager.Instance.GetAllEntities())
            if (entity.IsDead) deadEntities.Add(entity);

        foreach (var dead in deadEntities)
            Destroy(dead.gameObject);

        yield return null;

        var allAlive = BattleGridManager.Instance.GetAllEntities();
        TerrainManager.Instance.OnTurnEnd(allAlive);
        WeatherManager.Instance.OnTurnEnd();

        if (resultManager != null && resultManager.CheckBattleEnd())
        {
            CurrentPhase = BattlePhase.ResultPhase;
            Debug.Log("[BattlePhase] === RESULT PHASE ===");
        }
        else BeginCommandPhase();
    }
    GridPos CalcKnockbackDir(GridPos from, GridPos to)
    {
        int dc = to.col - from.col;
        int dr = to.row - from.row;
        return new GridPos(
            dc == 0 ? 0 : (dc > 0 ? 1 : -1),
            dr == 0 ? 0 : (dr > 0 ? 1 : -1)
        );
    }

    GridPos FindKnockbackDest(GridPos start, GridPos dir, int distance)
    {
        var cfg = BattleGridManager.Instance.config;
        GridPos cur = start;
        for (int i = 0; i < distance; i++)
        {
            var next = new GridPos(cur.col + dir.col, cur.row + dir.row);
            if (!cfg.IsInBounds(next.col, next.row)) break;    // ra khỏi bảng → dừng
            if (!cfg.IsWalkable(next.col, next.row)) break;    // tường → dừng
            var occupant = BattleGridManager.Instance.GetEntityAt(next);
            if (occupant != null) break;                        // có entity chặn → dừng
            cur = next;
        }
        return cur;
    }
}