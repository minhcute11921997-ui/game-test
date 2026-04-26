// Assets/_Project/Scripts/Combat/CombatCalculator.cs
using UnityEngine;

public static class CombatCalculator
{
    private static readonly float[,] TypeChart = new float[10, 10]
    {
        //         Neu   Fir   Wod   Wat   Ear   Thu   Win   Ice   Lit   Drk
        /* Neu */ { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f },
        /* Fir */ { 1.0f, 0.75f, 1.35f, 0.75f, 1.0f, 1.0f, 1.0f, 1.35f, 1.0f, 1.0f },
        /* Wod */ { 1.0f, 0.75f, 0.75f, 1.35f, 1.35f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f },
        /* Wat */ { 1.0f, 1.35f, 0.75f, 0.75f, 1.35f, 1.0f, 1.0f, 0.75f, 1.0f, 1.0f },
        /* Ear */ { 1.0f, 1.0f, 0.75f, 1.0f, 1.0f, 1.35f, 1.0f, 1.0f, 1.0f, 1.0f },
        /* Thu */ { 1.0f, 1.0f, 1.0f, 1.0f, 0.75f, 0.75f, 1.35f, 1.0f, 1.0f, 1.0f },
        /* Win */ { 1.0f, 1.0f, 1.35f, 1.0f, 1.0f, 0.75f, 0.75f, 1.35f, 1.0f, 1.0f },
        /* Ice */ { 1.0f, 0.75f, 1.35f, 1.35f, 1.0f, 1.0f, 0.75f, 0.75f, 1.0f, 1.0f },
        /* Lit */ { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 1.35f },
        /* Drk */ { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.35f, 0.0f },
    };

    public struct DamageResult
    {
        public int damage;
        public float typeMultiplier;
        public bool isStab;
        public bool isCritical;
    }

    public static int CalculateMaxHp(int baseHp, int level)
        => Mathf.FloorToInt((baseHp * 8f * level) / 100f) + level + 150;

    public static float CalculateCritRate(int luck) => 5f + (luck / 10f);
    public static float CalculateEvasionRate(int luck) => luck / 10f;

    public static float GetStageMultiplier(int stage)
    {
        stage = Mathf.Clamp(stage, -6, 6);
        return Mathf.Max(2, 2 + stage) / (float)Mathf.Max(2, 2 - stage);
    }

    // ── NEW: nhận DamageEffect trực tiếp (dùng trong Effect List) ──
    public static DamageResult CalculateDamage(
        BattleEntity attacker,
        BattleEntity defender,
        DamageEffect effect,
        int cellDistanceType = 0)
    {
        float atkStat = (effect.damageCategory == MoveCategory.Physical)
            ? attacker.EffectiveAttack : attacker.EffectiveSpAtk;
        float defStat = Mathf.Max(1f, (effect.damageCategory == MoveCategory.Physical)
            ? defender.EffectiveDefense : defender.EffectiveSpDef);

        // Cần elementType từ MoveData — truyền qua attacker.GetMove()
        MoveData move = attacker.GetMove();
        ElementType moveElement = move != null ? move.elementType : ElementType.Neutral;

        int power = move.GetDamage()?.basePower ?? 40;
        float baseDmg = Mathf.FloorToInt(
            ((2f * attackerLevel / 5f + 2f) * power * (atkStat / defStat)) / 10f);

        bool isStab = moveElement == attacker.Data.elementType
                      && attacker.Data.elementType != ElementType.Neutral;
        float stabMult = isStab ? 1.2f : 1.0f;

        float typeMult = GetTypeMultiplier(moveElement, defender.Data.elementType);
        if (typeMult == 0f)
            return new DamageResult { damage = 0, typeMultiplier = 0f, isStab = isStab };

        bool isCrit = Random.value < (CalculateCritRate(attacker.Data.luck) / 100f);
        float critMult = isCrit ? 1.5f : 1.0f;
        float rng = Random.Range(0.9f, 1.0f);
        float falloff = GetFalloff(effect.aoeShape, cellDistanceType, ref rng);

        int damage = Mathf.Max(1, Mathf.FloorToInt(baseDmg * stabMult * typeMult * critMult * rng * falloff));

        return new DamageResult
        {
            damage = damage,
            typeMultiplier = typeMult,
            isStab = isStab,
            isCritical = isCrit
        };
    }

    // ── Giữ nguyên Calculate cũ để không break các chỗ còn gọi ──
    public static DamageResult Calculate(
        BattleEntity attacker, BattleEntity defender, MoveData move,
        int attackerLevel, int attackerLuck,
        AttackShape aoeShape = AttackShape.Single, int cellDistanceType = 0)
    {
        float atkStat = (move.category == MoveCategory.Physical)
            ? attacker.EffectiveAttack : attacker.EffectiveSpAtk;
        float defStat = Mathf.Max(1f, (move.category == MoveCategory.Physical)
            ? defender.EffectiveDefense : defender.EffectiveSpDef);

        float baseDmg = Mathf.FloorToInt(
            ((2f * attackerLevel / 5f + 2f) * move.GetDamage()?.basePower ?? 40 * (atkStat / defStat)) / 10f);

        bool isStab = move.elementType == attacker.Data.elementType
                      && attacker.Data.elementType != ElementType.Neutral;
        float stabMult = isStab ? 1.2f : 1.0f;
        float typeMult = GetTypeMultiplier(move.elementType, defender.Data.elementType);
        if (typeMult == 0f)
            return new DamageResult { damage = 0, typeMultiplier = 0f, isStab = isStab };

        bool isCrit = Random.value < (CalculateCritRate(attackerLuck) / 100f);
        float critMult = isCrit ? 1.5f : 1.0f;
        float rng = Random.Range(0.9f, 1.0f);
        float falloff = GetFalloff(aoeShape, cellDistanceType, ref rng);

        int damage = Mathf.Max(1, Mathf.FloorToInt(baseDmg * stabMult * typeMult * critMult * rng * falloff));
        return new DamageResult { damage = damage, typeMultiplier = typeMult, isStab = isStab, isCritical = isCrit };
    }

    // ── EstimateDamage cho AI (nhận DamageEffect) ─────────────────
    public static int EstimateDamage(BattleEntity attacker, BattleEntity defender, DamageEffect effect)
    {
        float atkStat = (effect.damageCategory == MoveCategory.Physical)
            ? attacker.EffectiveAttack : attacker.EffectiveSpAtk;
        float defStat = Mathf.Max(1f, (effect.damageCategory == MoveCategory.Physical)
            ? defender.EffectiveDefense : defender.EffectiveSpDef);

        MoveData move = attacker.GetMove();
        ElementType moveElement = move != null ? move.elementType : ElementType.Neutral;

        float baseDmg = Mathf.FloorToInt(
            ((2f * attacker.Level / 5f + 2f) * effect.basePower * (atkStat / defStat)) / 10f);
        float typeMult = GetTypeMultiplier(moveElement, defender.Data.elementType);

        return Mathf.Max(1, Mathf.FloorToInt(baseDmg * 1.2f * typeMult * 0.95f));
    }

    private static float GetFalloff(AttackShape shape, int cellDistanceType, ref float rng)
    {
        switch (shape)
        {
            case AttackShape.Single:
            case AttackShape.Cross:
            case AttackShape.Line:
                return 1.0f;
            case AttackShape.Square3x3:
                if (cellDistanceType == 0) return 1.0f;
                if (cellDistanceType == 1) return 0.8f;
                return 0.7f;
            case AttackShape.Square2x2:
                rng *= Random.Range(0.85f, 1.0f);
                return 1.0f;
            default:
                return 1.0f;
        }
    }

    public static float GetTypeMultiplier(ElementType attackType, ElementType defendType)
        => TypeChart[(int)attackType, (int)defendType];
}