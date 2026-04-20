using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleEntity : MonoBehaviour
{
    [HideInInspector] public GridPos GridPos;
    [HideInInspector] public int TeamId;

    public int Speed { get; private set; }
    public int MoveRange { get; private set; } // ← đọc từ ThingData

    private ThingData _data;
    private int _currentHp;

    public void Init(ThingData data, int teamId)
    {
        _data = data;
        TeamId = teamId;
        _currentHp = data.hp;
        Speed = (int)data.speed;
        MoveRange = data.moveRange; // ← đọc từ data, không hardcode
        Debug.Log($"[BattleEntity] Spawn {data.thingName} | Team {teamId} | HP {_currentHp} | Speed {Speed} | MoveRange {MoveRange}");
    }

    public void TakeDamage(int dmg)
    {
        _currentHp -= dmg;
        Debug.Log($"{_data.thingName} còn {_currentHp} HP");
        if (_currentHp <= 0) Die();
    }

    void Die()
    {
        Debug.Log($"{_data.thingName} bị hạ!");
        BattleGridManager.Instance.RemoveEntity(GridPos);
        Destroy(gameObject);
        SceneManager.LoadScene("MainScene");
    }
}