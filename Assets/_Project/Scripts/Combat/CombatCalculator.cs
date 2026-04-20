using UnityEngine;

/// <summary>
/// Tính toán sát thương chiến đấu theo GDD:
/// - 6 chỉ số: HP, ATK, DEF, SpAtk, SpDef, Speed
/// - 9 Hệ: tương khắc + STAB x1.2
/// - Trả về struct DamageResult chứa đủ thông tin để BattlePhaseManager xử lý
/// </summary>
public static class CombatCalculator
{
    // Bảng tương khắc 9 Hệ — [attackerType, defenderType] = multiplier
    // 2.0 = khắc, 0.5 = bị khắc, 1.0 = bình thường, 0.0 = miễn dịch
    private static readonly float[,] TypeChart = new float[10, 10]
    {
        //         Neu  Fir  Wod  Wat  Ear  Thu  Win  Ice  Lit  Drk
        /* Neu */ { 1.0f,1.0f,1.0f,1.0f,1.0f,1.0f,1.0f,1.0f,1.0f,1.0f },
        /* Fir */ { 1.0f,0.5f,2.0f,0.5f,1.0f,1.0f,1.0f,2.0f,1.0f,1.0f },
        /* Wod */ { 1.0f,0.5f,0.5f,2.0f,2.0f,1.0f,1.0f,1.0f,1.0f,1.0f },
        /* Wat */ { 1.0f,2.0f,0.5f,0.5f,2.0f,1.0f,1.0f,0.5f,1.0f,1.0f },
        /* Ear */ { 1.0f,1.0f,0.5f,1.0f,1.0f,2.0f,1.0f,1.0f,1.0f,1.0f },
        /* Thu */ { 1.0f,1.0f,1.0f,1.0f,0.5f,0.5f,2.0f,1.0f,1.0f,1.0f },
        /* Win */ { 1.0f,1.0f,2.0f,1.0f,1.0f,0.5f,0.5f,2.0f,1.0f,1.0f },
        /* Ice */ { 1.0f,0.5f,2.0f,2.0f,1.0f,1.0f,0.5f,0.5f,1.0f,1.0f },
        /* Lit */ { 1.0f,1.0f,1.0f,1.0f,1.0f,1.0f,1.0f,1.0f,0.5f,2.0f },
        /* Drk */ { 1.0f,1.0f,1.0f,1.0f,1.0f,1.0f,1.0f,1.0f,2.0f,0.5f },
    };

    public struct DamageResult
    {
        public int damage;           // Sát thương cuối cùng
        public float typeMultiplier; // 0.5 / 1.0 / 2.0
        public bool isStab;          // Có STAB không
        public bool isCritical;      // Có chí mạng không
    }

    /// <summary>
    /// Tính sát thương khi attacker dùng move tấn công defender.
    /// </summary>
    public static DamageResult Calculate(
        ThingData attacker,
        ThingData defender,
        MoveData move)
    {
        // 1. Xác định stat tấn công và phòng thủ
        float atkStat = (move.category == MoveCategory.Physical)
            ? attacker.attack
            : attacker.spAtk;

        float defStat = (move.category == MoveCategory.Physical)
            ? defender.defense
            : defender.spDef;

        // 2. Công thức cơ bản (tương tự Gen-5 Pokemon, được điều chỉnh)
        // Damage = ((BasePower * Atk / Def) / 50 + 2) * Level
        // Level cố định 50 trong công thức vì BST đã cân bằng ở cùng cấp
        float base_dmg = ((move.basePower * atkStat / defStat) / 50f) + 2f;

        // 3. STAB — Dùng chiêu trùng hệ với bản thân được x1.2
        bool isStab = (move.elementType == attacker.elementType)
                   && attacker.elementType != ElementType.Neutral;
        float stabMultiplier = isStab ? 1.2f : 1.0f;

        // 4. Tương khắc hệ
        float typeMultiplier = GetTypeMultiplier(move.elementType, defender.elementType);

        // 5. Chí mạng — 6.25% cơ bản (1/16)
        bool isCritical = Random.value < 0.0625f;
        float critMultiplier = isCritical ? 1.5f : 1.0f;

        // 6. Ngẫu nhiên ±5% (tránh kết quả cứng nhắc)
        float randomFactor = Random.Range(0.95f, 1.05f);

        // 7. Tổng hợp
        float finalDamage = base_dmg * stabMultiplier * typeMultiplier * critMultiplier * randomFactor;
        int damage = Mathf.Max(1, Mathf.RoundToInt(finalDamage)); // Tối thiểu 1 dmg

        return new DamageResult
        {
            damage = damage,
            typeMultiplier = typeMultiplier,
            isStab = isStab,
            isCritical = isCritical
        };
    }

    /// <summary>
    /// Tra bảng tương khắc theo ElementType.
    /// </summary>
    public static float GetTypeMultiplier(ElementType attackType, ElementType defendType)
    {
        return TypeChart[(int)attackType, (int)defendType];
    }
}