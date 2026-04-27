// Assets/_Project/Scripts/Data/BookData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewBook", menuName = "Game/Book Data")]
public class BookData : ScriptableObject
{
    public string bookName = "Sách Cơ Bản";
    public Sprite icon;
    [Tooltip("Cộng thêm vào captureChance. Sách thường = 0, sách tốt = 15, sách hiếm = 30")]
    public int captureRateBonus = 0;
    [TextArea] public string description;
}