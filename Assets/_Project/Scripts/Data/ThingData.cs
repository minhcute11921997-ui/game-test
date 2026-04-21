using UnityEngine;
using System.Collections.Generic;


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
    public int hp = 100;
    public int attack = 20;
    public int defense = 15;
    public int spAtk = 15;
    public int spDef = 15;
    public float speed = 10f;
    public int level = 1;
    public int luck = 0;

    [Header("Kỹ năng mặc định")]
    public MoveData defaultMove;

    [Header("Di chuyển")]
    public int moveRange = 1;

    [Header("Tộc (Sprint 9)")]
    public TribeType tribeType = TribeType.None;

    [Header("Progression (Sprint 7)")]
    public int expYield = 50;
    [System.NonSerialized] public int experience = 0; // runtime only — resets each play session

    [Header("Kỹ năng đã học (Sprint 7)")]
    public List<MoveData> learnedMoves = new List<MoveData>(); // tối đa 4 trang bị + kho
}