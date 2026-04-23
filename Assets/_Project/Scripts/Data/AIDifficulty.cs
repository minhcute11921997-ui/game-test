using UnityEngine;
public enum AIDifficulty
{
    [InspectorName("Dễ (Bản Năng)")]    Easy,
    [InspectorName("Trung Bình")]        Medium,
    [InspectorName("Khó (Chiến Thuật)")] Hard,
    [InspectorName("Cực Khó (Toàn Tri)")] Ultra,
}

public enum ThingArchetype
{
    [InspectorName("Tấn Công")]  Attacker,
    [InspectorName("Phòng Thủ")] Defender,
    [InspectorName("Hỗ Trợ")]   Setup,
}