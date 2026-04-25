using System.Collections.Generic;
using UnityEngine;

public class WeatherState
{
    public WeatherType type;
    public int turnsLeft;
    public WeatherTarget target;
    public int appliedAt;
}

public class WeatherManager : MonoBehaviour
{
    public static WeatherManager Instance;

    // Mỗi phạm vi lưu 1 state (Both / TeamLeft / TeamRight)
    private readonly Dictionary<WeatherTarget, WeatherState> _states = new();

    // Đếm thứ tự apply thực tế, để chiêu resolve sau thắng
    private int _applyOrderCounter = 0;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ─── Áp thời tiết mới ─────────────────────────────────────────
    public void ApplyWeather(MoveData move, WeatherTarget target)
    {
        _states[target] = new WeatherState
        {
            type = move.weatherType,
            turnsLeft = move.envDuration,
            target = target,
            appliedAt = ++_applyOrderCounter
        };

        Debug.Log($"[Weather] {move.weatherType} áp vào {target}, {move.envDuration} lượt (order {_applyOrderCounter})");
    }

    // ─── Lấy thời tiết đang ảnh hưởng team ───────────────────────
    public WeatherType GetWeatherForTeam(int team)
    {
        WeatherTarget teamKey = team == 0 ? WeatherTarget.TeamLeft : WeatherTarget.TeamRight;

        bool hasTeam = _states.TryGetValue(teamKey, out var specific) && specific.turnsLeft > 0;
        bool hasBoth = _states.TryGetValue(WeatherTarget.Both, out var both) && both.turnsLeft > 0;

        if (hasTeam && hasBoth)
            return specific.appliedAt >= both.appliedAt ? specific.type : both.type;

        if (hasTeam) return specific.type;
        if (hasBoth) return both.type;

        return WeatherType.None;
    }

    // ─── Giảm lượt cuối/đầu lượt ──────────────────────────────────
    public void OnTurnEnd()
    {
        var keys = new List<WeatherTarget>(_states.Keys);
        foreach (var key in keys)
        {
            if (--_states[key].turnsLeft <= 0)
            {
                Debug.Log($"[Weather] {_states[key].type} hết hạn tại {key}");
                _states.Remove(key);
            }
        }
    }

    // ─── Query hiệu ứng cụ thể ────────────────────────────────────
    public bool IsBlizzardActive(int team)
        => GetWeatherForTeam(team) == WeatherType.Blizzard;

    public bool IsMagneticFieldActive(int team)
        => GetWeatherForTeam(team) == WeatherType.MagneticField;

    public void ClearAll()
    {
        _states.Clear();
        _applyOrderCounter = 0;
    }

    public void GetEffectiveAoE(int teamId, AttackShape baseShape, int baseRadius, out AttackShape effShape, out int effRadius)
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