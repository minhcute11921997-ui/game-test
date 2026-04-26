using System.Collections.Generic;
using UnityEngine;
public enum WeatherTarget
{
    Both,
    TeamLeft,
    TeamRight,
}

public class WeatherState
{
    public WeatherType type;
    public int turnsLeft;
    public WeatherTarget target;
    public bool isNewThisTurn;
}

public class WeatherManager : MonoBehaviour
{
    public static WeatherManager Instance;

    // Key = WeatherTarget: mỗi sân (Left/Right/Both) 1 slot
    private readonly Dictionary<WeatherTarget, WeatherState> _states = new();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    // ─── Áp thời tiết mới — thời tiết sau xóa thời tiết trước trên cùng sân ───
    public void ApplyWeather(MoveData move, TargetScope scope, int attackerTeamId, GridPos attackTarget)
    {
        // Bước 1: Resolve TargetScope → WeatherTarget nội bộ
        WeatherTarget target;

        if (scope == TargetScope.NoTarget)
        {
            // NoTarget = phủ cả 2 sân (Weather Both cũ)
            target = WeatherTarget.Both;
        }
        else
        {
            // BothSides / EnemySide / OwnSide → xác định sân cụ thể qua ô được click
            var cfg = BattleGridManager.Instance.config;

            if (attackTarget.col <= cfg.LeftMaxCol)
                target = WeatherTarget.TeamLeft;
            else if (attackTarget.col >= cfg.RightMinCol)
                target = WeatherTarget.TeamRight;
            else
                // Ô click nằm trong gap → fallback theo team attacker
                target = attackerTeamId == 0 ? WeatherTarget.TeamRight : WeatherTarget.TeamLeft;
        }

        // Bước 2: Áp vào _states
        if (target == WeatherTarget.Both)
        {
            // Phủ cả 2 sân → xóa hết mọi slot đang có
            _states.Clear();
            _states[WeatherTarget.Both] = new WeatherState
            {
                type = move.weatherType,
                turnsLeft = move.weatherDuration,
                target = WeatherTarget.Both,
                isNewThisTurn = true,
            };
            Debug.Log($"[Weather] {move.weatherType} phủ CẢ 2 SÂN — {move.weatherDuration} lượt");
        }
        else
        {
            // Phủ 1 sân cụ thể
            // Nếu đang có slot Both → tách ra, giữ nguyên sân kia
            if (_states.TryGetValue(WeatherTarget.Both, out var bothState) && bothState.turnsLeft > 0)
            {
                WeatherTarget otherSide = target == WeatherTarget.TeamLeft
                    ? WeatherTarget.TeamRight
                    : WeatherTarget.TeamLeft;

                _states[otherSide] = new WeatherState
                {
                    type = bothState.type,
                    turnsLeft = bothState.turnsLeft,
                    target = otherSide,
                    isNewThisTurn = bothState.isNewThisTurn,
                };
                _states.Remove(WeatherTarget.Both);
                Debug.Log($"[Weather] Tách Both → {otherSide} giữ {bothState.type} ({bothState.turnsLeft} lượt)");
            }

            // Ghi đè sân được chọn
            _states[target] = new WeatherState
            {
                type = move.weatherType,
                turnsLeft = move.weatherDuration,
                target = target,
                isNewThisTurn = true,
            };
            Debug.Log($"[Weather] {move.weatherType} áp vào {target} — {move.weatherDuration} lượt");
        }
    }

    // ─── Lấy thời tiết đang ảnh hưởng team ──────────────────────
    public WeatherType GetWeatherForTeam(int team)
    {
        WeatherTarget teamKey = team == 0 ? WeatherTarget.TeamLeft : WeatherTarget.TeamRight;

        bool hasTeam = _states.TryGetValue(teamKey, out var specific) && specific.turnsLeft > 0;
        bool hasBoth = _states.TryGetValue(WeatherTarget.Both, out var both) && both.turnsLeft > 0;

        if (hasTeam) return specific.type;
        if (hasBoth) return both.type;

        return WeatherType.None;
    }

    // ─── Cuối lượt: giảm đếm, xóa hết hạn ──────────────────────
    public void OnTurnEnd()
    {
        var expired = new List<WeatherTarget>();
        foreach (var kvp in _states)
        {
            var s = kvp.Value;

            if (s.isNewThisTurn)
            {
                s.isNewThisTurn = false;
                Debug.Log($"[Weather] {s.type} ({kvp.Key}) vừa đặt → giữ nguyên {s.turnsLeft} lượt");
                continue;
            }

            s.turnsLeft--;
            Debug.Log($"[Weather] {s.type} ({kvp.Key}) còn {s.turnsLeft} lượt");

            if (s.turnsLeft <= 0)
                expired.Add(kvp.Key);
        }

        foreach (var key in expired)
        {
            Debug.Log($"[Weather] {_states[key].type} hết hạn tại {key} → xóa");
            _states.Remove(key);
        }
    }

    // ─── Query hiệu ứng cụ thể ───────────────────────────────────
    public bool IsBlizzardActive(int team)
        => GetWeatherForTeam(team) == WeatherType.Blizzard;

    public bool IsMagneticFieldActive(int team)
        => GetWeatherForTeam(team) == WeatherType.MagneticField;

    public void ClearAll()
    {
        _states.Clear();
    }

    // ─── Tính AoE bị Bão Tuyết thu nhỏ ──────────────────────────
    public void GetEffectiveAoE(int teamId, AttackShape baseShape, int baseRadius,
                                out AttackShape effShape, out int effRadius)
    {
        effShape = baseShape;
        effRadius = baseRadius;

        if (!IsBlizzardActive(teamId)) return;

        switch (effShape)
        {
            case AttackShape.Square3x3:
                effShape = AttackShape.Square2x2;
                effRadius = Mathf.Max(1, effRadius - 1);
                break;
            case AttackShape.Square2x2:
                effShape = AttackShape.Single;
                effRadius = 1;
                break;
            case AttackShape.Cross:
                effRadius = Mathf.Max(0, effRadius - 1);
                if (effRadius == 0) effShape = AttackShape.Single;
                break;
        }
    }
}