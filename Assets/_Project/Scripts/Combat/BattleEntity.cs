using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleEntity : MonoBehaviour
{
    [HideInInspector] public GridPos GridPos;
    [HideInInspector] public int TeamId;
    [HideInInspector] public EntityHpBar hpBar;

    public int Speed { get; private set; }
    public int MoveRange { get; private set; }
    public ThingData Data { get; private set; }  // ← MỚI: expose để CombatCalculator dùng
    public int Level => Data != null ? Data.level : 1;

    private int _currentHp;
    public int CurrentHp => _currentHp;
    private int _maxHp;

    public void Init(ThingData data, int teamId)
    {
        Data = data;
        TeamId = teamId;
        _currentHp = data.hp;
        _maxHp = data.hp;
        Speed = (int)data.speed;
        MoveRange = data.moveRange;
        Debug.Log($"[BattleEntity] Spawn {data.thingName} | Team {teamId} | HP {_currentHp} | Spd {Speed}");
    }

    /// <summary>Lấy move của entity (hiện tại dùng defaultMove từ ThingData)</summary>
    public MoveData GetMove() => Data != null ? Data.defaultMove : null;

    public void TakeDamage(int dmg, bool isCrit = false)
    {
        _currentHp = Mathf.Max(0, _currentHp - dmg);
        float hpPercent = (float)_currentHp / _maxHp * 100f;
        Debug.Log($"[HP] {Data.thingName}: {_currentHp}/{_maxHp} ({hpPercent:F0}%)");

        // Sprint 3: hiện damage popup
        DamagePopup.Create(transform.position, dmg, isCrit);

        // Sprint 3: cập nhật HP bar
        if (hpBar != null) hpBar.SetHp(_currentHp, _maxHp);

        if (_currentHp <= 0) Die();
    }

    void Die()
    {
        Debug.Log($"[Battle] {Data.thingName} bị hạ! Quay về Overworld.");
        BattleGridManager.Instance.RemoveEntity(GridPos);
        Destroy(gameObject);
        SceneManager.LoadScene("MainScene");
    }
}