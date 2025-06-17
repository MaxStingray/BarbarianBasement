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
            Debug.LogError("Current tile not found in grid");
            return false;
        }

        switch (direction)
        {
            case Direction.North:
                if (y + 1 >= grid.GetLength(1)) return false;
                return !currentTile.IsBlocked(Direction.North) &&
                       !grid[x, y + 1].IsBlocked(Direction.South) &&
                       !grid[x, y + 1].IsOccupied;
            case Direction.South:
                if (y - 1 < 0) return false;
                return !currentTile.IsBlocked(Direction.South) &&
                       !grid[x, y - 1].IsBlocked(Direction.North) &&
                       !grid[x, y - 1].IsOccupied;
            case Direction.East:
                if (x + 1 >= grid.GetLength(0)) return false;
                return !currentTile.IsBlocked(Direction.East) &&
                       !grid[x + 1, y].IsBlocked(Direction.West) &&
                       !grid[x + 1, y].IsOccupied;
            case Direction.West:
                if (x - 1 < 0) return false;
                return !currentTile.IsBlocked(Direction.West) &&
                       !grid[x - 1, y].IsBlocked(Direction.East) &&
                       !grid[x - 1, y].IsOccupied;
            default:
                return false;
        }
    }

    public static bool TargetTileReached(GameTile currentTile, GameTile targetTile, GameTile[,] grid, out Direction requiredDirection)
    {
        int maxX = grid.GetLength(0);
        int maxY = grid.GetLength(1);

        int currentX = currentTile.x;
        int currentY = currentTile.y;

        (int dx, int dy, Direction dir)[] directions = new[]
        {
            (0, 1, Direction.North),
            (0, -1, Direction.South),
            (1, 0, Direction.East),
            (-1, 0, Direction.West)
        };

        foreach (var (dx, dy, dir) in directions)
        {
            int newX = currentX + dx;
            int newY = currentY + dy;

            if (newX >= 0 && newX < maxX && newY >= 0 && newY < maxY)
            {
                if (targetTile == grid[newX, newY])
                {
                    requiredDirection = dir;
                    if (!currentTile.IsBlocked(dir) &&
                        !targetTile.IsBlocked(GetOppositeDirection(dir)))
                    {
                        return true;
                    }
                }
            }
        }

        requiredDirection = Direction.North; // fallback
        return false;
    }

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

        return direction switch
        {
            Direction.North => (y + 1 < grid.GetLength(1)) ? grid[x, y + 1] : null,
            Direction.South => (y - 1 >= 0) ? grid[x, y - 1] : null,
            Direction.East => (x + 1 < grid.GetLength(0)) ? grid[x + 1, y] : null,
            Direction.West => (x - 1 >= 0) ? grid[x - 1, y] : null,
            _ => null
        };
    }

    public static Direction GetOppositeDirection(Direction dir)
    {
        return dir switch
        {
            Direction.North => Direction.South,
            Direction.South => Direction.North,
            Direction.East => Direction.West,
            Direction.West => Direction.East,
            _ => dir
        };
    }
}