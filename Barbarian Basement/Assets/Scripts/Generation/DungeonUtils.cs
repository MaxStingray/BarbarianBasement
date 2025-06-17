using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class DungeonUtils
{
    /// <summary>
    /// gets a list of leaf nodes
    /// </summary>
    /// <param name="root"></param>
    /// <returns></returns>
    public static List<BSPNode> GetLeafNodes(BSPNode root)
    {
        List<BSPNode> leaves = new List<BSPNode>();
        Queue<BSPNode> queue = new Queue<BSPNode>();
        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            BSPNode node = queue.Dequeue();
            if (node.IsLeaf)
            {
                leaves.Add(node);
            }
            else
            {
                if (node.Left != null) queue.Enqueue(node.Left);
                if (node.Right != null) queue.Enqueue(node.Right);
            }
        }

        return leaves;
    }

    /// <summary>
    /// returns direction offset 
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static Vector2Int GetDirectionOffset(Direction dir)
    {
        switch (dir)
        {
            case Direction.North: return new Vector2Int(0, 1);
            case Direction.South: return new Vector2Int(0, -1);
            case Direction.East: return new Vector2Int(1, 0);
            case Direction.West: return new Vector2Int(-1, 0);
        }
        return Vector2Int.zero;
    }

    public static Direction GetOppositeDirection(Direction dir)
    {
        switch (dir)
        {
            case Direction.North: return Direction.South;
            case Direction.South: return Direction.North;
            case Direction.East:  return Direction.West;
            case Direction.West:  return Direction.East;
            default: return dir;
        }
    }


    /// <summary>
    /// check if a given tile is in a corridor
    /// </summary>
    /// <param name="tile"></param>
    /// <returns></returns>
    public static bool IsCorridorTile(GameTile tile) => tile.Type == TileType.Corridor;


    /// <summary>
    /// check if a given tile is in a room
    /// </summary>
    /// <param name="tile"></param>
    /// <returns></returns>
    public static bool IsRoomTile(GameTile tile) => tile.Type == TileType.Room;

    /// <summary>
    /// determines if this tile is "safe" (not adjacent to a corridoor)
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static bool IsSafeTile(int rows, int cols, int x, int y, GameTile[,] grid)
    {
        // Defensive check
        if (x < 0 || y < 0 || x >= rows || y >= cols)
            return false;

        // Skip if not a floor
        if (!grid[x, y].IsFloor)
            return false;

        // Check surrounding tiles
        int corridorCount = 0;

        // Check cardinal directions
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),  // North
            new Vector2Int(0, -1), // South
            new Vector2Int(1, 0),  // East
            new Vector2Int(-1, 0)  // West
        };

        foreach (var dir in directions)
        {
            int nx = x + dir.x;
            int ny = y + dir.y;
            if (nx >= 0 && ny >= 0 && nx < rows && ny < cols)
            {
                if (!grid[nx, ny].IsFloor)
                    continue;

                // If that adjacent tile is a corridor
                if (IsCorridorTile(grid[nx, ny]))
                {
                    corridorCount++;
                }
            }
        }

        // If adjacent to 2+ corridors, treat as a corridor entry point — avoid it!
        if (corridorCount >= 2)
            return false;

        return true;
    }

    /// <summary>
    /// Gets a random FloorTile from the collection
    /// </summary>
    /// <param name="floorTiles"></param>
    /// <returns></returns>
    public static GameObject GetRandomFloorTile(FloorTile[] floorTiles)
    {
        int totalWeight = 0;
        foreach (var tile in floorTiles) totalWeight += tile.Weight;
        int rand = Random.Range(0, totalWeight);
        int runningWeight = 0;
        foreach (var tile in floorTiles)
        {
            runningWeight += tile.Weight;
            if (rand < runningWeight) return tile.Prefab;
        }
        return floorTiles[0].Prefab;
    }

    /// <summary>
    /// Gets a random WallTile from the collection
    /// </summary>
    /// <param name="wallTiles"></param>
    /// <returns></returns>
    public static GameObject GetRandomWallTile(WallTile[] wallTiles)
    {
        int totalWeight = 0;
        foreach (var tile in wallTiles) totalWeight += tile.Weight;
        int rand = Random.Range(0, totalWeight);
        int runningWeight = 0;
        foreach (var tile in wallTiles)
        {
            runningWeight += tile.Weight;
            if (rand < runningWeight) return tile.Prefab;
        }
        return wallTiles[0].Prefab;
    }

}
