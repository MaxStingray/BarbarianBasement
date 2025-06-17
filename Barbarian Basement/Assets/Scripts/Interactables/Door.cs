using UnityEngine;

public class Door : Interactable
{
    [SerializeField] private GameObject _doorMesh;

    //the wall on which the door is placed for the tile it's on
    //eg. WallDirection = south if the door is on the south face of the tile
    public Direction WallDirection;
    public override void OnInteract()
    {
        _doorMesh.SetActive(false);

        switch (WallDirection)
        {
            case Direction.North:
                TileData.NorthWall = false;
                break;
            case Direction.South:
                TileData.SouthWall = false;
                break;
            case Direction.East:
                TileData.EastWall = false;
                break;
            case Direction.West:
                TileData.WestWall = false;
                break;
        }
    }
}
