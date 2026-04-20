using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Component gắn vào prefab của mọi Thing trong Battle 
/// Thay thế BattleEnemy.cs cũ.
/// </summary>
public class BattleEntity : MonoBehaviour
{
    [HideInInspector] public GridPos GridPos;
    [HideInInspector] public int TeamId; // 0 = trái (player), 1 = phải (enemy)

    private ThingData _data;
    private int _currentHp;

    public void Init(ThingData data, int teamId)
    {
        _data = data;
        TeamId = teamId;
        _currentHp = data.hp;
        Debug.Log($"[BattleEntity] Spawn {data.thingName} | Team {teamId} | HP {_currentHp}");
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

        // TODO: thay bằng event khi có BattlePhaseManager
        SceneManager.LoadScene("MainScene");
    }
}