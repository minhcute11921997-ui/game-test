using UnityEngine;

/// <summary>
/// Tính toán chiến đấu theo GDD đã chốt:
///
/// HP:      MaxHP = floor(Base * 8 * Level / 100) + Level + 150
/// Damage:  BaseDamage = floor(((2*Level/5 + 2) * Power * Atk/Def) / 10)
///          FinalDamage = BaseDamage * STAB * Weakness * Critical * RNG * Falloff
///
/// STAB:    x1.5 nếu move cùng hệ với attacker (trừ Neutral)
/// Weakness: x2.0 / x1.0 / x0.5 / x0.0 (miễn dịch)
/// Critical: 5% + Luck/10 cơ bản, nhân x1.5 khi trúng
/// RNG:     0.9 – 1.0 (chung cho mọi chiêu)
/// AoE Falloff:
///   - Single 1x1   : x1.0
///   - Square 3x3   : tâm x1.0 | cận tâm x0.75 | góc chéo x0.5
///   - Square 2x2   : RNG riêng 0.85–1.0 nhân thêm vào RNG chung → tổng 0.765–1.0
///   - Cross / Line  : x1.0 mọi ô (xử lý ngoài, không falloff)
/// </summary>
public static class CombatCalculator
{
    // ─── Bảng tương khắc 9 hệ ───────────────────────────────────────────────
    // [attackerType, defenderType] : 2.0=khắc | 0.5=bị kháng | 0.0=miễn dịch
    private static readonly float[,] TypeChart = new float[10, 10]
    {
        //         Neu   Fir   Wod   Wat   Ear   Thu   Win   Ice   Lit   Drk
        /* Neu */ { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f },
        /* Fir */ { 1.0f, 0.5f, 2.0f, 0.5f, 1.0f, 1.0f, 1.0f, 2.0f, 1.0f, 1.0f },
        /* Wod */ { 1.0f, 0.5f, 0.5f, 2.0f, 2.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f },
        /* Wat */ { 1.0f, 2.0f, 0.5f, 0.5f, 2.0f, 1.0f, 1.0f, 0.5f, 1.0f, 1.0f },
        /* Ear */ { 1.0f, 1.0f, 0.5f, 1.0f, 1.0f, 2.0f, 1.0f, 1.0f, 1.0f, 1.0f },
        /* Thu */ { 1.0f, 1.0f, 1.0f, 1.0f, 0.5f, 0.5f, 2.0f, 1.0f, 1.0f, 1.0f },
        /* Win */ { 1.0f, 1.0f, 2.0f, 1.0f, 1.0f, 0.5f, 0.5f, 2.0f, 1.0f, 1.0f },
        /* Ice */ { 1.0f, 0.5f, 2.0f, 2.0f, 1.0f, 1.0f, 0.5f, 0.5f, 1.0f, 1.0f },
        /* Lit */ { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.5f, 2.0f },
        /* Drk */ { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 2.0f, 0.5f },
    };

    // ─── Kết quả trả về ─────────────────────────────────────────────────────
    public struct DamageResult
    {
        public int   damage;          // Sát thương cuối (đã nhân Falloff)
        public float typeMultiplier;  // 0.0 / 0.5 / 1.0 / 2.0
        public bool  isStab;
        public bool  isCritical;
    }

    // ─── HP Formula (V2) ────────────────────────────────────────────────────
    /// <summary>
    /// MaxHP = floor(baseHp * 8 * level / 100) + level + 150
    /// Cho ra ~730–1210 khi base 60–120, level 100.
    /// </summary>
    public static int CalculateMaxHp(int baseHp, int level)
    {
        return Mathf.FloorToInt((baseHp * 8f * level) / 100f) + level + 150;
    }

    // ─── Crit Rate ──────────────────────────────────────────────────────────
    /// <summary>CritRate(%) = 5 + Luck/10 — đạt 15% khi Luck = 100.</summary>
    public static float CalculateCritRate(int luck)
    {
        return 5f + (luck / 10f);
    }

    // ─── Evasion Rate ───────────────────────────────────────────────────────
    /// <summary>EvasionRate(%) = Luck/10 — đạt 10% khi Luck = 100.</summary>
    public static float CalculateEvasionRate(int luck)
    {
        return luck / 10f;
    }

    // ─── Stat Stage Multiplier ──────────────────────────────────────────────
    /// <summary>
    /// Stage chạy từ -6 đến +6.
    /// Multiplier = Max(2, 2+stage) / Max(2, 2-stage)
    /// Stage +2 = x2.0 | Stage -2 = x0.5
    /// </summary>
    public static float GetStageMultiplier(int stage)
    {
        stage = Mathf.Clamp(stage, -6, 6);
        return Mathf.Max(2, 2 + stage) / (float)Mathf.Max(2, 2 - stage);
    }

    // ─── Damage chính ───────────────────────────────────────────────────────
    /// <summary>
    /// Tính sát thương cho 1 ô đích.
    /// attackerLuck dùng để tính CritRate.
    /// aoeShape + cellDistanceFromCenter dùng để tính Falloff.
    /// </summary>
    public static DamageResult Calculate(
        ThingData  attacker,
        ThingData  defender,
        MoveData   move,
        int        attackerLevel,
        int        attackerLuck,
        AttackShape aoeShape            = AttackShape.Single,
        int         cellDistanceType    = 0)   // 0=tâm/single, 1=cận tâm, 2=góc chéo
    {
        // 1. Stat tấn công / phòng thủ (Physical vs Special)
        float atkStat = (move.category == MoveCategory.Physical)
            ? attacker.attack
            : attacker.spAtk;

        float defStat = (move.category == MoveCategory.Physical)
            ? defender.defense
            : defender.spDef;

        defStat = Mathf.Max(1f, defStat); // tránh chia 0

        // 2. Base Damage — công thức GDD
        // BaseDamage = floor(((2*Level/5 + 2) * Power * Atk/Def) / 10)
        float baseDmg = Mathf.FloorToInt(
            ((2f * attackerLevel / 5f + 2f) * move.basePower * (atkStat / defStat)) / 10f
        );

        // 3. STAB x1.5
        bool  isStab       = (move.elementType == attacker.elementType)
                           && attacker.elementType != ElementType.Neutral;
        float stabMult     = isStab ? 1.5f : 1.0f;

        // 4. Tương khắc hệ
        float typeMult     = GetTypeMultiplier(move.elementType, defender.elementType);

        // Miễn dịch → không cần tính tiếp
        if (typeMult == 0f)
            return new DamageResult { damage = 0, typeMultiplier = 0f, isStab = isStab, isCritical = false };

        // 5. Chí mạng
        float critRatePct  = CalculateCritRate(attackerLuck);
        bool  isCrit       = Random.value < (critRatePct / 100f);
        float critMult     = isCrit ? 1.5f : 1.0f;

        // 6. RNG chung 0.9 – 1.0
        float rng          = Random.Range(0.9f, 1.0f);

        // 7. AoE Falloff
        float falloff      = GetFalloff(aoeShape, cellDistanceType, ref rng);

        // 8. Final Damage
        float final        = baseDmg * stabMult * typeMult * critMult * rng * falloff;
        int   damage       = Mathf.Max(1, Mathf.FloorToInt(final));

        return new DamageResult
        {
            damage          = damage,
            typeMultiplier  = typeMult,
            isStab          = isStab,
            isCritical      = isCrit
        };
    }

    // ─── AoE Falloff ────────────────────────────────────────────────────────
    /// <summary>
    /// Trả về hệ số falloff và chỉnh rng nếu là 2x2.
    /// cellDistanceType: 0=tâm/single, 1=cận tâm (trên/dưới/trái/phải), 2=góc chéo.
    /// </summary>
    private static float GetFalloff(AttackShape shape, int cellDistanceType, ref float rng)
    {
        switch (shape)
        {
            case AttackShape.Single:
            case AttackShape.Cross:
            case AttackShape.Line:
                return 1.0f;                             // không falloff

            case AttackShape.Square3x3:
                if (cellDistanceType == 0) return 1.0f;  // tâm chấn
                if (cellDistanceType == 1) return 0.75f; // cận tâm
                return 0.5f;                             // góc chéo

            case AttackShape.Square2x2:
                // RNG riêng 0.85–1.0 nhân thêm vào RNG chung → tổng 0.765–1.0
                float aoeRng = Random.Range(0.85f, 1.0f);
                rng *= aoeRng;
                return 1.0f;                             // falloff tính qua RNG kép

            default:
                return 1.0f;
        }
    }

    // ─── Helper public ──────────────────────────────────────────────────────
    public static float GetTypeMultiplier(ElementType attackType, ElementType defendType)
    {
        return TypeChart[(int)attackType, (int)defendType];
    }
}
