// Assets/_Project/Scripts/Combat/BattlePhaseManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattlePhase
{
    Idle,
    CommandPhase,
    ExecutionPhase,
    JudgePhase,
    ResultPhase
}

public class BattlePhaseManager : MonoBehaviour
{
    public static BattlePhaseManager Instance { get; private set; }
    public BattlePhase CurrentPhase { get; private set; } = BattlePhase.Idle;

    private readonly Dictionary<BattleEntity, BattleCommand> _commands = new Dictionary<BattleEntity, BattleCommand>();

    [SerializeField] BattleResultManager resultManager;

    // ── Sprint 7: thông tin trận để tính kết quả ─────────────────
    public struct DefeatedRecord
    {
        public string name;
        public int    expYield;
        public int    hitsTaken;
    }
    public readonly List<DefeatedRecord>     DefeatedEnemies = new List<DefeatedRecord>();
    private readonly Dictionary<BattleEntity, int> _hitCounts   = new Dictionary<BattleEntity, int>();

    private int _expectedCommandCount;

    void Awake() => Instance = this;
    void Start()  => StartCoroutine(StartAfterSpawn());

    IEnumerator StartAfterSpawn()
    {
        yield return null; // wait one frame for BattleManager to spawn entities
        BeginCommandPhase();
    }

    // ── Command Phase ──────────────────────────────────────────────
    public void BeginCommandPhase()
    {
        CurrentPhase = BattlePhase.CommandPhase;
        _commands.Clear();

        // Cache expected count at start of phase so dying entities don't shift the threshold
        _expectedCommandCount = FindObjectsByType<BattleEntity>(FindObjectsInactive.Exclude).Length;

        Debug.Log($"[BattlePhase] === COMMAND PHASE === ({_expectedCommandCount} entities)");
        CommandPhaseController.Instance.BeginInput();
    }

    public void SubmitCommand(BattleEntity entity, BattleCommand cmd)
    {
        _commands[entity] = cmd;
        Debug.Log($"[Command] {entity.name} → Move:{cmd.moveTarget} Attack:{cmd.attackTarget}");

        if (_commands.Count >= _expectedCommandCount)
            StartCoroutine(BeginExecutionPhase());
    }

    // ── Execution Phase ────────────────────────────────────────────
    IEnumerator BeginExecutionPhase()
    {
        CurrentPhase = BattlePhase.ExecutionPhase;
        Debug.Log("[BattlePhase] === EXECUTION PHASE ===");

        var grid           = BattleGridManager.Instance;
        var moveCoroutines = new List<Coroutine>();

        foreach (var kvp in _commands)
        {
            BattleEntity entity = kvp.Key;
            GridPos      target = kvp.Value.moveTarget;

            if (entity == null) continue;
            if (!target.Equals(entity.GridPos))
            {
                var co = StartCoroutine(grid.MoveEntitySmooth(entity, target, null, 0.3f));
                moveCoroutines.Add(co);
            }
        }

        foreach (var co in moveCoroutines)
            yield return co;

        yield return StartCoroutine(JudgePhaseCoroutine());
    }

    // ── Judge Phase (coroutine): damage + animation + status + map effects ──
    IEnumerator JudgePhaseCoroutine()
    {
        CurrentPhase = BattlePhase.JudgePhase;
        Debug.Log("[BattlePhase] === JUDGE PHASE ===");

        // Sort by Speed descending
        var ordered = new List<KeyValuePair<BattleEntity, BattleCommand>>(_commands);
        ordered.Sort((a, b) => b.Key.Speed.CompareTo(a.Key.Speed));

        foreach (var kvp in ordered)
        {
            BattleEntity attacker = kvp.Key;
            BattleCommand cmd     = kvp.Value;

            // Skip if attacker died earlier this turn
            if (attacker == null || attacker.CurrentHp <= 0) continue;
            if (!cmd.HasAttack) continue;

            MoveData move = attacker.GetMove();
            if (move == null) continue;

            var cells = BattleGridManager.Instance.GetAoECells(
                cmd.attackTarget, move.shape, attacker.GridPos);

            foreach (var cell in cells)
            {
                BattleEntity target = BattleGridManager.Instance.GetEntityAt(cell);
                if (target == null || target.TeamId == attacker.TeamId) continue;

                // ── Sprint 6: Status move ─────────────────────────────
                if (move.category == MoveCategory.Status)
                {
                    BattleEntity stageTarget = move.affectSelf ? attacker : target;
                    if (move.stageChange != 0)
                    {
                        stageTarget.ApplyStage(move.statTarget, move.stageChange);
                        string sign = move.stageChange >= 0 ? "+" : "";
                        DamagePopup.CreateStatus(stageTarget.transform.position,
                            $"{move.statTarget} {sign}{move.stageChange}");
                    }
                    if (move.inflictStatus != StatusEffect.None)
                        target.ApplyStatusEffect(move.inflictStatus);
                    continue;
                }

                // ── Sprint 5: Evasion check ───────────────────────────
                float evasionRate = CombatCalculator.CalculateEvasionRate(target.Data.luck)
                                  + target.SynergyEvasionBonus;
                if (Random.value < evasionRate / 100f)
                {
                    DamagePopup.CreateMiss(target.transform.position);
                    Debug.Log($"[Evasion] {target.Data.thingName} né đòn!");
                    continue;
                }

                // ── AoE cellDistanceType ──────────────────────────────
                int distType = 0;
                if (move.shape == AttackShape.Square3x3)
                {
                    int dc = Mathf.Abs(cell.col - cmd.attackTarget.col);
                    int dr = Mathf.Abs(cell.row - cmd.attackTarget.row);
                    if      (dc == 0 && dr == 0) distType = 0;
                    else if (dc + dr == 1)        distType = 1;
                    else                          distType = 2;
                }

                // ── Sprint 6: Stage multipliers ───────────────────────
                StatEffectTarget atkStat = move.category == MoveCategory.Physical
                    ? StatEffectTarget.Attack : StatEffectTarget.SpAtk;
                StatEffectTarget defStat = move.category == MoveCategory.Physical
                    ? StatEffectTarget.Defense : StatEffectTarget.SpDef;

                float atkStageMult = CombatCalculator.GetStageMultiplier(attacker.GetStage(atkStat))
                                   * attacker.SynergyAtkMult;
                float defStageMult = CombatCalculator.GetStageMultiplier(target.GetStage(defStat))
                                   * target.SynergyDefMult;

                // Sprint 9: Legacy Colossus defense bonus
                if (SynergyManager.Instance != null)
                    defStageMult *= (1f + SynergyManager.Instance.GetLegacyDefBonus(target.TeamId));

                var r = CombatCalculator.Calculate(
                    attacker.Data,
                    target.Data,
                    move,
                    attackerLevel:    attacker.Level,
                    attackerLuck:     attacker.Data.luck,
                    atkStageMult:     atkStageMult,
                    defStageMult:     defStageMult,
                    aoeShape:         move.shape,
                    cellDistanceType: distType
                );

                string eff  = r.typeMultiplier > 1f ? " HIỆU QUẢ!" :
                              r.typeMultiplier < 1f ? " Không hiệu quả..." : "";
                string crit = r.isCritical ? " CHÍ MẠNG!" : "";
                string stab = r.isStab     ? " [STAB]"    : "";

                Debug.Log($"[Combat] {attacker.Data.thingName}{stab} → " +
                          $"{target.Data.thingName}: {r.damage} dmg " +
                          $"(x{r.typeMultiplier}){eff}{crit}");

                // ── Track hits (for Clean Kill stars, Sprint 7) ────────
                if (!_hitCounts.ContainsKey(target)) _hitCounts[target] = 0;
                _hitCounts[target]++;

                // Apply damage
                int prevHp = target.CurrentHp;
                target.TakeDamage(r.damage, r.isCritical);

                // ── Sprint 5: inflict status from damaging move ────────
                if (move.inflictStatus != StatusEffect.None && target.CurrentHp > 0)
                    target.ApplyStatusEffect(move.inflictStatus);

                // ── Sprint 7: record defeat ────────────────────────────
                if (target.CurrentHp <= 0 && prevHp > 0 && target.TeamId == 1)
                {
                    int hits = _hitCounts.TryGetValue(target, out int h) ? h : 1;
                    DefeatedEnemies.Add(new DefeatedRecord
                    {
                        name     = target.Data.thingName,
                        expYield = target.Data.expYield,
                        hitsTaken = hits
                    });
                }

                // ── Sprint 5: hit animation (only for surviving entities) ──
                if (target != null && target.CurrentHp > 0)
                    yield return StartCoroutine(target.HitAnimation(attacker.transform.position));
            }
        }

        // ── End-of-turn: status tick, angel regen, map effects ────
        yield return new WaitForSeconds(0.3f);

        var allEntities = FindObjectsByType<BattleEntity>(FindObjectsInactive.Exclude);
        foreach (var e in allEntities)
        {
            e.TickStatus();

            // Angel synergy regen (% of MaxHP)
            if (e.SynergyRegenPercent > 0f)
                e.HealHp(Mathf.Max(1, Mathf.RoundToInt(e.Data.hp * e.SynergyRegenPercent / 100f)));
        }

        // Map end-of-turn effects (Sprint 8)
        BattleGridManager.Instance.TriggerEndOfTurnEffects(allEntities);

        // Legacy bomb effects (Sprint 9)
        if (SynergyManager.Instance != null)
            SynergyManager.Instance.ApplyLegacyEffects();

        yield return new WaitForSeconds(0.5f);

        // ── Check battle end ──────────────────────────────────────
        if (resultManager != null && resultManager.CheckBattleEnd(_hitCounts))
        {
            CurrentPhase = BattlePhase.ResultPhase;
            Debug.Log("[BattlePhase] === RESULT PHASE ===");
        }
        else
        {
            BeginCommandPhase();
        }
    }
}