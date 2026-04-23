using UnityEngine;
using System.Collections.Generic;

public static class RuntimeGameState
{
    public static ThingData CurrentEnemy;
    public static List<ThingData> Party = new();

    public static ThingData ActiveThing => Party.Count > 0 ? Party[0] : null;

    public static void ResetForNewSession()
{
    Party.Clear();
    CurrentEnemy = null;
}
}