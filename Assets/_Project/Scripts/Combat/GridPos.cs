using System;
using UnityEngine;

[Serializable]
public struct GridPos : IEquatable<GridPos>
{
    public int col, row;

    public GridPos(int col, int row) { this.col = col; this.row = row; }

    public bool Equals(GridPos other) => col == other.col && row == other.row;
    public override bool Equals(object obj) => obj is GridPos g && Equals(g);
    public override int GetHashCode() => HashCode.Combine(col, row);
    public override string ToString() => $"({col},{row})";

    public static bool operator ==(GridPos a, GridPos b) => a.Equals(b);
    public static bool operator !=(GridPos a, GridPos b) => !a.Equals(b);

    public GridPos Step(GridPos dir) => new(col + dir.col, row + dir.row);

    public static int ManhattanDist(GridPos a, GridPos b)
        => Mathf.Abs(a.col - b.col) + Mathf.Abs(a.row - b.row);
}