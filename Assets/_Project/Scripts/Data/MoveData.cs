// Assets/_Project/Scripts/Data/MoveData.cs
using UnityEngine;

public enum ElementType
{
    [InspectorName("Trung Lập")]   Neutral = 0,
    [InspectorName("Hỏa (Lửa)")]  Fire    = 1,
    [InspectorName("Mộc (Cây)")]  Wood    = 2,
    [InspectorName("Thủy (Nước)")] Water  = 3,
    [InspectorName("Thổ (Đất)")]  Earth   = 4,
    [InspectorName("Lôi (Sấm)")]  Thunder = 5,
    [InspectorName("Phong (Gió)")] Wind   = 6,
    [InspectorName("Băng (Đá)")]  Ice     = 7,
    [InspectorName("Quang (Sáng)")] Light = 8,
    [InspectorName("Ám (Tối)")]   Dark    = 9,
}
public enum MoveCategory
{
    [InspectorName("Vật Lý")]   Physical,
    [InspectorName("Đặc Biệt")] Special,
    [InspectorName("Trạng Thái")] Status,
}

public enum AttackShape
{
    [InspectorName("Đơn (1 ô)")]       Single,
    [InspectorName("Chữ Thập")]        Cross,
    [InspectorName("Vuông 2×2")]       Square2x2,
    [InspectorName("Vuông 3×3")]       Square3x3,
    [InspectorName("Đường Thẳng")]     Line,
}

[CreateAssetMenu(fileName = "NewMove", menuName = "Game/Move Data")]
public class MoveData : ScriptableObject
{
    public string moveName = "Tackle";
    public ElementType elementType = ElementType.Neutral;
    public MoveCategory category = MoveCategory.Physical;
    public AttackShape shape = AttackShape.Single;   // ← MỚI
    [Range(0, 200)] public int basePower = 40;
    [Range(0, 100)] public int accuracy = 100;
    public int pp = 20;
}