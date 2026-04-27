// Assets/_Project/Scripts/Data/MoveEffect.cs
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class MoveEffect
{
    public TargetScope targetScope = TargetScope.EnemySide;
    public AttackShape aoeShape = AttackShape.Single;
    public int aoeRadius = 1;

    [Range(0f, 1f)]
    public float triggerChance = 1f;

    public abstract EffectResult Resolve(BattleEntity attacker, List<BattleEntity> targets);

    public virtual IEnumerator PlayAnimation(EffectResult result, BattleEntity attacker)
    {
        yield break;
    }
}

// ── DamageEffect ──────────────────────────────────────────────────
[Serializable]
public class DamageEffect : MoveEffect
{
    public MoveCategory damageCategory = MoveCategory.Physical;
    [Range(0, 200)] public int basePower = 40;
    [Range(0, 100)] public int accuracy = 100;

    public override EffectResult Resolve(BattleEntity attacker, List<BattleEntity> targets)
    {
        var result = new EffectResult
        {
            triggered = true,
            resultType = EffectResultType.Damage
        };

        foreach (var target in targets)
        {
            if (target == null || target.TeamId == attacker.TeamId) continue;

            float evasionRate = CombatCalculator.CalculateEvasionRate(target.EffectiveLuck);
            if (UnityEngine.Random.value < evasionRate / 100f)
            {
                result.logMessage += $"{target.Data.thingName} né!\n";
                continue;
            }

            var dmgResult = CombatCalculator.CalculateDamage(attacker, target, this);
            result.hits.Add((target, dmgResult.damage));
            result.logMessage += $"{attacker.Data.thingName}→{target.Data.thingName}: " +
                                 $"{dmgResult.damage} dmg " +
                                 $"(x{dmgResult.typeMultiplier})" +
                                 $"{(dmgResult.isCritical ? " CHÍ MẠNG!" : "")}\n";
        }

        return result;
    }
}

// ── HealEffect ────────────────────────────────────────────────────
[Serializable]
public class HealEffect : MoveEffect
{
    [Range(0.05f, 1f)] public float healPercent = 0.25f;

    public override EffectResult Resolve(BattleEntity attacker, List<BattleEntity> targets)
    {
        var result = new EffectResult
        {
            triggered = true,
            resultType = EffectResultType.Heal
        };

        var actualTargets = (targetScope == TargetScope.OwnSide || targets.Count == 0)
            ? new List<BattleEntity> { attacker }
            : targets;

        foreach (var target in actualTargets)
        {
            if (target == null) continue;
            int healAmt = UnityEngine.Mathf.FloorToInt(target.MaxHp * healPercent);
            result.hits.Add((target, healAmt));
            result.logMessage += $"Hồi {healAmt} HP cho {target.Data.thingName}\n";
        }

        return result;
    }
}

// ── StatStageEffect ───────────────────────────────────────────────
[Serializable]
public class StatStageEffect : MoveEffect
{
    public StatType statType = StatType.Attack;
    [Range(-3, 3)] public int delta = 1;

    public override EffectResult Resolve(BattleEntity attacker, List<BattleEntity> targets)
    {
        var result = new EffectResult
        {
            triggered = true,
            resultType = EffectResultType.StatStage,
            statType = statType,
            statDelta = delta
        };

        var actualTargets = (targetScope == TargetScope.OwnSide)
            ? new List<BattleEntity> { attacker }
            : targets;

        foreach (var target in actualTargets)
        {
            if (target == null) continue;
            int newStage = target.ApplyStage(statType, delta);
            result.hits.Add((target, delta));
            string dir = delta > 0 ? "tăng" : "giảm";
            result.logMessage += $"{target.Data.thingName} {dir} {statType} {UnityEngine.Mathf.Abs(delta)} stage → {newStage}\n";
        }

        return result;
    }
}

// ── TerrainEffect ─────────────────────────────────────────────────
[Serializable]
public class TerrainEffect : MoveEffect
{
    public TerrainEffectType terrainType = TerrainEffectType.ThornTrap;
    public AttackShape terrainShape = AttackShape.Single;
    public int maxCount = 1;
    public int duration = 3;

    public override EffectResult Resolve(BattleEntity attacker, List<BattleEntity> targets)
    {
        return new EffectResult
        {
            triggered = true,
            resultType = EffectResultType.Terrain,
            logMessage = $"Đặt địa hình {terrainType}"
        };
    }
}

// ── WeatherEffect ─────────────────────────────────────────────────
[Serializable]
public class WeatherEffect : MoveEffect
{
    public WeatherType weatherType = WeatherType.Blizzard;
    public int duration = 5;

    public override EffectResult Resolve(BattleEntity attacker, List<BattleEntity> targets)
    {
        return new EffectResult
        {
            triggered = true,
            resultType = EffectResultType.Weather,
            logMessage = $"Tung thời tiết {weatherType}"
        };
    }
    // ── KnockbackEffect ───────────────────────────────────────────────
    [Serializable]
    public class KnockbackEffect : MoveEffect
    {
        [Range(1, 5)] public int pushDistance = 1;

        public override EffectResult Resolve(BattleEntity attacker, List<BattleEntity> targets)
        {
            return new EffectResult
            {
                triggered = true,
                resultType = EffectResultType.Knockback,
                logMessage = $"Knockback {pushDistance} ô"
            };
        }
    }
}