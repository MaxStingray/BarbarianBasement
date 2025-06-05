using UnityEngine;

public enum Direction
{
    North,
    South,
    East,
    West
}
/// <summary>
/// Utilities related to movement (tile inspection etc)
/// </summary>
public static class MoveUtils
{
    /// <summary>
    /// can we move to an adjacent tile from our target tile?
    /// </summary>
    /// <param name="currentTile"></param>
    /// <param name="targetTile"></param>
    /// <returns>true if move is valid</returns>
    public static bool CanMoveToTile(GameTile currentTile, Direction direction, GameTile[,] grid)
    {
        int x = -1, y = -1;

        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                if (grid[i, j] == currentTile)
                {
                    x = i;
                    y = j;
                    break;
                }
            }
            if (x != -1) break;
        }

        if (x == -1 || y == -1)
        {
            Debug.LogError("current tile not found in grid");
            return false;
        }

        switch (direction)
        {
            case Direction.North:
                if (y + 1 >= grid.GetLength(1)) return false;
                return !currentTile.NorthWall && !grid[x, y + 1].SouthWall && !grid[x, y + 1].IsOccupied;
            case Direction.South:
                if (y - 1 < 0) return false;
                return !currentTile.SouthWall && !grid[x, y - 1].NorthWall && !grid[x, y - 1].IsOccupied;
            case Direction.East:
                if (x + 1 >= grid.GetLength(0)) return false;
                return !currentTile.EastWall && !grid[x + 1, y].WestWall && !grid[x + 1, y].IsOccupied;
            case Direction.West:
                if (x - 1 < 0) return false;
                return !currentTile.WestWall && !grid[x - 1, y].EastWall && !grid[x - 1, y].IsOccupied;
            default:
                return false;
        }
    }

    /// <summary>
    /// retrieves a target tile from the holy grid, using direction
    /// </summary>
    /// <param name="currentTile"></param>
    /// <param name="direction"></param>
    /// <param name="grid"></param>
    /// <returns>a tile (hopefully)</returns>
    public static GameTile GetTargetTile(GameTile currentTile, Direction direction, GameTile[,] grid)
    {
        int x = -1, y = -1;
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                if (grid[i, j] == currentTile)
                {
                    x = i;
                    y = j;
                    break;
                }
            }
            if (x != -1) break;
        }

        if (x == -1 || y == -1)
        {
            Debug.LogError("Current tile not found in grid.");
            return null;
        }

        switch (direction)
        {
            case Direction.North:
                return y + 1 < grid.GetLength(1) ? grid[x, y + 1] : null;
            case Direction.South:
                return y - 1 >= 0 ? grid[x, y - 1] : null;
            case Direction.East:
                return x + 1 < grid.GetLength(0) ? grid[x + 1, y] : null;
            case Direction.West:
                return x - 1 >= 0 ? grid[x - 1, y] : null;
            default:
                return null;
        }
    }
}
