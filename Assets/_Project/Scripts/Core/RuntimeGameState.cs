using UnityEngine;
using System.Collections.Generic;

public static class RuntimeGameState
{
    public static ThingData CurrentEnemy;
    public static List<ThingData> Party = new();
    public static List<BookEntry> BookInventory = new(); // ← THÊM

    public static ThingData ActiveThing => Party.Count > 0 ? Party[0] : null;

    public static void ResetForNewSession()
    {
        Party.Clear();
        CurrentEnemy = null;
        // BookInventory KHÔNG clear — tồn tại xuyên session
    }

    // Helper trừ sách sau khi dùng
    public static bool UseBook(BookData book)
    {
        var entry = BookInventory.Find(e => e.bookData == book);
        if (entry == null || entry.count <= 0) return false;
        entry.count--;
        if (entry.count <= 0) BookInventory.Remove(entry);
        return true;
    }
}