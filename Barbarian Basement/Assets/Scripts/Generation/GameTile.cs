using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The "type" of tile and what kind of space it belongs to
/// </summary>
[System.Serializable]
public enum TileType
{
    Empty,
    Room,
    Corridor
}

/// <summary>
/// A container class holding information about a game tile
/// </summary>
[System.Serializable]
public class GameTile
{
    public TileType Type = TileType.Empty;

    // Position in world space
    public Vector3 Position;

    // Grid coordinates
    public int x;
    public int y;

    public bool IsFloor = false;

    // Used for visual wall instantiation (not logic)
    public bool NorthWall = true;
    public bool SouthWall = true;
    public bool EastWall = true;
    public bool WestWall = true;

    // Movement-blocking logic (decoupled from visuals)
    public HashSet<Direction> BlockedDirections = new HashSet<Direction>();

    // Dynamic occupancy
    public bool IsOccupied = false;
    public CharacterSheet OccupiedByCharacter;
    public Interactable OccupiedByInteractable;

    public bool HasDoor => OccupiedByInteractable is Door;

    public bool IsBlocked(Direction dir) => BlockedDirections.Contains(dir);

    public void SetBlocked(Direction dir, bool blocked)
    {
        if (blocked)
            BlockedDirections.Add(dir);
        else
            BlockedDirections.Remove(dir);
    }
}