using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BookSelectionUI : MonoBehaviour
{
    public static BookSelectionUI Instance { get; private set; }

    [Header("Panel Root")]
    [SerializeField] private GameObject panel;

    [Header("List")]
    [SerializeField] private Transform bookListContainer;
    [SerializeField] private GameObject bookButtonPrefab;

    [Header("Buttons")]
    [SerializeField] private Button btnCancel;

    [Header("Empty State")]
    [SerializeField] private GameObject emptyLabel;

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
        for (int i = bookListContainer.childCount - 1; i >= 0; i--)
            Destroy(bookListContainer.GetChild(i).gameObject);

        bool hasItems = false;
        foreach (var entry in RuntimeGameState.BookInventory)
        {
            if (entry?.bookData == null || entry.count <= 0) continue;
            hasItems = true;

            var go = Instantiate(bookButtonPrefab, bookListContainer);
            go.transform.Find("TxtName")?.GetComponent<TextMeshProUGUI>()
                ?.SetText(entry.bookData.bookName);
            go.transform.Find("TxtCount")?.GetComponent<TextMeshProUGUI>()
                ?.SetText($"x{entry.count}");
            go.transform.Find("TxtBonus")?.GetComponent<TextMeshProUGUI>()
                ?.SetText(entry.bookData.captureRateBonus > 0 ? $"+{entry.bookData.captureRateBonus}%" : "");
            var iconComp = go.transform.Find("Icon")?.GetComponent<Image>();
            if (iconComp && entry.bookData.icon) iconComp.sprite = entry.bookData.icon;

            var captured = entry.bookData;
            go.GetComponent<Button>()?.onClick.AddListener(() => { Hide(); _onChosen?.Invoke(captured); });
        }

        if (emptyLabel) emptyLabel.SetActive(!hasItems);
        panel.SetActive(true);
    }

    public void Hide() => panel?.SetActive(false);
}