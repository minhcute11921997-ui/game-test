using UnityEngine;

[CreateAssetMenu(fileName = "BattleGridConfig", menuName = "Battle/Grid Config")]
public class BattleGridConfig : ScriptableObject
{
    [Header("Kích thước sân")]
    public int boardCols = 8;
    public int boardRows = 8;
    public int gapCols = 2;

    // ── Computed ──────────────────────────────────────────
    public int TotalCols => boardCols * 2 + gapCols;  // = 18
    public int LeftMaxCol => boardCols - 1;             // = 7
    public int GapMinCol => boardCols;                 // = 8
    public int GapMaxCol => boardCols + gapCols - 1;   // = 9
    public int RightMinCol => boardCols + gapCols;       // = 10

    public bool IsGap(int col)
        => col >= GapMinCol && col <= GapMaxCol;

    public int GetTeam(int col)
    {
        if (col <= LeftMaxCol) return 0; // team trái (người chơi)
        if (col >= RightMinCol) return 1; // team phải (địch)
        return -1; // gap
    }

    public bool IsInBounds(int col, int row)
        => col >= 0 && col < TotalCols && row >= 0 && row < boardRows;

    public bool IsWalkable(int col, int row)
        => IsInBounds(col, row) && !IsGap(col);
}
