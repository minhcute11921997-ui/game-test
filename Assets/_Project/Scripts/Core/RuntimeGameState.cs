using UnityEngine;
using System.Collections.Generic;

public static class RuntimeGameState
{
    public static ThingData CurrentEnemy;

    /// <summary>Sprint 4+: tối đa 2 Thing mỗi phe.</summary>
    public static List<ThingData> Party = new List<ThingData>();

    /// <summary>Sprint 6: phe địch tối đa 2 Thing.</summary>
    public static List<ThingData> EnemyParty = new List<ThingData>();

    /// <summary>Sprint 7: kho nguyên liệu thu được sau trận.</summary>
    public static Dictionary<string, int> Inventory = new Dictionary<string, int>();

    public static ThingData ActiveThing => Party.Count > 0 ? Party[0] : null;
}