using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Sprint 3+: Kiểm tra kết thúc trận.
/// Sprint 7+: Trao EXP, tính sao, lưu inventory, hiện kết quả + Gacha.
/// </summary>
public class BattleResultManager : MonoBehaviour
{
    [SerializeField] BattleResultPanel resultPanel;
    [SerializeField] GachaManager      gachaManager;

    // ── Public API ─────────────────────────────────────────────────
    /// <summary>
    /// Gọi từ BattlePhaseManager sau judge phase.
    /// Trả về true nếu trận đã kết thúc (bắt đầu coroutine result).
    /// </summary>
    public bool CheckBattleEnd(Dictionary<BattleEntity, int> hitCounts = null)
    {
        var allEntities = FindObjectsByType<BattleEntity>(FindObjectsInactive.Exclude);

        var team0 = new List<BattleEntity>();
        var team1 = new List<BattleEntity>();
        foreach (var e in allEntities)
        {
            if (e.TeamId == 0) team0.Add(e);
            else               team1.Add(e);
        }

        bool team0Dead = team0.Count == 0 || team0.TrueForAll(e => e.CurrentHp <= 0);
        bool team1Dead = team1.Count == 0 || team1.TrueForAll(e => e.CurrentHp <= 0);

        if (!team0Dead && !team1Dead) return false;

        bool isWin = team1Dead && !team0Dead;
        StartCoroutine(HandleBattleEnd(isWin));
        return true;
    }

    // ── Result Flow ────────────────────────────────────────────────
    IEnumerator HandleBattleEnd(bool isWin)
    {
        // ── 1. Collect defeated enemy data from BattlePhaseManager ──
        var defeated    = BattlePhaseManager.Instance?.DefeatedEnemies ?? new List<BattlePhaseManager.DefeatedRecord>();
        int totalExp    = 0;
        int totalStars  = 0;
        var materials   = new Dictionary<string, int>();

        foreach (var rec in defeated)
        {
            totalExp   += rec.expYield;
            int stars   = CalculateStars(rec.hitsTaken);
            totalStars  = Mathf.Max(totalStars, stars); // use best clean kill

            // Inventory: defeated enemy name as material, quantity = stars
            if (!materials.ContainsKey(rec.name)) materials[rec.name] = 0;
            materials[rec.name] += stars;
        }

        // If no recorded defeats (e.g. player lost), leave totalExp = 0
        int awardedExp = isWin ? totalExp : Mathf.Max(0, totalExp / 4); // consolation EXP

        // ── 2. Award EXP + Gacha level up (win only) ───────────────
        if (isWin)
        {
            foreach (var member in RuntimeGameState.Party)
            {
                bool leveledUp = LevelUpSystem.AddExp(member, awardedExp);

                if (leveledUp && gachaManager != null)
                {
                    var options = LevelUpSystem.GetGachaOptions(member, 3);
                    yield return StartCoroutine(gachaManager.ShowGacha(member, options));
                }
            }

            // ── 3. Save materials to Inventory ─────────────────────
            foreach (var kvp in materials)
            {
                if (!RuntimeGameState.Inventory.ContainsKey(kvp.Key))
                    RuntimeGameState.Inventory[kvp.Key] = 0;
                RuntimeGameState.Inventory[kvp.Key] += kvp.Value;
            }
        }

        // ── 4. Show Result Panel ────────────────────────────────────
        if (resultPanel != null)
        {
            resultPanel.Show(isWin, awardedExp, totalStars, isWin ? materials : null);
            yield return new WaitUntil(() => resultPanel.IsDone);
        }
        else
        {
            yield return new WaitForSeconds(1.5f);
        }

        SceneManager.LoadScene("OverworldScene");
    }

    // ── Stars ──────────────────────────────────────────────────────
    /// <summary>
    /// 1 hit   → 3 ★  (Clean Kill)
    /// 2 hits  → 2 ★
    /// 3+ hits → 1 ★
    /// </summary>
    static int CalculateStars(int hitsToKill)
    {
        if (hitsToKill <= 1) return 3;
        if (hitsToKill <= 2) return 2;
        return 1;
    }
}