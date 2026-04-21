using System.Collections;
using UnityEngine;

public class BattleEntity : MonoBehaviour
{
    [HideInInspector] public GridPos GridPos;
    [HideInInspector] public int TeamId;
    [HideInInspector] public EntityHpBar hpBar;

    public int Speed      { get; private set; }
    public int MoveRange  { get; private set; }
    public ThingData Data { get; private set; }
    public int Level      => Data != null ? Data.level : 1;

    private int _currentHp;
    public int CurrentHp => _currentHp;
    private int _maxHp;

    // ── Stat Stages (-6..+6), index = (int)StatEffectTarget ──────────
    private readonly int[] _statStages = new int[6]; // [0] unused; [1..5] = Atk/Def/SpAtk/SpDef/Spd

    // ── Status Effect ─────────────────────────────────────────────────
    public StatusEffect ActiveStatus { get; private set; } = StatusEffect.None;
    private int _statusTurnsLeft;

    // ── Synergy multipliers (set by SynergyManager after spawn) ───────
    public float SynergyAtkMult      { get; set; } = 1f;
    public float SynergyDefMult      { get; set; } = 1f;
    public float SynergySPAtkMult    { get; set; } = 1f;
    public float SynergyEvasionBonus { get; set; } = 0f;  // additive %
    public float SynergyRegenPercent { get; set; } = 0f;  // % MaxHP restored per turn

    // ─────────────────────────────────────────────────────────────────
    public void Init(ThingData data, int teamId)
    {
        Data      = data;
        TeamId    = teamId;
        _currentHp = data.hp;
        _maxHp     = data.hp;
        Speed      = (int)data.speed;
        MoveRange  = data.moveRange;
        Debug.Log($"[BattleEntity] Spawn {data.thingName} | Team {teamId} | HP {_currentHp} | Spd {Speed}");
    }

    /// <summary>Lấy move của entity (defaultMove từ ThingData).</summary>
    public MoveData GetMove() => Data != null ? Data.defaultMove : null;

    // ── Stat Stages ───────────────────────────────────────────────────
    public int GetStage(StatEffectTarget stat)
    {
        if (stat == StatEffectTarget.None) return 0;
        return _statStages[(int)stat];
    }

    public void ApplyStage(StatEffectTarget stat, int delta)
    {
        if (stat == StatEffectTarget.None) return;
        int idx = (int)stat;
        _statStages[idx] = Mathf.Clamp(_statStages[idx] + delta, -6, 6);
        string sign = _statStages[idx] >= 0 ? "+" : "";
        Debug.Log($"[Stage] {Data.thingName} {stat} = {sign}{_statStages[idx]}");
    }

    // ── Status Effect ─────────────────────────────────────────────────
    public void ApplyStatusEffect(StatusEffect effect, int turns = 3)
    {
        if (ActiveStatus != StatusEffect.None) return; // can't overwrite
        ActiveStatus      = effect;
        _statusTurnsLeft  = turns;
        Debug.Log($"[Status] {Data.thingName} bị {effect} trong {turns} lượt");
    }

    /// <summary>Gọi cuối lượt để tick burn/poison. Trả về true nếu có hiệu ứng.</summary>
    public bool TickStatus()
    {
        if (ActiveStatus == StatusEffect.None) return false;

        int dmg = 0;
        if (ActiveStatus == StatusEffect.Burn)   dmg = Mathf.Max(1, _maxHp / 16);
        if (ActiveStatus == StatusEffect.Poison) dmg = Mathf.Max(1, _maxHp / 8);

        if (dmg > 0) TakeDamage(dmg);

        _statusTurnsLeft--;
        if (_statusTurnsLeft <= 0)
        {
            Debug.Log($"[Status] {Data.thingName} hết {ActiveStatus}");
            ActiveStatus = StatusEffect.None;
        }
        return true;
    }

    // ── Damage ────────────────────────────────────────────────────────
    public void TakeDamage(int dmg, bool isCrit = false)
    {
        _currentHp = Mathf.Max(0, _currentHp - dmg);
        float hpPercent = (float)_currentHp / _maxHp * 100f;
        Debug.Log($"[HP] {Data.thingName}: {_currentHp}/{_maxHp} ({hpPercent:F0}%)");

        DamagePopup.Create(transform.position, dmg, isCrit);
        if (hpBar != null) hpBar.SetHp(_currentHp, _maxHp);
        if (_currentHp <= 0) Die();
    }

    /// <summary>Heal entity (Angel regen / Crystal cell). Does NOT overheal.</summary>
    public void HealHp(int amount)
    {
        if (_currentHp <= 0) return;
        _currentHp = Mathf.Min(_maxHp, _currentHp + amount);
        if (hpBar != null) hpBar.SetHp(_currentHp, _maxHp);
        Debug.Log($"[Heal] {Data.thingName}: +{amount} HP → {_currentHp}/{_maxHp}");
    }

    // ── Hit Animation: flash + knockback ─────────────────────────────
    /// <summary>
    /// Coroutine: hit flash (white → red) then knockback lerp.
    /// Yield this from BattlePhaseManager so combat waits for animation.
    /// </summary>
    public IEnumerator HitAnimation(Vector3 attackerWorldPos)
    {
        // null-guard: entity might have been destroyed between frames
        if (this == null) yield break;

        var sr           = GetComponent<SpriteRenderer>();
        Color origColor  = sr != null ? sr.color : Color.white;
        Vector3 origPos  = transform.position;

        // Flash: white then red tint
        if (sr != null)
        {
            sr.color = Color.white;
            yield return new WaitForSeconds(0.05f);
            if (this == null) yield break;
            sr.color = new Color(1f, 0.3f, 0.3f, 1f);
            yield return new WaitForSeconds(0.05f);
            if (this == null) yield break;
        }

        // Knockback: move slightly away from attacker, then return
        Vector3 dir = transform.position - attackerWorldPos;
        if (dir.sqrMagnitude < 0.0001f) dir = Vector3.right;
        dir.Normalize();
        Vector3 knockPos = origPos + dir * 0.18f;

        float t = 0f;
        while (t < 1f)
        {
            if (this == null) yield break;
            t = Mathf.Min(1f, t + Time.deltaTime / 0.08f);
            transform.position = Vector3.Lerp(origPos, knockPos, t);
            yield return null;
        }
        t = 0f;
        while (t < 1f)
        {
            if (this == null) yield break;
            t = Mathf.Min(1f, t + Time.deltaTime / 0.08f);
            transform.position = Vector3.Lerp(knockPos, origPos, t);
            yield return null;
        }

        if (this != null)
        {
            transform.position = origPos;
            if (sr != null) sr.color = origColor;
        }
    }

    // ── Death ─────────────────────────────────────────────────────────
    void Die()
    {
        Debug.Log($"[Battle] {Data.thingName} bị hạ!");

        // Legacy tribe effects (Sprint 9)
        if (SynergyManager.Instance != null)
        {
            if (Data.tribeType == TribeType.Colossus)
                SynergyManager.Instance.OnColossusKilled(TeamId);
            else if (Data.tribeType == TribeType.Gunner)
                SynergyManager.Instance.OnGunnerKilled(TeamId, 1 - TeamId);
        }

        BattleGridManager.Instance.RemoveEntity(GridPos);
        Destroy(gameObject);
    }
}