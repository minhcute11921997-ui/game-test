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
    [InspectorName("Thời Tiết")] Weather,   // ← tách ra
    [InspectorName("Địa Hình")] Terrain,   // ← tách ra
}

// EnvironmentCategory đã bị XÓA — không dùng nữa

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
    [InspectorName("Không cần chọn ô")] NoTarget, // Weather Both
}
[CreateAssetMenu(fileName = "NewMove", menuName = "Game/Move Data")]
public class MoveData : ScriptableObject
{
    [Header("Thông Tin Cơ Bản")]
    public string moveName = "Tackle";
    public ElementType elementType = ElementType.Neutral;
    public MoveCategory category = MoveCategory.Physical;


    [Header("Phạm vi chọn ô mục tiêu")]
    public TargetScope targetScope = TargetScope.EnemySide;
    [Tooltip("Chỉ dùng khi category = Status")]
    public StatusSubType statusSubType = StatusSubType.Buff;
    [Header("Buff / Debuff (chỉ dùng khi statusSubType = Buff/Debuff)")]
    [Tooltip("Chỉ số bị ảnh hưởng: attack, defense, spAtk, spDef, speed")]
    public string statTarget = "attack";

    [Tooltip("Số stage thay đổi: +1/+2 là buff, -1/-2 là debuff")]
    [Range(-3, 3)]
    public int statDelta = 1;

    [Header("Heal (chỉ dùng khi statusSubType = Heal)")]

    [Tooltip("Phần trăm MaxHP được hồi, ví dụ 0.25 = 25%")]

    [Range(0.05f, 1.0f)]
    public float healPercent = 0.25f;

    [Header("Chiêu Tấn Công")]
    public AttackShape shape = AttackShape.Single;
    [Range(1, 5)] public int aoeRadius = 1;
    [Range(0, 200)] public int basePower = 40;
    [Range(0, 100)] public int accuracy = 100;
    public int pp = 20;

    [Header("Thời Tiết (chỉ dùng khi category = Weather)")]
    public WeatherType weatherType = WeatherType.None;
    public int weatherDuration = 5;

    [Header("Địa Hình (chỉ dùng khi category = Terrain)")]
    public TerrainEffectType terrainEffect = TerrainEffectType.None;
    public AttackShape terrainShape = AttackShape.Single;
    public int terrainMaxCount = 1;
    public int terrainDuration = 3;


}