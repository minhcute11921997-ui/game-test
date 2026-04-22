using UnityEngine;

public class AIPersonality
{
    public enum AIPersonality
{
    [InspectorName("Tấn Công")]   Aggressive,
    [InspectorName("Phòng Thủ")]  Defensive,
    [InspectorName("Ngẫu Nhiên")] Random,
    [InspectorName("Hỗ Trợ")]    Support,
}
}
