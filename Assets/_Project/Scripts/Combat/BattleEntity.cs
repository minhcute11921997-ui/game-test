// Assets/_Project/Scripts/Combat/BattleEntity.cs
using System.Collections.Generic;
using UnityEngine;

public class BattleEntity : MonoBehaviour
{
    [HideInInspector] public GridPos GridPos;
    [HideInInspector] public int TeamId;
    [HideInInspector] public EntityHpBar hpBar;

    private float _tieBreakRoll;
    public float TieBreakRoll => _tieBreakRoll;
    public int Speed { get; private set; }
    public int MoveRange { get; private set; }
    public ThingData Data { get; private set; }

    public GridPos LastGridPos { get; private set; }
    public GridPos LastMoveDir { get; private set; }
    public int AIBuffCount { get; private set; }
    public int TurnCount { get; private set; }
    public int Level => Data != null ? Data.level : 1;

    private int _currentHp;
    public int CurrentHp => _currentHp;
    private int _maxHp;
    public int MaxHp => _maxHp;

    private bool _canMove = true;
    private bool _lockedNextTurn = false;
    public bool CanMove => _canMove;

    // ── Stat Stages (StatType thay string) ───────────────────────
    private Dictionary<StatType, int> _statStages = new();

    public int GetStage(StatType stat) =>
        _statStages.TryGetValue(stat, out int v) ? v : 0;

    /// <summary>Áp dụng stage, trả về stage mới sau khi clamp.</summary>
    public int ApplyStage(StatType stat, int delta)
    {
        int cur = GetStage(stat);
        int newStage = Mathf.Clamp(cur + delta, -6, 6);
        _statStages[stat] = newStage;
        return newStage;
    }

    public float EffectiveAttack => Data.attack * CombatCalculator.GetStageMultiplier(GetStage(StatType.Attack));
    public float EffectiveSpAtk => Data.spAtk * CombatCalculator.GetStageMultiplier(GetStage(StatType.SpAtk));
    public float EffectiveDefense => Data.defense * CombatCalculator.GetStageMultiplier(GetStage(StatType.Defense));
    public float EffectiveSpDef => Data.spDef * CombatCalculator.GetStageMultiplier(GetStage(StatType.SpDef));
    public float EffectiveSpeed => Data.speed * CombatCalculator.GetStageMultiplier(GetStage(StatType.Speed));

    public bool IsImmobilized() => !_canMove || IsForcedStillByMagnet();

    private MoveData _chosenMove;
    private Dictionary<MoveData, int> _currentPP = new();

    public int GetCurrentPP(MoveData move)
    {
        if (move == null) return 0;
        if (!_currentPP.ContainsKey(move))
            _currentPP[move] = move.maxPP;   // khởi tạo lần đầu
        return _currentPP[move];
    }

    public bool UseMove(MoveData move)
    {
        if (move == null) return false;
        int pp = GetCurrentPP(move);
        if (pp <= 0)
        {
            Debug.Log($"[PP] {Data.thingName}: {move.moveName} đã hết PP!");
            return false;
        }
        _currentPP[move] = pp - 1;
        Debug.Log($"[PP] {Data.thingName}: {move.moveName} PP {pp} → {pp - 1}");
        return true;
    }
    public MoveData GetMove() => _chosenMove ?? Data?.defaultMove;
    public void SetChosenMove(MoveData move) => _chosenMove = move;

    private int _moveCountThisCycle = 0;

    public void Init(ThingData data, int teamId)
    {
        Data = data;
        TeamId = teamId;
        _currentHp = data.hp;
        _maxHp = data.hp;
        Speed = (int)data.speed;
        MoveRange = data.moveRange;
        LastMoveDir = new GridPos(0, 0);
        LastGridPos = new GridPos(0, 0);
        AIBuffCount = 0;
        TurnCount = 0;
        Debug.Log($"[BattleEntity] Spawn {data.thingName} | Team {teamId} | HP {_currentHp} | Spd {Speed}");
    }

    public void OnTurnStart()
    {
        _canMove = !_lockedNextTurn;
        _lockedNextTurn = false;
        _tieBreakRoll = UnityEngine.Random.value;
    }

    public void LockMovementNextTurn() => _lockedNextTurn = true;

    public void TakeDamage(int dmg, bool isCrit = false)
    {
        _currentHp = Mathf.Max(0, _currentHp - dmg);
        float pct = (float)_currentHp / _maxHp * 100f;
        Debug.Log($"[HP] {Data.thingName}: {_currentHp}/{_maxHp} ({pct:F0}%)");
        DamagePopup.Create(transform.position, dmg, isCrit);
        hpBar?.SetHp(_currentHp, _maxHp);
        if (_currentHp <= 0) Die();
    }

    public void Heal(int amount)
    {
        int before = _currentHp;
        _currentHp = Mathf.Min(_maxHp, _currentHp + amount);
        int actual = _currentHp - before;
        Debug.Log($"[HP] {Data.thingName} hồi {actual} HP: {_currentHp}/{_maxHp}");
        hpBar?.SetHp(_currentHp, _maxHp);
    }

    public void OnMoved()
    {
        if (WeatherManager.Instance.IsMagneticFieldActive(TeamId))
        {
            _moveCountThisCycle++;
            if (_moveCountThisCycle >= 3) _moveCountThisCycle = 0;
        }
        else _moveCountThisCycle = 0;
    }

    public void TrackMovement(GridPos newPos)
    {
        LastMoveDir = new GridPos(
            newPos.col > GridPos.col ? 1 : newPos.col < GridPos.col ? -1 : 0,
            newPos.row > GridPos.row ? 1 : newPos.row < GridPos.row ? -1 : 0);
        LastGridPos = GridPos;
    }

    public void IncrementBuffCount() => AIBuffCount++;
    public void IncrementTurnCount() => TurnCount++;

    public bool IsForcedStillByMagnet()
    {
        if (!WeatherManager.Instance.IsMagneticFieldActive(TeamId)) return false;
        return _moveCountThisCycle == 2;
    }

    private bool _isDead = false;
    public bool IsDead => _isDead;
    void Die()
    {
        _isDead = true;
        Debug.Log($"[Battle] {Data.thingName} bị hạ!");
        BattleGridManager.Instance.RemoveEntity(GridPos);
        hpBar?.gameObject.SetActive(false);
    }
}