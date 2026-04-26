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
            int cmp = b.Key.Speed.CompareTo(a.Key.Speed);
            return cmp != 0 ? cmp : b.Key.TieBreakRoll.CompareTo(a.Key.TieBreakRoll);
        });

        foreach (var kvp in ordered)
        {
            BattleEntity attacker = kvp.Key;
            BattleCommand cmd = kvp.Value;
            if (attacker == null || attacker.IsDead) continue;

            MoveData move = attacker.GetMove();
            if (move == null || move.effects.Count == 0) continue;

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

            var targets = ResolveTargets(attacker, cmd, effect);
            var result = effect.Resolve(attacker, targets);

            if (!string.IsNullOrEmpty(result.logMessage))
                Debug.Log($"[Effect] {result.logMessage}");

            // Áp dụng kết quả thực tế
            ApplyEffectResult(result, attacker, cmd, move, effect);

            yield return StartCoroutine(effect.PlayAnimation(result, attacker));
        }
    }

    void ApplyEffectResult(EffectResult result, BattleEntity attacker,
                           BattleCommand cmd, MoveData move, MoveEffect effect)
    {
        switch (result.resultType)
        {
            case EffectResultType.Damage:
                foreach (var (target, dmg) in result.hits)
                    target.TakeDamage(dmg, false);
                break;

            case EffectResultType.Heal:
                foreach (var (target, amt) in result.hits)
                    target.Heal(amt);
                if (attacker.TeamId == 1) attacker.IncrementBuffCount();
                break;

            case EffectResultType.StatStage:
                // Đã apply bên trong StatStageEffect.Resolve — không apply lại
                if (attacker.TeamId == 1) attacker.IncrementBuffCount();
                break;

            case EffectResultType.Terrain:
                if (effect is TerrainEffect te && cmd.HasAttack)
                {
                    var cells = BattleGridManager.Instance.GetAoECells(
                        cmd.attackTarget, te.terrainShape, attacker.GridPos, te.aoeRadius);
                    foreach (var cell in cells)
                        TerrainManager.Instance.PlaceTerrain(cell, BuildTerrainMoveData(te, move));
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
        }
    }

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

            bool isEnemy = entity.TeamId != attacker.TeamId;
            bool isAlly = entity.TeamId == attacker.TeamId;

            switch (effect.targetScope)
            {
                case TargetScope.EnemySide: if (isEnemy) result.Add(entity); break;
                case TargetScope.OwnSide: if (isAlly) result.Add(entity); break;
                case TargetScope.BothSides: result.Add(entity); break;
            }
        }

        return result;
    }

    // Helper tạo MoveData tạm để WeatherManager / TerrainManager đọc
    MoveData BuildWeatherMoveData(WeatherEffect we, MoveData source)
    {
        var tmp = ScriptableObject.CreateInstance<MoveData>();
        tmp.moveName = source.moveName;
        tmp.elementType = source.elementType;
        return tmp;
    }

    MoveData BuildTerrainMoveData(TerrainEffect te, MoveData source)
    {
        var tmp = ScriptableObject.CreateInstance<MoveData>();
        tmp.moveName = source.moveName;
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
}