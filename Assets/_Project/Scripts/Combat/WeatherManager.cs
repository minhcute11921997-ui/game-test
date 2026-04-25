using System.Collections.Generic;
using UnityEngine;

public class WeatherState
{
    public WeatherType type;
    public int turnsLeft;
    public WeatherTarget target;
    public bool isNewThisTurn;   // ← THÊM: đặt lượt này chưa bị giảm
}

public class WeatherManager : MonoBehaviour
{
    public static WeatherManager Instance;

    // Key = WeatherTarget: mỗi sân (Left/Right/Both) 1 slot → đúng cơ chế
    private readonly Dictionary<WeatherTarget, WeatherState> _states = new();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    // ─── Áp thời tiết mới ────────────────────────────────────────
    public void ApplyWeather(MoveData move, WeatherTarget target)
    {
        _states[target] = new WeatherState
        {
            type = move.weatherType,
            turnsLeft = move.weatherDuration,
            target = target,
            isNewThisTurn = true,   // ← chưa giảm lượt này
        };
        Debug.Log($"[Weather] {move.weatherType} áp vào {target}, {move.weatherDuration} lượt");
    }

    // ─── Lấy thời tiết đang ảnh hưởng team ──────────────────────
    public WeatherType GetWeatherForTeam(int team)
    {
        WeatherTarget teamKey = team == 0 ? WeatherTarget.TeamLeft : WeatherTarget.TeamRight;

        bool hasTeam = _states.TryGetValue(teamKey, out var specific) && specific.turnsLeft > 0;
        bool hasBoth = _states.TryGetValue(WeatherTarget.Both, out var both) && both.turnsLeft > 0;

        // Ưu tiên cái được đặt SAU (ghi đè mới nhất)
        if (hasTeam && hasBoth)
            return specific.turnsLeft >= both.turnsLeft ? specific.type : both.type;

        if (hasTeam) return specific.type;
        if (hasBoth) return both.type;

        return WeatherType.None;
    }

    // ─── Cuối lượt ───────────────────────────────────────────────
    public void OnTurnEnd()
    {
        var expired = new List<WeatherTarget>();
        foreach (var kvp in _states)
        {
            var s = kvp.Value;

            if (s.isNewThisTurn)
            {
                // Đặt lượt này → bỏ qua, không giảm
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
            Debug.Log($"[Weather] {_states[key].type} hết hạn tại {key} → xoá");
            _states.Remove(key);
        }
    }

    public bool IsBlizzardActive(int team)
        => GetWeatherForTeam(team) == WeatherType.Blizzard;

    public bool IsMagneticFieldActive(int team)
        => GetWeatherForTeam(team) == WeatherType.MagneticField;

    public void ClearAll()
    {
        _states.Clear();
    }

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