// Assets/_Project/Scripts/Combat/EnemyAI.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sprint 5: Enemy AI thực sự.
/// Đánh giá target theo TypeChart rồi di chuyển đến vị trí tối ưu.
/// </summary>
public static class EnemyAI
{
    /// <summary>
    /// Quyết định lệnh cho một enemy entity.
    /// Ưu tiên tấn công target khắc hệ; nếu bằng nhau chọn HP thấp nhất.
    /// </summary>
    public static BattleCommand DecideCommand(
        BattleEntity       enemy,
        List<BattleEntity> playerTargets,
        BattleGridManager  grid)
    {
        if (playerTargets == null || playerTargets.Count == 0)
            return BattleCommand.MoveOnly(enemy.GridPos, enemy.GridPos);

        // 1. Chọn target tốt nhất ─────────────────────────────────────
        BattleEntity bestTarget = null;
        float        bestScore  = float.MinValue;

        ElementType moveType = enemy.Data.defaultMove != null
            ? enemy.Data.defaultMove.elementType
            : enemy.Data.elementType;

        foreach (var t in playerTargets)
        {
            if (t == null || t.CurrentHp <= 0) continue;

            // Type multiplier nếu dùng move này tấn công t
            float typeMult = CombatCalculator.GetTypeMultiplier(moveType, t.Data.elementType);

            // Thưởng điểm cho HP thấp (higher priority = lower HP)
            float hpRatio = (float)t.CurrentHp / Mathf.Max(1, t.Data.hp);
            float score   = typeMult * 10f + (1f - hpRatio) * 5f;

            if (score > bestScore)
            {
                bestScore  = score;
                bestTarget = t;
            }
        }

        if (bestTarget == null)
            return BattleCommand.MoveOnly(enemy.GridPos, enemy.GridPos);

        // 2. Tìm ô di chuyển gần target nhất trong vùng di chuyển ─────
        var    cfg         = grid.config;
        GridPos bestMovePos = enemy.GridPos;
        int    minDist     = int.MaxValue;

        for (int dc = -enemy.MoveRange; dc <= enemy.MoveRange; dc++)
        {
            for (int dr = -enemy.MoveRange; dr <= enemy.MoveRange; dr++)
            {
                int c = enemy.GridPos.col + dc;
                int r = enemy.GridPos.row + dr;

                if (!cfg.IsWalkable(c, r))            continue;
                if (cfg.GetTeam(c) != enemy.TeamId)   continue; // stay in own territory

                var pos = new GridPos(c, r);
                if (grid.IsOccupied(pos) && pos != enemy.GridPos) continue;

                int dist = GridPos.ManhattanDist(pos, bestTarget.GridPos);
                if (dist < minDist)
                {
                    minDist      = dist;
                    bestMovePos  = pos;
                }
            }
        }

        return BattleCommand.MoveAndAttack(bestMovePos, bestTarget.GridPos);
    }
}
