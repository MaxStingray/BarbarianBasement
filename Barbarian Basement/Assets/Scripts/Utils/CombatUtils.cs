using UnityEngine;

/// <summary>
/// Utilities class for handling basic combat
/// </summary>
public static class CombatUtils
{
    public static float CombatTurnStartDelay = 1f;
    public static void Attack(CharacterSheet attacker, CharacterSheet target)
    {
        //attacker rolls attack die
        int hits = 0;
        for (int i = 0; i < attacker.AttackDice; i++)
        {
            if (RollAttackDie())
            {
                hits++;
            }
        }

        int blocks = 0;
        for (int i = 0; i < target.DefendDice; i++)
        {
            if (RollDefenceDie(target is Enemy))
            {
                blocks++;
            }
        }

        int actualHits = hits - blocks;
        Debug.Log($"{attacker.CharacterName}: {hits} hits");
        Debug.Log($"{target.CharacterName}: {blocks} blocks");
        Debug.Log($"{target.CharacterName} takes {Mathf.Max(0, actualHits)} damage!");

        if (actualHits > 0)
        {
            target.TakeHits(actualHits);
        }
    }

    /// <summary>
    /// roll one attack die (3 sides of the attack die are skulls, therefore we always have a 50% chance of hitting)
    /// </summary>
    /// <returns> true if hit</returns>
    public static bool RollAttackDie()
    {
        var hitNumber = Random.Range(0, 100);

        if (hitNumber < 50)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// roll one Defence die
    /// enemies have a one in 6 chance to block
    /// heroes have 2 in 6
    /// </summary>
    /// <param name="isEnemy"></param>
    /// <returns>true if blocked</returns>
    public static bool RollDefenceDie(bool isEnemy)
    {
        var blockNumber = Random.Range(0, 100);

        if (isEnemy)
        {
            //round it to 17
            if (blockNumber < 17)
            {
                return true;
            }
        }
        else
        {
            //rounded down
            if (blockNumber < 33)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// from the current tile, is a specified tile visible?
    /// use Bresenham's line algorithm and check for blocking walls
    /// </summary>
    /// <param name="startTile"></param>
    /// <param name="endTile"></param>
    /// <returns>true if target is visible</returns>
    public static bool HasLineOfSight(GameTile startTile, GameTile endTile, GameTile[,] grid)
    {
        int x0 = startTile.x;
        int y0 = startTile.y;
        int x1 = endTile.x;
        int y1 = endTile.y;

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);

        int sx = (x0 < x1) ? 1 : -1;
        int sy = (y0 < y1) ? 1 : -1;

        int err = dx - dy;

        while (true)
        {
            // Reached the destination tile
            if (x0 == x1 && y0 == y1)
            {
                break;
            }

            int nextX = x0;
            int nextY = y0;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                nextX += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                nextY += sy;
            }

            // Check bounds
            if (nextX < 0 || nextX >= grid.GetLength(0) || nextY < 0 || nextY >= grid.GetLength(1))
            {
                return false;
            }

            GameTile currentTile = grid[x0, y0];
            GameTile nextTile = grid[nextX, nextY];

            // Determine direction of movement
            if (nextX > x0)
            {
                if (currentTile.EastWall || nextTile.WestWall)
                    return false;
            }
            else if (nextX < x0)
            {
                if (currentTile.WestWall || nextTile.EastWall)
                    return false;
            }

            if (nextY > y0)
            {
                if (currentTile.NorthWall || nextTile.SouthWall)
                    return false;
            }
            else if (nextY < y0)
            {
                if (currentTile.SouthWall || nextTile.NorthWall)
                    return false;
            }

            // Move to the next tile
            x0 = nextX;
            y0 = nextY;
        }

        return true;
    }
}
