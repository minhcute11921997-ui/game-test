using UnityEngine;
using System.Collections.Generic;

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

    [Header("AI Chiến Đấu")]
    public AIDifficulty   aiDifficulty = AIDifficulty.Medium;
    public ThingArchetype archetype    = ThingArchetype.Attacker;

    [Header("Kỹ Năng Mặc Định")]
    public MoveData defaultMove;

    [Header("Kỹ Năng Bổ Sung")]
    public List<MoveData> moves = new();

    /// <summary>Pool chiêu đầy đủ = defaultMove + moves, không trùng</summary>
    public List<MoveData> AllMoves
    {
        get
        {
            var pool = new List<MoveData>();
            if (defaultMove != null) pool.Add(defaultMove);
            foreach (var m in moves)
                if (m != null && !pool.Contains(m)) pool.Add(m);
            return pool;
        }
    }

    [Header("Chỉ Số Chiến Đấu")]
    public int hp = 100;
    public int attack = 20;
    public int defense = 15;
    public int spAtk = 15;
    public int spDef = 15;
    public float speed = 10f;
    public int level = 1;
    public int luck = 0;

    [Header("Di Chuyển")]
    public int moveRange = 1;
}