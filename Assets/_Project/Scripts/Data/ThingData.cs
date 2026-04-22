using UnityEngine;


[CreateAssetMenu(fileName = "NewThing", menuName = "Game/Thing Data")]

public class ThingData : ScriptableObject
{
    [Header("Tên & Prefab")]
public string thingName;
public GameObject prefab;
public GameObject battlePrefab;

[Header("Tỷ lệ & Xuất hiện")]
public float spawnWeight = 50f;

[Header("Hệ Nguyên Tố")]
public ElementType elementType = ElementType.Neutral;

[Header("Tính Cách AI")]
public AIPersonality aiPersonality = AIPersonality.Aggressive;

[Header("Chỉ Số Chiến Đấu")]
public int hp = 100;
public int attack = 20;
public int defense = 15;
public int spAtk = 15;
public int spDef = 15;
public float speed = 10f;
public int level = 1;
public int luck = 0;

[Header("Kỹ Năng Mặc Định")]
public MoveData defaultMove;

[Header("Di Chuyển")]
public int moveRange = 1;
}