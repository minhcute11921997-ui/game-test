using UnityEngine;


[CreateAssetMenu(fileName = "NewThing", menuName = "Game/Thing Data")]

public class ThingData : ScriptableObject
{
    public string thingName;
    public GameObject prefab;
    public GameObject battlePrefab;

    [Header("Tỷ lệ & Xuất hiện")]
    public float spawnWeight = 50f;

    [Header("Hệ nguyên tố")]
    public ElementType elementType = ElementType.Neutral;

    [Header("Chỉ số chiến đấu (Stats)")]

    [Header("AI")]
public AIPersonality aiPersonality = AIPersonality.Aggressive;
    public int hp = 100;
    public int attack = 20;
    public int defense = 15;
    public int spAtk = 15;
    public int spDef = 15;
    public float speed = 10f;
    public int level = 1;      // ← THÊM
    public int luck = 0;

    [Header("Kỹ năng mặc định")]
    public MoveData defaultMove;  // ← MỚI (tạo ở Bước 2)

    [Header("Di chuyển")]
    public int moveRange = 1;
}