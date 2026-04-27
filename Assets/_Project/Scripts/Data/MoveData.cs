// Assets/_Project/Scripts/Data/MoveData.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ElementType
{
    [InspectorName("Trung Lập")] Neutral = 0,
    [InspectorName("Hỏa (Lửa)")] Fire = 1,
    [InspectorName("Mộc (Cây)")] Wood = 2,
    [InspectorName("Thủy (Nước)")] Water = 3,
    [InspectorName("Thổ (Đất)")] Earth = 4,
    [InspectorName("Lôi (Sấm)")] Thunder = 5,
    [InspectorName("Phong (Gió)")] Wind = 6,
    [InspectorName("Băng (Đá)")] Ice = 7,
    [InspectorName("Quang (Sáng)")] Light = 8,
    [InspectorName("Ám (Tối)")] Dark = 9,
}

public enum MoveCategory
{
    [InspectorName("Vật Lý")] Physical,
    [InspectorName("Đặc Biệt")] Special,
    [InspectorName("Trạng Thái")] Status,
    [InspectorName("Thời Tiết")] Weather,
    [InspectorName("Địa Hình")] Terrain,
}

public enum StatusSubType
{
    [InspectorName("Buff (Tăng chỉ số)")] Buff,
    [InspectorName("Debuff (Giảm chỉ số)")] Debuff,
    [InspectorName("Hồi Phục (Heal)")] Heal,
}

public enum AttackShape
{
    [InspectorName("Đơn (1 ô)")] Single,
    [InspectorName("Chữ Thập")] Cross,
    [InspectorName("Vuông 2×2")] Square2x2,
    [InspectorName("Vuông 3×3")] Square3x3,
    [InspectorName("Đường Thẳng")] Line,
}

public enum WeatherType
{
    [InspectorName("Không có")] None,
    [InspectorName("Bão Tuyết")] Blizzard,
    [InspectorName("Vùng Từ Trường")] MagneticField,
}

public enum TerrainEffectType
{
    [InspectorName("Không có")] None,
    [InspectorName("Bẫy Gai")] ThornTrap,
    [InspectorName("Vết Cháy")] BurnMark,
}

public enum TargetScope
{
    [InspectorName("Sân địch")] EnemySide,
    [InspectorName("Sân mình")] OwnSide,
    [InspectorName("Cả 2 sân")] BothSides,
    [InspectorName("Không cần chọn ô")] NoTarget,
}

public enum StatType
{
    [InspectorName("Tấn Công (Attack)")] Attack,
    [InspectorName("Phòng Thủ (Defense)")] Defense,
    [InspectorName("Đặc Công (SpAtk)")] SpAtk,
    [InspectorName("Đặc Thủ (SpDef)")] SpDef,
    [InspectorName("Tốc Độ (Speed)")] Speed,
    [InspectorName("May Mắn (Luck)")] Luck,
}

public enum ThingFootprint
{
    [InspectorName("1×1 (Mặc định)")] Size1x1,
    [InspectorName("2×2")] Size2x2,
    [InspectorName("3×3")] Size3x3,
    [InspectorName("Chữ Thập (bán kính 1)")] Cross1,
}

[CreateAssetMenu(fileName = "NewMove", menuName = "Game/Move Data")]
public class MoveData : ScriptableObject
{
    [Header("Thông Tin Cơ Bản")]
    public string moveName = "Tackle";
    public ElementType elementType = ElementType.Neutral;
    public MoveCategory category = MoveCategory.Physical;

    [Range(1, 64)] public int maxPP = 20;

    [Header("⚠️ Thứ tự: Buff/Debuff → Damage → Heal → Terrain → Weather")]
    [SerializeReference]
    public List<MoveEffect> effects = new();

    // Helpers đọc nhanh
    public DamageEffect GetDamage() => effects.OfType<DamageEffect>().FirstOrDefault();
    public HealEffect GetHeal() => effects.OfType<HealEffect>().FirstOrDefault();
    public WeatherEffect GetWeather() => effects.OfType<WeatherEffect>().FirstOrDefault();
    public TerrainEffect GetTerrain() => effects.OfType<TerrainEffect>().FirstOrDefault();

    // Backward-compat helpers để các hệ thống cũ đọc được nhanh
    public TargetScope primaryScope =>
        effects.Count > 0 ? effects[0].targetScope : TargetScope.EnemySide;

    public bool hasNoTarget =>
        effects.Count > 0 && effects[0].targetScope == TargetScope.NoTarget;



    public int GetBasePower() => GetDamage()?.basePower ?? 0;

    public StatusSubType GetStatusSubType()
    {
        if (GetHeal() != null) return StatusSubType.Heal;
        var stat = effects.OfType<StatStageEffect>().FirstOrDefault();
        if (stat != null) return stat.delta > 0 ? StatusSubType.Buff : StatusSubType.Debuff;
        return StatusSubType.Debuff;
    }
}