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
    [InspectorName("Môi Trường")] Environment,
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

public enum EnvironmentCategory
{
    [InspectorName("Thời Tiết")] Weather,
    [InspectorName("Địa Hình")] Terrain,
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

public enum WeatherTarget
{
    [InspectorName("Cả 2 sân")] Both,
    [InspectorName("Sân trái (Team 0)")] TeamLeft,
    [InspectorName("Sân phải (Team 1)")] TeamRight,
}

[CreateAssetMenu(fileName = "NewMove", menuName = "Game/Move Data")]
public class MoveData : ScriptableObject
{
    [Header("Thông Tin Cơ Bản")]
    public string moveName = "Tackle";
    public ElementType elementType = ElementType.Neutral;
    public MoveCategory category = MoveCategory.Physical;

    [Tooltip("Chỉ dùng khi category = Status")]
    public StatusSubType statusSubType = StatusSubType.Buff;

    [Header("Chiêu Tấn Công")]
    public AttackShape shape = AttackShape.Single;
    [Range(1, 5)] public int aoeRadius = 1; // bán kính AoE (dùng cho Cross, Square)
    [Range(0, 200)] public int basePower = 40;
    [Range(0, 100)] public int accuracy = 100;
    public int pp = 20;

    [Header("Môi Trường (chỉ dùng khi category = Environment)")]
    public EnvironmentCategory envCategory = EnvironmentCategory.Weather;
    public int envDuration = 5;             // số lượt tồn tại

    [Header("Thời Tiết")]
    public WeatherType weatherType = WeatherType.None;
    public WeatherTarget weatherTarget = WeatherTarget.Both;

    [Header("Địa Hình")]
    public TerrainEffectType terrainEffect = TerrainEffectType.None;
    public AttackShape terrainShape = AttackShape.Single; // vùng đặt terrain
    public int terrainMaxCount = 1;         // giới hạn ô cùng lúc trên 2 sân
}