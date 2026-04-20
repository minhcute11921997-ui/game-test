// Assets/_Project/Scripts/Data/MoveData.cs
using UnityEngine;

public enum ElementType
{
    Neutral = 0, Fire = 1, Wood = 2, Water = 3, Earth = 4, Thunder = 5, Wind = 6, Ice = 7, Light = 8, Dark = 9
}
public enum MoveCategory { Physical, Special, Status }

public enum AttackShape
{
    Single,     // 1x1 — ô đúng vào chỗ click
    Cross,      // Hình chữ thập: trên/dưới/trái/phải + tâm
    Square2x2,  // 2x2 xung quanh tâm
    Square3x3,  // 3x3 xung quanh tâm
    Line,       // Đường thẳng từ attacker đến target
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