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

    // ── AI State Tracking ─────────────────────────────────────────
    public GridPos LastGridPos { get; private set; }
    public GridPos LastMoveDir { get; private set; }
    public int AIBuffCount { get; private set; }
    public int TurnCount { get; private set; }

    public int Level => Data != null ? Data.level : 1;

    // ── HP ────────────────────────────────────────────────────────
    private int _currentHp;
    public int CurrentHp => _currentHp;
    private int _maxHp;
    public int MaxHp => _maxHp;

    // ── Bẫy Gai ──────────────────────────────────────────────────
    private bool _canMove = true;
    private bool _lockedNextTurn = false;
    public bool CanMove => _canMove;

    [Header("Stat Stages")]
    private Dictionary<string, int> _statStages = new Dictionary<string, int>();

    public int GetStage(string stat) => _statStages.ContainsKey(stat) ? _statStages[stat] : 0;

    public void ApplyStage(string stat, int delta)
    {
        int cur = GetStage(stat);
        // Giới hạn max buff/debuff từ -6 đến +6
        _statStages[stat] = Mathf.Clamp(cur + delta, -6, 6);
    }

    public float EffectiveAttack => Data.attack * CombatCalculator.GetStageMultiplier(GetStage("attack"));
    public float EffectiveSpAtk => Data.spAtk * CombatCalculator.GetStageMultiplier(GetStage("spAtk"));
    public float EffectiveDefense => Data.defense * CombatCalculator.GetStageMultiplier(GetStage("defense"));
    public float EffectiveSpDef => Data.spDef * CombatCalculator.GetStageMultiplier(GetStage("spDef")); // Thêm SpDef cho đủ
    public float EffectiveSpeed => Data.speed * CombatCalculator.GetStageMultiplier(GetStage("speed"));
    public bool IsImmobilized()
    {
        // Bị trói nếu: dẫm bẫy gai (!CanMove) HOẶC vi phạm Từ Trường
        return !_canMove || IsForcedStillByMagnet();
    }

    // ── Chiêu AI chọn lượt này ───────────────────────────────────
    private MoveData _chosenMove;
    public MoveData GetMove() => _chosenMove ?? Data?.defaultMove;
    public void SetChosenMove(MoveData move) => _chosenMove = move;

    // ── Từ Trường ─────────────────────────────────────────────────
    private int _moveCountThisCycle = 0;

    // ─────────────────────────────────────────────────────────────
    public void Init(ThingData data, int teamId)
    {
        Data = data;
        TeamId = teamId;
        _currentHp = data.hp;
        _maxHp = data.hp;
        Speed = (int)data.speed;
        MoveRange = data.moveRange;

        // Reset AI state
        LastMoveDir = new GridPos(0, 0);
        LastGridPos = new GridPos(0, 0);
        AIBuffCount = 0;
        TurnCount = 0;

        Debug.Log($"[BattleEntity] Spawn {data.thingName} | Team {teamId} | HP {_currentHp} | Spd {Speed}");
    }

    // ── Đầu lượt ─────────────────────────────────────────────────
    public void OnTurnStart()
    {
        _canMove = !_lockedNextTurn;
        _lockedNextTurn = false;
        _tieBreakRoll = UnityEngine.Random.value;
    }

    // ── Khoá di chuyển (TerrainManager gọi) ──────────────────────
    public void LockMovementNextTurn() => _lockedNextTurn = true;

    // ── Nhận damage ───────────────────────────────────────────────
    public void TakeDamage(int dmg, bool isCrit = false)
    {
        _currentHp = Mathf.Max(0, _currentHp - dmg);
        float pct = (float)_currentHp / _maxHp * 100f;
        Debug.Log($"[HP] {Data.thingName}: {_currentHp}/{_maxHp} ({pct:F0}%)");

        DamagePopup.Create(transform.position, dmg, isCrit);
        hpBar?.SetHp(_currentHp, _maxHp);

        if (_currentHp <= 0) Die();
    }

    // ── Hồi máu ──────────────────────────────────────────────
    public void Heal(int amount)
    {
        int before = _currentHp;
        _currentHp = Mathf.Min(_maxHp, _currentHp + amount);
        int actual = _currentHp - before;
        Debug.Log($"[HP] {Data.thingName} hồi {actual} HP: {_currentHp}/{_maxHp}");
        hpBar?.SetHp(_currentHp, _maxHp);
    }

    // ── Sau khi di chuyển ────────────────────────────────────────
    public void OnMoved()
    {
        if (WeatherManager.Instance.IsMagneticFieldActive(TeamId))
        {
            _moveCountThisCycle++;
            if (_moveCountThisCycle >= 3)
                _moveCountThisCycle = 0;
        }
        else
        {
            _moveCountThisCycle = 0;   // Từ Trường hết → reset
        }
    }

    // ── AI: cập nhật hướng di chuyển (gọi TRƯỚC khi move thật) ──
    public void TrackMovement(GridPos newPos)
    {
        LastMoveDir = new GridPos(
            newPos.col > GridPos.col ? 1 : newPos.col < GridPos.col ? -1 : 0,
            newPos.row > GridPos.row ? 1 : newPos.row < GridPos.row ? -1 : 0
        );
        LastGridPos = GridPos;
    }

    // ── AI: tăng đếm buff / lượt ─────────────────────────────────
    public void IncrementBuffCount() => AIBuffCount++;
    public void IncrementTurnCount() => TurnCount++;

    // ── Từ Trường ─────────────────────────────────────────────────
    public bool IsForcedStillByMagnet()
    {
        if (!WeatherManager.Instance.IsMagneticFieldActive(TeamId))
            return false;
        return _moveCountThisCycle == 2;
    }

    // ── Chết ──────────────────────────────────────────────────────
    private bool _isDead = false;
    public bool IsDead => _isDead;
    void Die()
    {
        _isDead = true;
        Debug.Log($"[Battle] {Data.thingName} bị hạ!");
        BattleGridManager.Instance.RemoveEntity(GridPos);
        hpBar?.gameObject.SetActive(false);
        // Không Destroy ngay — để EndJudgePhase() dọn dẹp sau
    }
}