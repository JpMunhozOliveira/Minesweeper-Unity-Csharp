using UnityEngine;

public struct Cell
{
    public enum TypeCell {
        Invalid,
        Empty,
        Mine,
        Number,
    }

    public Vector3Int Position;
    public TypeCell Type;
    public int Number;
    public bool Revealed;
    public bool Flagged;
    public bool Exploded;
}
