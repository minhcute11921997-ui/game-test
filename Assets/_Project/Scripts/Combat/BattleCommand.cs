// Assets/_Project/Scripts/Combat/BattleCommand.cs
/// <summary>
/// Lệnh 1 lượt của 1 entity: di chuyển đến đâu, tấn công ô nào
/// </summary>
[System.Serializable]
public struct BattleCommand
{
    public GridPos moveTarget;   // ô sẽ di chuyển đến (= GridPos hiện tại nếu đứng yên)
    public GridPos attackTarget; // ô sẽ tấn công (= GridPos(-1,-1) nếu không tấn công)

    public bool HasAttack => attackTarget.col >= 0;

    public static BattleCommand StayAndAttack(GridPos currentPos, GridPos attackPos)
        => new BattleCommand { moveTarget = currentPos, attackTarget = attackPos };

    public static BattleCommand MoveOnly(GridPos currentPos, GridPos moveTo)
        => new BattleCommand { moveTarget = moveTo, attackTarget = new GridPos(-1, -1) };

    public static BattleCommand MoveAndAttack(GridPos moveTo, GridPos attackPos)
        => new BattleCommand { moveTarget = moveTo, attackTarget = attackPos };
}