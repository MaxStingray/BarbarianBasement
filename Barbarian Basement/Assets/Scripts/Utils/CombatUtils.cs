using System.Collections;
using UnityEngine;

/// <summary>
/// Utilities class for handling basic combat
/// </summary>
public static class CombatUtils
{
    public static float CombatTurnStartDelay = 1f;
    public static void Attack(int attackDice, CharacterSheet attacker, CharacterSheet target, out bool hit)
    {
        hit = false;
        int hits = 0;
        for (int i = 0; i < attackDice; i++)
        {
            if (RollAttackDie())
                hits++;
        }

        int blocks = 0;
        for (int i = 0; i < target.CurrentDefendDice; i++)
        {
            if (RollDefenceDie(target is Enemy))
                blocks++;
        }

        int actualHits = hits - blocks;

        Debug.Log($"{attacker.CharacterName}: {hits} hits");
        Debug.Log($"{target.CharacterName}: {blocks} blocks");
        Debug.Log($"{target.CharacterName} takes {Mathf.Max(0, actualHits)} damage!");

        if (actualHits > 0)
        {
            hit = true;
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

    public static int RollMovementDie()
    {
        return Random.Range(1, 6);
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
            if (x0 == x1 && y0 == y1)
                break;

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

            // Bounds check
            if (nextX < 0 || nextX >= grid.GetLength(0) || nextY < 0 || nextY >= grid.GetLength(1))
                return false;

            GameTile currentTile = grid[x0, y0];
            GameTile nextTile = grid[nextX, nextY];

            // Determine direction of movement
            if (nextX > x0 && (currentTile.IsBlocked(Direction.East) || nextTile.IsBlocked(Direction.West)))
                return false;
            if (nextX < x0 && (currentTile.IsBlocked(Direction.West) || nextTile.IsBlocked(Direction.East)))
                return false;
            if (nextY > y0 && (currentTile.IsBlocked(Direction.North) || nextTile.IsBlocked(Direction.South)))
                return false;
            if (nextY < y0 && (currentTile.IsBlocked(Direction.South) || nextTile.IsBlocked(Direction.North)))
                return false;

            x0 = nextX;
            y0 = nextY;
        }

        return true;
    }

    /// <summary>
    /// checks if there is a valid target in range and returns its game tile if so
    /// </summary>
    /// <param name="attacker"></param>
    /// <param name="range"></param>
    /// <param name="targetTile">the tile the target is occupying</param>
    /// <returns></returns>
    public static bool ValidTargetForRanged(CharacterSheet attacker, int range, GameTile[,] grid, out GameTile targetTile)
    {
        //placeholder code
        targetTile = null;

        GameTile currentTile = attacker.CurrentTile;
        Direction direction = attacker.FacingDirection;

        int steps = 0;

        while (true)
        {
            GameTile nextTile = MoveUtils.GetTargetTile(currentTile, direction, grid);

            //ensure a valid tile
            if (nextTile == null)
            {
                return false;
            }

            //check whether the next move will be blocked by a wall or obstacle
            if (currentTile.IsBlocked(direction) || nextTile.IsBlocked(MoveUtils.GetOppositeDirection(direction)))
            {
                return false;
            }

            if (nextTile.IsOccupied && nextTile.OccupiedByCharacter != null)
            {
                targetTile = nextTile;
                return true;
            }

            //move to the next step
            steps++;

            //check range
            if (range > 0 && steps >= range)
                return false;

            currentTile = nextTile;
        }

    }

    /// <summary>
    /// plays the attack sfx and visuals related to the current character class
    /// </summary>
    /// <param name="attacker"></param>
    /// <param name="hit"></param>
    /// <returns></returns>
    public static IEnumerator PlayCharacterAttackEffects(Player attacker, bool hit)
    {
        if (hit)
        {
            attacker.PlayHitEffect();
            attacker.HitConfirmSound();
        }
        else
        {
            attacker.SwingSound();
        }

        yield return new WaitForSeconds(CombatTurnStartDelay);
    }
}
