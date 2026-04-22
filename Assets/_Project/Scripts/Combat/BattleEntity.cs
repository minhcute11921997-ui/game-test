using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleEntity : MonoBehaviour
{
    [HideInInspector] public GridPos GridPos;
    [HideInInspector] public int TeamId;
    [HideInInspector] public EntityHpBar hpBar;

    public int Speed      { get; private set; }
    public int MoveRange  { get; private set; }
    public ThingData Data { get; private set; }
    public int Level => Data != null ? Data.level : 1;

    private int _currentHp;
    public int CurrentHp => _currentHp;
    private int _maxHp;
    public int MaxHp => _maxHp;

    // ── Bẫy Gai ──────────────────────────────────────────────────
    private bool _canMove = true;
    private bool _lockedNextTurn = false;

    public bool CanMove => _canMove;

    // Đánh dấu khoá lượt tới (TerrainManager gọi cuối lượt)
    public void LockMovementNextTurn() => _lockedNextTurn = true;

    // ── Từ Trường ─────────────────────────────────────────────────
    private int _moveCountThisCycle = 0;

    // ── Đầu lượt: áp khoá từ lượt trước rồi reset ────────────────
    public void OnTurnStart()
    {
        _canMove = !_lockedNextTurn; // khoá nếu bị đánh dấu
        _lockedNextTurn = false;     // xoá đánh dấu sau khi đã áp
    }

    public void Init(ThingData data, int teamId)
    {
        Data   = data;
        TeamId = teamId;
        _currentHp = data.hp;
        _maxHp     = data.hp;
        Speed      = (int)data.speed;
        MoveRange  = data.moveRange;
        Debug.Log($"[BattleEntity] Spawn {data.thingName} | Team {teamId} | HP {_currentHp} | Spd {Speed}");
    }

    public MoveData GetMove() => Data != null ? Data.defaultMove : null;

    public void TakeDamage(int dmg, bool isCrit = false)
    {
        _currentHp = Mathf.Max(0, _currentHp - dmg);
        float hpPercent = (float)_currentHp / _maxHp * 100f;
        Debug.Log($"[HP] {Data.thingName}: {_currentHp}/{_maxHp} ({hpPercent:F0}%)");

        DamagePopup.Create(transform.position, dmg, isCrit);

        if (hpBar != null) hpBar.SetHp(_currentHp, _maxHp);

        if (_currentHp <= 0) Die();
    }

    // ── Sau khi di chuyển: đếm Từ Trường ─────────────────────────
    public void OnMoved()
    {
        if (WeatherManager.Instance.IsMagneticFieldActive(TeamId))
        {
            _moveCountThisCycle++;
            if (_moveCountThisCycle >= 3)
                _moveCountThisCycle = 0;
        }
    }

    public bool IsForcedStillByMagnet()
    {
        if (!WeatherManager.Instance.IsMagneticFieldActive(TeamId))
            return false;
        return _moveCountThisCycle == 2;
    }

    void Die()
    {
        Debug.Log($"[Battle] {Data.thingName} bị hạ! Quay về Overworld.");
        BattleGridManager.Instance.RemoveEntity(GridPos);
        Destroy(gameObject);
        SceneManager.LoadScene("MainScene");
    }
}