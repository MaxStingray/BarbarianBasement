using UnityEngine;
[System.Serializable]
public class GameTile
{
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
    //does this square have a character on it?
    public bool IsOccupied = false;
    //if this is occupied by a character, store a ref to the character sheet
    public CharacterSheet OccupiedByCharacter;
    //if occupied by an interactable, store a ref to that
    public Iinteractable OccupiedByInteractable;
}
