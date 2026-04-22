using System.Collections.Generic;
using UnityEngine;

public class WeatherState
{
    public WeatherType type;
    public int         turnsLeft;
    public WeatherTarget target;
}

public class WeatherManager : MonoBehaviour
{
    public static WeatherManager Instance;

    // Mỗi phạm vi lưu 1 state (Both / TeamLeft / TeamRight)
    private readonly Dictionary<WeatherTarget, WeatherState> _states = new();

    void Awake() => Instance = this;

    // ─── Áp thời tiết mới ───────────────────────────────────────────────────
    public void ApplyWeather(MoveData move)
    {
        // Đè lên target tương ứng
        _states[move.weatherTarget] = new WeatherState
        {
            type      = move.weatherType,
            turnsLeft = move.envDuration,
            target    = move.weatherTarget,
        };
        Debug.Log($"[Weather] {move.weatherType} áp vào {move.weatherTarget}, {move.envDuration} lượt");
    }

    // ─── Lấy thời tiết đang ảnh hưởng team ──────────────────────────────────
    public WeatherType GetWeatherForTeam(int team)
    {
        WeatherTarget teamKey = team == 0 ? WeatherTarget.TeamLeft : WeatherTarget.TeamRight;

        // Ưu tiên: Both > team cụ thể (cái nào còn lượt thì áp)
        if (_states.TryGetValue(WeatherTarget.Both, out var both) && both.turnsLeft > 0)
            return both.type;
        if (_states.TryGetValue(teamKey, out var specific) && specific.turnsLeft > 0)
            return specific.type;

        return WeatherType.None;
    }

    // ─── Giảm lượt cuối lượt ────────────────────────────────────────────────
    public void OnTurnEnd()
    {
        var keys = new List<WeatherTarget>(_states.Keys);
        foreach (var key in keys)
        {
            if (--_states[key].turnsLeft <= 0)
            {
                Debug.Log($"[Weather] {_states[key].type} hết hạn");
                _states.Remove(key);
            }
        }
    }

    // ─── Query hiệu ứng cụ thể ──────────────────────────────────────────────
    public bool IsBlizzardActive(int team)
        => GetWeatherForTeam(team) == WeatherType.Blizzard;

    public bool IsMagneticFieldActive(int team)
        => GetWeatherForTeam(team) == WeatherType.MagneticField;

    public void ClearAll() => _states.Clear();
}