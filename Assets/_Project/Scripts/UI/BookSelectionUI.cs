// Assets/_Project/Scripts/UI/BookSelectionUI.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BookSelectionUI : MonoBehaviour
{
    public static BookSelectionUI Instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private Transform bookListContainer; // ScrollView content
    [SerializeField] private GameObject bookButtonPrefab; // prefab 1 nút sách
    [SerializeField] private Button btnCancel;

    private Action<BookData> _onChosen;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }
    void OnDestroy() { if (Instance == this) Instance = null; }
    void Start()
    {
        Hide();
        btnCancel.onClick.AddListener(() => { Hide(); _onChosen?.Invoke(null); });
    }

    public void Show(Action<BookData> onChosen)
    {
        _onChosen = onChosen;

        // Xóa nút cũ
        for (int i = bookListContainer.childCount - 1; i >= 0; i--)
            Destroy(bookListContainer.GetChild(i).gameObject);

        // Sinh nút theo inventory
        var inventory = RuntimeGameState.BookInventory;
        if (inventory.Count == 0)
        {
            Debug.Log("[BookUI] Không có sách nào trong túi!");
            // Có thể show text "Túi trống"
        }

        foreach (var entry in inventory)
        {
            if (entry.count <= 0) continue;
            var go = Instantiate(bookButtonPrefab, bookListContainer);
            var btn = go.GetComponent<Button>();
            var txtName = go.transform.Find("TxtName")?.GetComponent<TextMeshProUGUI>();
            var txtCount = go.transform.Find("TxtCount")?.GetComponent<TextMeshProUGUI>();
            var img = go.transform.Find("Icon")?.GetComponent<Image>();

            if (txtName) txtName.text = entry.bookData.bookName;
            if (txtCount) txtCount.text = $"x{entry.count}";
            if (img && entry.bookData.icon) img.sprite = entry.bookData.icon;

            var capturedBook = entry.bookData; // capture cho lambda
            btn.onClick.AddListener(() =>
            {
                Hide();
                _onChosen?.Invoke(capturedBook);
            });
        }

        panel.SetActive(true);
    }

    public void Hide() => panel.SetActive(false);
}