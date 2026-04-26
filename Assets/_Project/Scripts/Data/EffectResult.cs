// Assets/_Project/Scripts/Data/EffectResult.cs
using System.Collections.Generic;

public enum EffectResultType { Damage, Heal, StatStage, Terrain, Weather }

public class EffectResult
{
    public bool triggered;
    public EffectResultType resultType;
    public List<(BattleEntity target, int value)> hits = new();
    public string logMessage;

    // StatStage: lưu thêm info để animate
    public StatType statType;
    public int statDelta;

    // Terrain / Weather: không cần value trong hits, chỉ log là đủ
}