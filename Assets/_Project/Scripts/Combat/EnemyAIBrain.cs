using System.Collections.Generic;
using UnityEngine;

public static class EnemyAIBrain
{
    // ── Entry point duy nhất ────────────────────────────────────────
    public static BattleCommand Decide(BattleEntity enemy, List<BattleEntity> players)
{
    if (players.Count == 0)
        return BattleCommand.MoveOnly(enemy.GridPos, enemy.GridPos);

    AIDifficulty   diff      = enemy.Data.aiDifficulty;
    ThingArchetype archetype = enemy.Data.archetype;

    MoveData chosenMove  = PickMove(enemy, players, diff, archetype);
    GridPos  moveTarget  = PickMoveTarget(enemy, players, diff, archetype);
    GridPos  attackTarget = chosenMove != null
        ? PickAttackTarget(enemy, players, diff, archetype, moveTarget, chosenMove)
        : new GridPos(-1, -1); // null move = bỏ lượt tấn công

    enemy.SetChosenMove(chosenMove);

    return (chosenMove != null && attackTarget.col >= 0)
        ? BattleCommand.MoveAndAttack(moveTarget, attackTarget)
        : BattleCommand.MoveOnly(enemy.GridPos, moveTarget);
}

    // ════════════════════════════════════════════════════════════════
    // CHỌN CHIÊU — theo Archetype trước, Difficulty filter sau
    // ════════════════════════════════════════════════════════════════
    static MoveData PickMove(BattleEntity enemy, List<BattleEntity> players,
                             AIDifficulty diff, ThingArchetype archetype)
    {
        var moves = enemy.Data.moves;
        if (moves == null || moves.Count == 0) return enemy.Data.defaultMove;

        switch (archetype)
        {
            case ThingArchetype.Attacker:
                // 85% chiêu damage cao nhất, 15% random
                if (Random.value < 0.85f)
                    return HighestDamageMove(moves);
                return moves[Random.Range(0, moves.Count)];

            case ThingArchetype.Defender:
    if (enemy.TurnCount % 3 == 0 && enemy.TurnCount > 0) // ← đổi từ % 3 == 2
    {
        var buffMove = FindStatusMove(moves, "def");
        if (buffMove != null) return buffMove;
    }
    return PickMoveByDifficulty(enemy, players, moves, diff);

            case ThingArchetype.Setup:
                // 2 lượt đầu: luôn dùng buff/debuff
                if (enemy.TurnCount < 2)
                {
                    var statusMove = FindStatusMove(moves, "any");
                    if (statusMove != null) return statusMove;
                }
                return PickMoveByDifficulty(enemy, players, moves, diff);

            default:
                return PickMoveByDifficulty(enemy, players, moves, diff);
        }
    }

    static MoveData PickMoveByDifficulty(BattleEntity enemy, List<BattleEntity> players,
                                          List<MoveData> moves, AIDifficulty diff)
    {
        switch (diff)
        {
            case AIDifficulty.Easy:
                // Luôn chiêu power thấp nhất, 15% bỏ lượt
                if (Random.value < 0.15f) return null; // null = bỏ lượt
                return LowestPowerMove(moves);

            case AIDifficulty.Medium:
                return moves[Random.Range(0, moves.Count)];

            case AIDifficulty.Hard:
            case AIDifficulty.Ultra:
                // Ưu tiên khắc hệ, không có thì power cao nhất
                var typeMove = FindSuperEffectiveMove(moves, players);
                return typeMove ?? HighestDamageMove(moves);

            default:
                return moves[0];
        }
    }

    // ════════════════════════════════════════════════════════════════
    // CHỌN MOVE TARGET — theo Difficulty + Archetype Defender override
    // ════════════════════════════════════════════════════════════════
    static GridPos PickMoveTarget(BattleEntity enemy, List<BattleEntity> players,
                                   AIDifficulty diff, ThingArchetype archetype)
    {
        var reachable = GetReachable(enemy);

        // Defender: ở MỌI difficulty đều né terrain + né AoE player
        bool avoidAoE = (archetype == ThingArchetype.Defender);

        // Lọc terrain bất lợi (mọi difficulty)
        var safe = FilterOutHarmfulTerrain(reachable, enemy.TeamId);
        if (safe.Count == 0) safe = reachable; // fallback nếu toàn terrain

        switch (diff)
        {
            case AIDifficulty.Easy:
                return PickEasyMove(enemy, safe);

            case AIDifficulty.Medium:
                return safe[Random.Range(0, safe.Count)];

            case AIDifficulty.Hard:
                return PickHardMove(enemy, players, safe, avoidAoE);

            case AIDifficulty.Ultra:
                return PickUltraMove(enemy, players, safe);

            default:
                return safe.Count > 0 ? safe[0] : enemy.GridPos;
        }
    }

    // EASY: chọn 1 hướng cố định đầu battle, 80% giữ, 20% đổi
    static GridPos PickEasyMove(BattleEntity enemy, List<GridPos> reachable)
    {
        // Dùng TurnCount 0 để init hướng
        // Thực tế nên lưu fixedDir vào enemy, đây là approximation
        if (Random.value < 0.2f || reachable.Count == 1)
            return reachable[Random.Range(0, reachable.Count)];

        // Ưu tiên ô cùng hướng col với vị trí hiện tại (giữ hướng)
        GridPos cur = enemy.GridPos;
        var sameRow = reachable.FindAll(p => p.row == cur.row);
        return sameRow.Count > 0
            ? sameRow[Random.Range(0, sameRow.Count)]
            : reachable[Random.Range(0, reachable.Count)];
    }

    // HARD: tính hướng từ enemy → predicted player pos
    static GridPos PickHardMove(BattleEntity enemy, List<BattleEntity> players,
                                 List<GridPos> reachable, bool avoidAoEForDefender)
    {
        BattleEntity target = NearestPlayer(enemy, players);
        // Predicted pos của player = currentPos + lastMoveDir
        GridPos predicted = new GridPos(
            target.GridPos.col + target.LastMoveDir.col,
            target.GridPos.row + target.LastMoveDir.row
        );

        // 35% đi về hướng predicted, 65% random trong reachable
        if (Random.value < 0.35f)
        {
            var toward = ClosestTo(reachable, predicted);
            return toward;
        }
        return reachable[Random.Range(0, reachable.Count)];
    }

    // ULTRA: biết trước AoE player lượt này → 85% tránh, 15% vào
    static GridPos PickUltraMove(BattleEntity enemy, List<BattleEntity> players,
                                  List<GridPos> reachable)
    {
        // Tính AoE thật của player (dùng command đã submit)
        var playerAoECells = GetAllPlayerAoECells(players);

        var outsideAoE = reachable.FindAll(p => !playerAoECells.Contains(p));
        var insideAoE  = reachable.FindAll(p => playerAoECells.Contains(p));

        if (Random.value < 0.85f)
        {
            // Tránh AoE: chọn ô tốt nhất ngoài AoE (gần nhất với player)
            if (outsideAoE.Count > 0)
            {
                BattleEntity target = NearestPlayer(enemy, players);
                return ClosestTo(outsideAoE, target.GridPos);
            }
        }

        // 15% vào AoE: chọn ô có lợi nhất trong vùng bị trúng
        if (insideAoE.Count > 0)
            return insideAoE[Random.Range(0, insideAoE.Count)];

        return reachable.Count > 0 ? reachable[0] : enemy.GridPos;
    }

    // ════════════════════════════════════════════════════════════════
    // CHỌN ATTACK TARGET — Difficulty + Setup override
    // ════════════════════════════════════════════════════════════════
    static GridPos PickAttackTarget(BattleEntity enemy, List<BattleEntity> players,
                                     AIDifficulty diff, ThingArchetype archetype,
                                     GridPos fromPos, MoveData chosenMove)
    {
        // 2vs2: nếu có player HP ngưỡng sắp chết VÀ chiêu khắc hệ → kết liễu ngay
        BattleEntity finishTarget = FindFinishTarget(enemy, players, chosenMove);
        if (finishTarget != null) return finishTarget.GridPos;

        // Setup: chiêu buff/debuff có target logic riêng
        if (archetype == ThingArchetype.Setup && chosenMove != null
            && chosenMove.category == MoveCategory.Status)
        {
            return PickSetupTarget(enemy, players, chosenMove);
        }

        var attackable = GetAttackable(fromPos, enemy.TeamId);

        switch (diff)
        {
            case AIDifficulty.Easy:
                // 70% bắn ô hiện tại player, 30% random trong MoveRange player
                if (Random.value < 0.70f)
                {
                    BattleEntity t = LowestHpPlayer(players);
                    if (attackable.Contains(t.GridPos)) return t.GridPos;
                }
                return RandomPlayerAreaCell(players, attackable);

            case AIDifficulty.Medium:
                // 50% ô hiện tại, 50% random
                if (Random.value < 0.50f)
                {
                    BattleEntity t = LowestHpPlayer(players);
                    if (attackable.Contains(t.GridPos)) return t.GridPos;
                }
                return RandomPlayerAreaCell(players, attackable);

            case AIDifficulty.Hard:
                // Predicted position
                BattleEntity hardTarget = LowestHpPlayer(players);
                GridPos predicted = new GridPos(
                    hardTarget.GridPos.col + hardTarget.LastMoveDir.col,
                    hardTarget.GridPos.row + hardTarget.LastMoveDir.row
                );
                // AoE: tìm tâm cover nhiều ô MoveRange player nhất
                if (chosenMove != null && chosenMove.shape != AttackShape.Single)
                    return BestAoECenter(fromPos, players, chosenMove, attackable);
                return attackable.Contains(predicted) ? predicted : hardTarget.GridPos;

            case AIDifficulty.Ultra:
                // 85% bắn ô player sẽ đến, 15% bắn ô hiện tại
                BattleEntity ultraTarget = LowestHpPlayer(players);
                GridPos willMove = new GridPos(
                    ultraTarget.GridPos.col + ultraTarget.LastMoveDir.col,
                    ultraTarget.GridPos.row + ultraTarget.LastMoveDir.row
                );
                if (Random.value < 0.85f && attackable.Contains(willMove))
                    return willMove;
                return ultraTarget.GridPos;

            default:
                return LowestHpPlayer(players).GridPos;
        }
    }

    // Setup target: chiêu buff/debuff nhắm đúng mục tiêu
    static GridPos PickSetupTarget(BattleEntity enemy, List<BattleEntity> players, MoveData move)
    {
        // Debuff → nhắm player stat cao nhất tương ứng
        // Đơn giản hóa: nhắm player HP cao nhất (target cứng nhất)
        BattleEntity strongest = null;
        foreach (var p in players)
            if (strongest == null || p.CurrentHp > strongest.CurrentHp) strongest = p;

        return strongest?.GridPos ?? players[0].GridPos;
    }

    // ════════════════════════════════════════════════════════════════
    // HELPER FUNCTIONS
    // ════════════════════════════════════════════════════════════════

    static List<GridPos> GetReachable(BattleEntity enemy)
{
    var result = new List<GridPos>();
    var cfg    = BattleGridManager.Instance.config;
    GridPos origin = enemy.GridPos;
    int range = enemy.MoveRange;

    for (int dc = -range; dc <= range; dc++)
        for (int dr = -range; dr <= range; dr++)
        {
            if (Mathf.Abs(dc) + Mathf.Abs(dr) > range) continue; // ← THÊM
            int c = origin.col + dc, r = origin.row + dr;
            if (!cfg.IsWalkable(c, r)) continue;
            if (cfg.GetTeam(c) != enemy.TeamId) continue;
            var pos = new GridPos(c, r);
            if (BattleGridManager.Instance.IsOccupied(pos) && pos != origin) continue;
            result.Add(pos);
        }

    if (result.Count == 0) result.Add(origin);
    return result;
}

    static List<GridPos> GetAttackable(GridPos from, int teamId)
    {
        var result = new List<GridPos>();
        var cfg = BattleGridManager.Instance.config;
        int colStart = teamId == 0 ? cfg.RightMinCol : 0;
        int colEnd   = teamId == 0 ? cfg.TotalCols - 1 : cfg.LeftMaxCol;
        for (int c = colStart; c <= colEnd; c++)
            for (int r = 0; r < cfg.boardRows; r++)
                result.Add(new GridPos(c, r));
        return result;
    }

    static List<GridPos> FilterOutHarmfulTerrain(List<GridPos> cells, int teamId)
    {
        var safe = new List<GridPos>();
        foreach (var p in cells)
        {
            var cell = TerrainManager.Instance.GetCell(p);
            bool harmful = cell != null && cell.effectType != TerrainEffectType.None;
            if (!harmful) safe.Add(p);
        }
        return safe;
    }

    static HashSet<GridPos> GetAllPlayerAoECells(List<BattleEntity> players)
{
    var result = new HashSet<GridPos>();
    foreach (var p in players)
    {
        MoveData move = p.GetMove(); // ← đổi từ p.Data.defaultMove
        if (move == null) continue;
        var aoe = BattleGridManager.Instance.GetAoECells(
            p.GridPos, move.shape, p.GridPos, move.aoeRadius);
        foreach (var cell in aoe) result.Add(cell);
    }
    return result;
}

    static BattleEntity FindFinishTarget(BattleEntity enemy, List<BattleEntity> players, MoveData move)
    {
        if (move == null) return null;
        float hpThreshold = 0.25f; // dưới 25% HP = "sắp chết"
        foreach (var p in players)
        {
            bool lowHp = (float)p.CurrentHp / p.MaxHp <= hpThreshold;
            float typeMult = CombatCalculator.GetTypeMultiplier(move.elementType, p.Data.elementType);
            if (lowHp && typeMult >= 2.0f) return p;
        }
        return null;
    }

    static GridPos BestAoECenter(GridPos from, List<BattleEntity> players, MoveData move, List<GridPos> attackable)
    {
        GridPos best = players[0].GridPos;
        int bestCount = 0;
        foreach (var cell in attackable)
        {
            var aoe = BattleGridManager.Instance.GetAoECells(cell, move.shape, from, move.aoeRadius);
            int count = 0;
            foreach (var p in players)
                if (aoe.Contains(p.GridPos)) count++;
            if (count > bestCount) { bestCount = count; best = cell; }
        }
        return best;
    }

    static GridPos RandomPlayerAreaCell(List<BattleEntity> players, List<GridPos> attackable)
    {
        var inRange = new List<GridPos>();
        foreach (var p in players)
        {
            int range = p.MoveRange;
            for (int dc = -range; dc <= range; dc++)
                for (int dr = -range; dr <= range; dr++)
                {
                    var c = new GridPos(p.GridPos.col + dc, p.GridPos.row + dr);
                    if (attackable.Contains(c)) inRange.Add(c);
                }
        }
        return inRange.Count > 0
            ? inRange[Random.Range(0, inRange.Count)]
            : attackable.Count > 0 ? attackable[Random.Range(0, attackable.Count)] : new GridPos(-1, -1);
    }

    static BattleEntity NearestPlayer(BattleEntity enemy, List<BattleEntity> players)
    {
        BattleEntity nearest = null;
        int best = int.MaxValue;
        foreach (var p in players)
        {
            int d = Mathf.Abs(p.GridPos.col - enemy.GridPos.col)
                  + Mathf.Abs(p.GridPos.row - enemy.GridPos.row);
            if (d < best) { best = d; nearest = p; }
        }
        return nearest;
    }

    static BattleEntity LowestHpPlayer(List<BattleEntity> players)
    {
        BattleEntity result = null;
        foreach (var p in players)
            if (result == null || p.CurrentHp < result.CurrentHp) result = p;
        return result;
    }

    static GridPos ClosestTo(List<GridPos> cells, GridPos target)
    {
        GridPos best = cells[0];
        int bestDist = int.MaxValue;
        foreach (var c in cells)
        {
            int d = Mathf.Abs(c.col - target.col) + Mathf.Abs(c.row - target.row);
            if (d < bestDist) { bestDist = d; best = c; }
        }
        return best;
    }

    static MoveData HighestDamageMove(List<MoveData> moves)
    {
        MoveData best = null;
        foreach (var m in moves)
            if (m.category != MoveCategory.Status && m.category != MoveCategory.Environment)
                if (best == null || m.basePower > best.basePower) best = m;
        return best ?? moves[0];
    }

    static MoveData LowestPowerMove(List<MoveData> moves)
    {
        MoveData best = null;
        foreach (var m in moves)
            if (m.category != MoveCategory.Status && m.category != MoveCategory.Environment)
                if (best == null || m.basePower < best.basePower) best = m;
        return best ?? moves[0];
    }

    static MoveData FindStatusMove(List<MoveData> moves, string hint)
    {
        foreach (var m in moves)
            if (m.category == MoveCategory.Status) return m;
        return null;
    }

    static MoveData FindSuperEffectiveMove(List<MoveData> moves, List<BattleEntity> players)
    {
        MoveData best = null;
        float bestMult = 1f;
        foreach (var m in moves)
        {
            if (m.category == MoveCategory.Status || m.category == MoveCategory.Environment) continue;
            foreach (var p in players)
            {
                float mult = CombatCalculator.GetTypeMultiplier(m.elementType, p.Data.elementType);
                if (mult > bestMult) { bestMult = mult; best = m; }
            }
        }
        return best;
    }
}