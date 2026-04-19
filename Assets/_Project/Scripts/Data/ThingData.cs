using UnityEngine;

[CreateAssetMenu(fileName = "NewThing", menuName = "Game/Thing Data")]
public class ThingData : ScriptableObject
{
    public string thingName;
    public GameObject prefab; // Hình ảnh/Prefab của Thing trên Overworld
    public GameObject battlePrefab; // Hình ảnh của Thing trên lưới 8x8

    [Header("Tỷ lệ & Xuất hiện")]
    public float spawnWeight = 50f;

    [Header("Chỉ số chiến đấu (Stats)")]
    public int hp = 100;
    public int attack = 20;
    public int defense = 15;
    public float speed = 10f;
}