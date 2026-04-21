// Assets/_Project/Scripts/Combat/SynergyManager.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sprint 9: Tộc Synergy — tính passive buff theo số lượng cùng tộc trong party.
/// Attach vào BattleManager GameObject.
/// Gọi EvaluateSynergies() sau khi tất cả entity spawn (BattleManager.Start).
/// </summary>
public class SynergyManager : MonoBehaviour
{
    public static SynergyManager Instance { get; private set; }

    // ── Legacy buffs: key = teamId ────────────────────────────────
    private readonly Dictionary<int, LegacyBuff> _legacyBuffs = new Dictionary<int, LegacyBuff>();

    void Awake() => Instance = this;

    // ── Synergy Evaluation ────────────────────────────────────────
    /// <summary>Gọi sau khi spawn để áp multiplier synergy lên từng entity.</summary>
    public void EvaluateSynergies()
    {
        var entities = FindObjectsByType<BattleEntity>(FindObjectsInactive.Exclude);

        var team0 = new List<BattleEntity>();
        var team1 = new List<BattleEntity>();
        foreach (var e in entities)
        {
            if (e.TeamId == 0) team0.Add(e);
            else               team1.Add(e);
        }

        ApplySynergyToTeam(team0);
        ApplySynergyToTeam(team1);
    }

    void ApplySynergyToTeam(List<BattleEntity> team)
    {
        if (team.Count == 0) return;

        // Đếm từng tộc trong team
        var counts = new Dictionary<TribeType, int>();
        foreach (var e in team)
        {
            var t = e.Data.tribeType;
            if (t == TribeType.None) continue;
            counts[t] = counts.TryGetValue(t, out int n) ? n + 1 : 1;
        }

        foreach (var e in team)
        {
            // Beast ×2 → +10% ATK
            if (HasSynergy(counts, TribeType.Beast, 2))
                e.SynergyAtkMult *= 1.1f;

            // Warrior ×2 → +10% ATK
            if (HasSynergy(counts, TribeType.Warrior, 2))
                e.SynergyAtkMult *= 1.1f;

            // Mechanical ×2 → +10% DEF
            if (HasSynergy(counts, TribeType.Mechanical, 2))
                e.SynergyDefMult *= 1.1f;

            // Ancient ×2 → +10% SP.ATK
            if (HasSynergy(counts, TribeType.Ancient, 2))
                e.SynergySPAtkMult *= 1.1f;

            // Supernatural ×2 → +5% evasion
            if (HasSynergy(counts, TribeType.Supernatural, 2))
                e.SynergyEvasionBonus += 5f;

            // Angel ×2 → 2% HP regen per turn
            if (HasSynergy(counts, TribeType.Angel, 2))
                e.SynergyRegenPercent += 2f;

            // Legendary ×1 → +15% ATK & DEF
            if (HasSynergy(counts, TribeType.Legendary, 1))
            {
                e.SynergyAtkMult *= 1.15f;
                e.SynergyDefMult *= 1.15f;
            }

            // Log applied synergies
            if (e.SynergyAtkMult > 1f || e.SynergyDefMult > 1f || e.SynergyEvasionBonus > 0f)
                Debug.Log($"[Synergy] {e.Data.thingName}: atkMult={e.SynergyAtkMult:F2} " +
                          $"defMult={e.SynergyDefMult:F2} evasion+{e.SynergyEvasionBonus}%");
        }
    }

    static bool HasSynergy(Dictionary<TribeType, int> counts, TribeType tribe, int required)
        => counts.TryGetValue(tribe, out int n) && n >= required;

    // ── Legacy Effects ────────────────────────────────────────────

    /// <summary>Colossus KO → đồng đội nhận +20% DEF trong 4 lượt.</summary>
    public void OnColossusKilled(int teamId)
    {
        _legacyBuffs[teamId] = new LegacyBuff { defBonus = 0.2f, bombDmg = 0, turnsLeft = 4 };
        Debug.Log($"[Legacy] Colossus team {teamId} KO → đồng đội +20% DEF 4 lượt");

        // Apply immediately to surviving allies
        foreach (var e in FindObjectsByType<BattleEntity>(FindObjectsInactive.Exclude))
            if (e.TeamId == teamId) e.SynergyDefMult += 0.2f;
    }

    /// <summary>Gunner KO → kẻ địch chịu 10 damage bomb mỗi lượt trong 5 lượt.</summary>
    public void OnGunnerKilled(int killedTeamId, int enemyTeamId)
    {
        _legacyBuffs[enemyTeamId] = new LegacyBuff { defBonus = 0f, bombDmg = 10, turnsLeft = 5 };
        Debug.Log($"[Legacy] Gunner team {killedTeamId} KO → team {enemyTeamId} chịu 10 bomb dmg/lượt");
    }

    /// <summary>Gọi cuối lượt từ BattlePhaseManager để tick legacy effects.</summary>
    public void ApplyLegacyEffects()
    {
        var toRemove = new List<int>();

        foreach (var kvp in _legacyBuffs)
        {
            int       teamId = kvp.Key;
            LegacyBuff buff  = kvp.Value;

            if (buff.bombDmg > 0)
            {
                foreach (var e in FindObjectsByType<BattleEntity>(FindObjectsInactive.Exclude))
                    if (e.TeamId == teamId) e.TakeDamage(buff.bombDmg);
            }

            buff.turnsLeft--;
            if (buff.turnsLeft <= 0)
                toRemove.Add(teamId);
            else
                _legacyBuffs[teamId] = buff;
        }

        foreach (var key in toRemove) _legacyBuffs.Remove(key);
    }

    /// <summary>DEF% bonus từ legacy Colossus (dùng trong BattlePhaseManager damage calc).</summary>
    public float GetLegacyDefBonus(int teamId)
        => _legacyBuffs.TryGetValue(teamId, out var buff) ? buff.defBonus : 0f;

    // ── Data ──────────────────────────────────────────────────────
    struct LegacyBuff
    {
        public float defBonus;
        public int   bombDmg;
        public int   turnsLeft;
    }
}
