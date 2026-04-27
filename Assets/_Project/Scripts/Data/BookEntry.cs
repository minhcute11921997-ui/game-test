// Assets/_Project/Scripts/Data/BookEntry.cs
using System;

[Serializable]
public class BookEntry
{
    public BookData bookData;
    public int count;

    public BookEntry(BookData data, int count)
    {
        bookData = data;
        this.count = count;
    }
}