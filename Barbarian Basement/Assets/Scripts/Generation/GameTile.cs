using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
/// <summary>
/// the "type" of tile and what kind of space it belongs to
/// </summary>
///
[System.Serializable]
public enum TileType
{
    Empty,
    Room,
    Corridor
}
/// <summary>
/// a container class holding information about a game tile
/// </summary>
[System.Serializable]
public class GameTile
{
    public HashSet<Direction> Doors = new HashSet<Direction>();
    public TileType Type = TileType.Empty;
    //position in world space
    public Vector3 Position;
    //grid coordinates
    public int x;
    public int y;
    public bool IsFloor = false;
    public bool NorthWall = true;
    public bool SouthWall = true;
    public bool EastWall = true;
    public bool WestWall = true;
    public bool HasDoor = false;
    //does this square have a character on it?
    public bool IsOccupied = false;
    //if this is occupied by a character, store a ref to the character sheet
    public CharacterSheet OccupiedByCharacter;
    //if occupied by an interactable, store a ref to that
    public Interactable OccupiedByInteractable;

    /// <summary>
    /// does this tile have a door on a wall in a given direction?
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public bool HasDoorOn(Direction dir)
    {
        return OccupiedByInteractable is Door door && door.WallDirection == dir;
    }
}
