using UnityEngine;

public class SetStarterThing : MonoBehaviour
{
    public ThingData starterThing;

    [Header("Test Book — Xóa sau khi có inventory thật")]
    [SerializeField] private BookData testBook;

    void Awake()
    {
        if (RuntimeGameState.Party.Count == 0 && starterThing != null)
            RuntimeGameState.Party.Add(starterThing);

        // TEST: thêm 5 sách vào túi
        if (testBook != null && RuntimeGameState.BookInventory.Count == 0)
            RuntimeGameState.AddBook(testBook, 5);
    }
}