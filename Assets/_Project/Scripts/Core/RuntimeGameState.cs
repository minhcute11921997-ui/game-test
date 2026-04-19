using UnityEngine;
using System.Collections.Generic;

// Phải có chữ "public static" ở đây
public static class RuntimeGameState
{
    public static ThingData CurrentEnemy;
    public static List<ThingData> Party = new();
}