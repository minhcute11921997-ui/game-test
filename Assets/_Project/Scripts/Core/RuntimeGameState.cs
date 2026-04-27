using UnityEngine;
using System.Collections.Generic;

public static class RuntimeGameState
{
    public static ThingData CurrentEnemy;
    public static List<ThingData> Party = new();
    public static List<BookEntry> BookInventory = new(); // ← THÊM MỚI

    public static ThingData ActiveThing => Party.Count > 0 ? Party[0] : null;

    public static void ResetForNewSession()
    {
        Party.Clear();
        CurrentEnemy = null;
        // BookInventory KHÔNG clear — tồn tại xuyên session
    }

    public static bool UseBook(BookData book)
    {
        var entry = BookInventory.Find(e => e.bookData == book);
        if (entry == null || entry.count <= 0) return false;
        entry.count--;
        if (entry.count <= 0) BookInventory.Remove(entry);
        Debug.Log($"[Inventory] Dùng {book.bookName} | Còn: {entry?.count ?? 0}");
        return true;
    }

    public static void AddBook(BookData book, int amount = 1)
    {
        var entry = BookInventory.Find(e => e.bookData == book);
        if (entry != null) entry.count += amount;
        else BookInventory.Add(new BookEntry(book, amount));
    }
}