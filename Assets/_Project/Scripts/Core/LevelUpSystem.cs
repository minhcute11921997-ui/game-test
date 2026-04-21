// Assets/_Project/Scripts/Core/LevelUpSystem.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sprint 7: Công thức EXP / Level Up và lấy danh sách kỹ năng Gacha.
/// </summary>
public static class LevelUpSystem
{
    /// <summary>EXP cần thiết để lên từ level hiện tại lên level + 1.</summary>
    public static int ExpToNextLevel(int level)
        => level * level * 10; // Level 1→2: 10, Level 5→6: 250, Level 10→11: 1000

    /// <summary>
    /// Cộng EXP vào ThingData.
    /// Trả về true nếu leveled up ít nhất 1 lần.
    /// </summary>
    public static bool AddExp(ThingData thing, int exp)
    {
        if (exp <= 0) return false;

        thing.experience += exp;
        bool leveledUp = false;

        while (thing.experience >= ExpToNextLevel(thing.level) && thing.level < 100)
        {
            thing.experience -= ExpToNextLevel(thing.level);
            thing.level++;
            leveledUp = true;
            Debug.Log($"[LevelUp] {thing.thingName} lên Lv.{thing.level}!");
        }
        return leveledUp;
    }

    /// <summary>
    /// Lấy tối đa <paramref name="count"/> MoveData ngẫu nhiên từ Resources/Moves/.
    /// Ưu tiên cùng hệ với <paramref name="thing"/>, bỏ qua đã học rồi.
    /// </summary>
    public static List<MoveData> GetGachaOptions(ThingData thing, int count = 3)
    {
        var all = Resources.LoadAll<MoveData>("Moves");
        if (all == null || all.Length == 0)
        {
            Debug.LogWarning("[LevelUp] Không tìm thấy MoveData trong Resources/Moves/");
            return new List<MoveData>();
        }

        var sameType = new List<MoveData>();
        var other    = new List<MoveData>();

        foreach (var m in all)
        {
            if (thing.learnedMoves != null && thing.learnedMoves.Contains(m)) continue;
            if (m.elementType == thing.elementType) sameType.Add(m);
            else                                     other.Add(m);
        }

        Shuffle(sameType);
        Shuffle(other);

        var result = new List<MoveData>(sameType);
        result.AddRange(other);
        return result.GetRange(0, Mathf.Min(count, result.Count));
    }

    static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
