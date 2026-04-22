using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class EnemyAI
{
    public static BattleCommand DecideCommand(
        BattleEntity enemy,
        List<BattleEntity> playerThings,
        BattleGridManager grid)
    {
        return enemy.Data.aiPersonality switch
        {
            AIPersonality.Aggressive => AggressiveAI(enemy, playerThings, grid),
            AIPersonality.Defensive  => DefensiveAI(enemy, playerThings, grid),
            AIPersonality.Tactical   => TacticalAI(enemy, playerThings, grid),
            AIPersonality.Coward     => CowardAI(enemy, playerThings, grid),
            _ => StandStill(enemy)
        };
    }

    // ─── AGGRESSIVE: lao vào target HP thấp nhất ───────────────────
    static BattleCommand AggressiveAI(BattleEntity enemy,
        List<BattleEntity> targets, BattleGridManager grid)
    {
        var target = targets.OrderBy(t => t.CurrentHp).First();
        var movePos = GetClosestMovePos(enemy, target, grid);
        return new BattleCommand { entity = enemy, moveTarget = movePos, attackTarget = target.GridPos };
    }

    // ─── DEFENSIVE: giữ khoảng cách 2 ô, ưu tiên AoE ──────────────
    static BattleCommand DefensiveAI(BattleEntity enemy,
        List<BattleEntity> targets, BattleGridManager grid)
    {
        var target = targets.OrderBy(t => t.CurrentHp).First();

        // Cố giữ khoảng cách 2 ô so với target
        var moveCells = grid.GetMovableRange(enemy);
        GridPos bestMove = moveCells
            .OrderBy(pos => Mathf.Abs(ManhattanDist(pos, target.GridPos) - 2))
            .FirstOrDefault();

        if (bestMove.Equals(default)) bestMove = enemy.GridPos;

        return new BattleCommand { entity = enemy, moveTarget = bestMove, attackTarget = target.GridPos };
    }

    // ─── TACTICAL: ưu tiên target bị khắc hệ ──────────────────────
    static BattleCommand TacticalAI(BattleEntity enemy,
        List<BattleEntity> targets, BattleGridManager grid)
    {
        // Tìm target bị khắc hệ, nếu không có thì chọn HP thấp nhất
        var weakTarget = targets
            .Where(t => CombatCalculator.GetTypeEffectiveness(
                enemy.Data.elementType, t.Data.elementType) > 1f)
            .OrderBy(t => t.CurrentHp)
            .FirstOrDefault()
            ?? targets.OrderBy(t => t.CurrentHp).First();

        var movePos = GetClosestMovePos(enemy, weakTarget, grid);
        return new BattleCommand { entity = enemy, moveTarget = movePos, attackTarget = weakTarget.GridPos };
    }

    // ─── COWARD: bỏ chạy nếu HP < 30%, không thì tấn công ─────────
    static BattleCommand CowardAI(BattleEntity enemy,
        List<BattleEntity> targets, BattleGridManager grid)
    {
        bool isLowHp = (float)enemy.CurrentHp / enemy.MaxHp < 0.3f;

        var moveCells = grid.GetMovableRange(enemy);
        GridPos movePos;

        if (isLowHp)
        {
            // Bỏ chạy: tìm ô xa nhất so với tất cả target
            movePos = moveCells
                .OrderByDescending(pos => targets.Sum(t => ManhattanDist(pos, t.GridPos)))
                .FirstOrDefault();
        }
        else
        {
            // HP còn nhiều: hành xử như Aggressive
            var target = targets.OrderBy(t => t.CurrentHp).First();
            movePos = GetClosestMovePos(enemy, target, grid);
        }

        var attackTarget = targets.OrderBy(t => t.CurrentHp).First();
        return new BattleCommand { entity = enemy, moveTarget = movePos, attackTarget = attackTarget.GridPos };
    }

    // ─── HELPERS ────────────────────────────────────────────────────
    static GridPos GetClosestMovePos(BattleEntity enemy, BattleEntity target, BattleGridManager grid)
    {
        var moveCells = grid.GetMovableRange(enemy);
        if (moveCells.Count == 0) return enemy.GridPos;
        return moveCells
            .OrderBy(pos => ManhattanDist(pos, target.GridPos))
            .First();
    }

    static int ManhattanDist(GridPos a, GridPos b)
        => Mathf.Abs(a.col - b.col) + Mathf.Abs(a.row - b.row);

    static BattleCommand StandStill(BattleEntity enemy)
        => new BattleCommand { entity = enemy, moveTarget = enemy.GridPos, attackTarget = enemy.GridPos };
}