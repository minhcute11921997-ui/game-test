using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class EnemyAIBrain
{
    public static BattleCommand Decide(BattleEntity enemy, List<BattleEntity> players)
    {
        if (players.Count == 0)
            return BattleCommand.MoveOnly(enemy.GridPos, enemy.GridPos);

        AIDifficulty diff = enemy.Data.aiDifficulty;
        ThingArchetype archetype = enemy.Data.archetype;

        MoveData chosenMove = PickMove(enemy, players, diff, archetype);
        GridPos moveTarget = PickMoveTarget(enemy, players, diff, archetype);
        GridPos attackTarget = chosenMove != null
            ? PickAttackTarget(enemy, players, diff, archetype, moveTarget, chosenMove)
            : new GridPos(-1, -1);

        enemy.SetChosenMove(chosenMove);

        Debug.Log($"[AI] {enemy.Data.thingName} | Diff:{enemy.Data.aiDifficulty} | Arch:{enemy.Data.archetype}");
        Debug.Log($"[AI] PickMove → {chosenMove?.moveName ?? "BỎ LƯỢT"}");
        Debug.Log($"[AI] MoveTarget:{moveTarget} | AttackTarget:{attackTarget}");

        return (chosenMove != null && attackTarget.col >= 0)
            ? BattleCommand.MoveAndAttack(moveTarget, attackTarget)
            : BattleCommand.MoveOnly(enemy.GridPos, moveTarget);
    }

    // ════════════════════════════════════════════════════════════════
    // CHỌN CHIÊU
    // ════════════════════════════════════════════════════════════════
    static MoveData PickMove(BattleEntity enemy, List<BattleEntity> players,
                             AIDifficulty diff, ThingArchetype archetype)
    {
        var moves = enemy.Data.moves;
        if (moves.Count == 0) return null;

        switch (archetype)
        {
            case ThingArchetype.Attacker:
                if (Random.value < 0.85f) return HighestDamageMove(moves);
                return moves[Random.Range(0, moves.Count)];

            case ThingArchetype.Defender:
                if (enemy.TurnCount % 3 == 0 && enemy.TurnCount > 0)
                {
                    var buffMove = FindStatusMove(moves, "def");
                    if (buffMove != null) return buffMove;
                }
                return PickMoveByDifficulty(enemy, players, moves, diff);

            case ThingArchetype.Setup:
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
                if (Random.value < 0.15f) return null;
                return LowestPowerMove(moves);

            case AIDifficulty.Medium:
                {
                    var usable = moves.FindAll(m =>
                        m.GetDamage() != null ||
                        m.effects.OfType<StatStageEffect>().Any() ||
                        m.effects.OfType<HealEffect>().Any() ||
                        IsUsefulEnvironmentMove(enemy, m));
                    if (usable.Count == 0) usable = moves;
                    return usable[Random.Range(0, usable.Count)];
                }

            case AIDifficulty.Hard:
            case AIDifficulty.Ultra:
                var typeMove = FindSuperEffectiveMove(moves, players);
                return typeMove ?? HighestDamageMove(moves);

            default:
                return moves[0];
        }
    }

    // ════════════════════════════════════════════════════════════════
    // CHỌN MOVE TARGET
    // ════════════════════════════════════════════════════════════════
    static GridPos PickMoveTarget(BattleEntity enemy, List<BattleEntity> players,
                                   AIDifficulty diff, ThingArchetype archetype)
    {
        var reachable = GetReachable(enemy);
        bool avoidAoE = (archetype == ThingArchetype.Defender);

        var safe = FilterOutHarmfulTerrain(reachable, enemy.TeamId);
        if (safe.Count == 0) safe = reachable;

        switch (diff)
        {
            case AIDifficulty.Easy: return PickEasyMove(enemy, safe);
            case AIDifficulty.Medium: return safe[Random.Range(0, safe.Count)];
            case AIDifficulty.Hard: return PickHardMove(enemy, players, safe, avoidAoE);
            case AIDifficulty.Ultra: return PickUltraMove(enemy, players, safe);
            default: return safe.Count > 0 ? safe[0] : enemy.GridPos;
        }
    }

    static GridPos PickEasyMove(BattleEntity enemy, List<GridPos> reachable)
    {
        if (Random.value < 0.2f || reachable.Count == 1)
            return reachable[Random.Range(0, reachable.Count)];

        GridPos cur = enemy.GridPos;
        var sameRow = reachable.FindAll(p => p.row == cur.row);
        return sameRow.Count > 0
            ? sameRow[Random.Range(0, sameRow.Count)]
            : reachable[Random.Range(0, reachable.Count)];
    }

    static GridPos PickHardMove(BattleEntity enemy, List<BattleEntity> players,
                                 List<GridPos> reachable, bool avoidAoEForDefender)
    {
        BattleEntity target = NearestPlayer(enemy, players);
        GridPos predicted = new GridPos(
            target.GridPos.col + target.LastMoveDir.col,
            target.GridPos.row + target.LastMoveDir.row);

        if (Random.value < 0.35f)
            return ClosestTo(reachable, predicted);
        return reachable[Random.Range(0, reachable.Count)];
    }

    static GridPos PickUltraMove(BattleEntity enemy, List<BattleEntity> players,
                                  List<GridPos> reachable)
    {
        var playerAoECells = GetAllPlayerAoECells(players);
        var outsideAoE = reachable.FindAll(p => !playerAoECells.Contains(p));
        var insideAoE = reachable.FindAll(p => playerAoECells.Contains(p));

        if (Random.value < 0.85f && outsideAoE.Count > 0)
        {
            BattleEntity target = NearestPlayer(enemy, players);
            return ClosestTo(outsideAoE, target.GridPos);
        }

        if (insideAoE.Count > 0) return insideAoE[Random.Range(0, insideAoE.Count)];
        return reachable.Count > 0 ? reachable[0] : enemy.GridPos;
    }

    // ════════════════════════════════════════════════════════════════
    // CHỌN ATTACK TARGET
    // ════════════════════════════════════════════════════════════════
    static GridPos PickAttackTarget(BattleEntity enemy, List<BattleEntity> players,
                                     AIDifficulty diff, ThingArchetype archetype,
                                     GridPos fromPos, MoveData chosenMove)
    {
        BattleEntity finishTarget = FindFinishTarget(enemy, players, chosenMove);
        if (finishTarget != null) return finishTarget.GridPos;

        if (archetype == ThingArchetype.Setup && chosenMove != null
    && chosenMove.GetDamage() == null && chosenMove.GetTerrain() == null && chosenMove.GetWeather() == null)
            return PickSetupTarget(enemy, players, chosenMove);

        var attackable = GetAttackable(fromPos, enemy.TeamId);

        switch (diff)
        {
            case AIDifficulty.Easy:
                if (Random.value < 0.70f)
                {
                    BattleEntity t = LowestHpPlayer(players);
                    if (attackable.Contains(t.GridPos)) return t.GridPos;
                }
                return RandomPlayerAreaCell(players, attackable);

            case AIDifficulty.Medium:
                if (Random.value < 0.50f)
                {
                    BattleEntity t = LowestHpPlayer(players);
                    if (attackable.Contains(t.GridPos)) return t.GridPos;
                }
                return RandomPlayerAreaCell(players, attackable);

            case AIDifficulty.Hard:
                BattleEntity hardTarget = LowestHpPlayer(players);
                GridPos predicted = new GridPos(
                    hardTarget.GridPos.col + hardTarget.LastMoveDir.col,
                    hardTarget.GridPos.row + hardTarget.LastMoveDir.row);
                var dmgEff = chosenMove?.GetDamage();
                if (dmgEff != null && dmgEff.aoeShape != AttackShape.Single)
                    return BestAoECenter(fromPos, players, chosenMove, attackable);
                return attackable.Contains(predicted) ? predicted : hardTarget.GridPos;

            case AIDifficulty.Ultra:
                BattleEntity ultraTarget = LowestHpPlayer(players);
                GridPos willMove = new GridPos(
                    ultraTarget.GridPos.col + ultraTarget.LastMoveDir.col,
                    ultraTarget.GridPos.row + ultraTarget.LastMoveDir.row);
                if (Random.value < 0.85f && attackable.Contains(willMove)) return willMove;
                return ultraTarget.GridPos;

            default:
                return LowestHpPlayer(players).GridPos;
        }
    }

    static GridPos PickSetupTarget(BattleEntity enemy, List<BattleEntity> players, MoveData move)
    {
        BattleEntity strongest = null;
        foreach (var p in players)
            if (strongest == null || p.CurrentHp > strongest.CurrentHp) strongest = p;
        return strongest?.GridPos ?? players[0].GridPos;
    }

    // ════════════════════════════════════════════════════════════════
    // HELPERS
    // ════════════════════════════════════════════════════════════════
    static List<GridPos> GetReachable(BattleEntity enemy)
    {
        if (enemy.IsImmobilized()) return new List<GridPos> { enemy.GridPos };
        var result = new List<GridPos>();
        var cfg = BattleGridManager.Instance.config;
        GridPos origin = enemy.GridPos;
        int range = enemy.MoveRange;

        for (int dc = -range; dc <= range; dc++)
            for (int dr = -range; dr <= range; dr++)
            {
                if (Mathf.Abs(dc) + Mathf.Abs(dr) > range) continue;
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
        int colEnd = teamId == 0 ? cfg.TotalCols - 1 : cfg.LeftMaxCol;
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
            MoveData move = p.GetMove();
            if (move == null) continue;
            var dmg = move.GetDamage();
            AttackShape sh = dmg != null ? dmg.aoeShape : AttackShape.Single;
            int rad = dmg != null ? dmg.aoeRadius : 1;
            WeatherManager.Instance.GetEffectiveAoE(p.TeamId, sh, rad, out AttackShape effShape, out int effRadius);
            var aoe = BattleGridManager.Instance.GetAoECells(p.GridPos, effShape, p.GridPos, effRadius);
            foreach (var cell in aoe) result.Add(cell);
        }
        return result;
    }

    static BattleEntity FindFinishTarget(BattleEntity enemy, List<BattleEntity> players, MoveData move)
    {
        if (move == null) return null;
        float hpThreshold = 0.25f;
        foreach (var p in players)
        {
            bool lowHp = (float)p.CurrentHp / p.MaxHp <= hpThreshold;
            float typeMult = CombatCalculator.GetTypeMultiplier(move.elementType, p.Data.elementType);
            if (lowHp && typeMult >= 2.0f) return p;
        }
        return null;
    }

    static GridPos BestAoECenter(GridPos from, List<BattleEntity> players,
                                  MoveData move, List<GridPos> attackable)
    {
        GridPos best = players[0].GridPos;
        int bestCount = 0;
        foreach (var cell in attackable)
        {
            var dmg = move.GetDamage();
            AttackShape sh = dmg != null ? dmg.aoeShape : AttackShape.Single;
            int rad = dmg != null ? dmg.aoeRadius : 1;
            var aoe = BattleGridManager.Instance.GetAoECells(cell, sh, from, rad);
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
            : attackable.Count > 0 ? attackable[Random.Range(0, attackable.Count)] : players[0].GridPos;
    }

    static BattleEntity NearestPlayer(BattleEntity enemy, List<BattleEntity> players)
    {
        BattleEntity nearest = null; int best = int.MaxValue;
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
        GridPos best = cells[0]; int bestDist = int.MaxValue;
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
        {
            var dmg = m.GetDamage();
            if (dmg == null) continue;
            if (best == null || dmg.basePower > (best.GetDamage()?.basePower ?? 0))
                best = m;
        }
        return best ?? moves.FirstOrDefault();
    }

    static MoveData LowestPowerMove(List<MoveData> moves)
    {
        MoveData best = null;
        foreach (var m in moves)
        {
            var dmg = m.GetDamage();
            if (dmg == null) continue;
            if (best == null || dmg.basePower < (best.GetDamage()?.basePower ?? int.MaxValue))
                best = m;
        }
        return best ?? moves.FirstOrDefault();
    }

    static MoveData FindStatusMove(List<MoveData> moves, string hint)
    {
        foreach (var m in moves)
            if (m.GetDamage() == null && m.GetTerrain() == null && m.GetWeather() == null)
                return m;
        return null;
    }

    static MoveData FindSuperEffectiveMove(List<MoveData> moves, List<BattleEntity> players)
    {
        MoveData best = null; float bestMult = 1f;
        foreach (var m in moves)
        {
            if (m.GetDamage() == null) continue;
            foreach (var p in players)
            {
                float mult = CombatCalculator.GetTypeMultiplier(m.elementType, p.Data.elementType);
                if (mult > bestMult) { bestMult = mult; best = m; }
            }
        }
        return best;
    }

    // Trả về true nếu chiêu Terrain/Weather còn có ích
    static bool IsUsefulEnvironmentMove(BattleEntity enemy, MoveData move)
    {
        var we = move.GetWeather();
        if (we != null)
        {
            WeatherType current = WeatherManager.Instance.GetWeatherForTeam(enemy.TeamId);
            return current != we.weatherType;
        }
        var te = move.GetTerrain();
        if (te != null)
        {
            int current = TerrainManager.Instance.CountByType(te.terrainType);
            return current < te.maxCount;
        }
        return true;
    }
}