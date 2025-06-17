using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.Analytics;

[System.Serializable]
public class FloorTile
{
    public GameObject Prefab;
    public int Weight;
    public string Name;
}

[System.Serializable]
public class WallTile
{
    public GameObject Prefab;
    public int Weight;
    public string Name;
}

/// <summary>
/// The dungeon generation script
/// Warning: Big
/// </summary>
public class DunGen : MonoBehaviour
{
    [SerializeField] private Transform _dungeonRoot;
    public int Rows = 15;
    public int Cols = 15;
    [SerializeField] private int minRoomSize = 3;
    [SerializeField] private int maxRoomSize = 5;
    [SerializeField] private FloorTile[] _floorTiles;
    [SerializeField] private WallTile[] _wallTiles;
    [SerializeField] private GameObject _doorPrefab;

    private GameTile[,] grid;

    public GameTile[,] Grid => grid;

    public Vector3 PlayerSpawnPosition { get; private set; }
    public GameTile PlayerStartTile { get; private set; }
    public Vector3 StairsPosition { get; private set; }
    public GameTile StairsTile { get; private set; }
    public bool DungeonGenerated { get; private set; }

    //special room placements
    private List<BSPNode> _rooms = new List<BSPNode>(); //store all rooms
    private List<BSPNode> _eligibleInteractableRooms = new List<BSPNode>(); //rooms eligible for containing interactables (excludes player and stairs rooms)
    public List<GameTile> InteractableTiles { get; private set; } = new List<GameTile>();
    public BSPNode PlayerRoom { get; private set; }
    public BSPNode StairsRoom { get; private set; }


    [ContextMenu("Generate Dungeon")]
    public void GenerateDungeon()
    {
        InitializeGrid();
        SplitAndCreateRooms();
        BlockWalls();
        AddDoors();
        InstantiateFloorTiles();
        InstantiateWalls();
        DungeonGenerated = true;
    }

    private void InitializeGrid()
    {
        grid = new GameTile[Rows, Cols];
        for (int x = 0; x < Rows; x++)
        {
            for (int y = 0; y < Cols; y++)
            {
                //set world space position
                Vector3 pos = new Vector3(_dungeonRoot.position.x + (x * 4), 0, _dungeonRoot.position.z + (y * 4));
                //create the tile and assign the coordinates
                grid[x, y] = new GameTile
                {
                    Position = pos,
                    x = x,
                    y = y
                };
            }
        }
    }

    /// <summary>
    /// To be called on reset
    /// </summary>
    public void ClearDungeon()
    {
        foreach (Transform child in _dungeonRoot)
        {
            Destroy(child.gameObject);
        }

        grid = null;
        DungeonGenerated = false;
    }

    #region Dungeon Generation

    /// <summary>
    /// divide the tiles and stamp out the rooms
    /// </summary>
    private void SplitAndCreateRooms()
    {
        BSPNode rootNode = new BSPNode(new RectInt(0, 0, Rows, Cols));
        Queue<BSPNode> nodes = new Queue<BSPNode>();
        nodes.Enqueue(rootNode);

        while (nodes.Count > 0)
        {
            BSPNode node = nodes.Dequeue();
            if (node.Area.width > maxRoomSize * 2 || node.Area.height > maxRoomSize * 2)
            {
                if (SplitNode(node))
                {
                    nodes.Enqueue(node.Left);
                    nodes.Enqueue(node.Right);
                }
            }
        }

        List<BSPNode> leafNodes = DungeonUtils.GetLeafNodes(rootNode);

        foreach (var leaf in leafNodes)
        {
            PlaceRoom(leaf);
        }

        ConnectRooms(leafNodes);
        _rooms = leafNodes;
        ChoosePlayerAndStairs(leafNodes);
        MarkEligibleInteractableRooms(_rooms);
    }


    /// <summary>
    /// Splits a BSP node
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private bool SplitNode(BSPNode node)
    {
        // Determine if we can split horizontally or vertically
        bool splitHorizontally = Random.value > 0.5f;

        // Only allow splitting if both child nodes will be large enough for a room
        if (splitHorizontally)
        {
            if (node.Area.height < minRoomSize * 2)
                return false; // Can't split safely

            int maxSplitY = node.Area.height - minRoomSize;
            if (maxSplitY <= minRoomSize)
                return false;

            int splitY = Random.Range(minRoomSize, maxSplitY);
            node.Left = new BSPNode(new RectInt(node.Area.x, node.Area.y, node.Area.width, splitY));
            node.Right = new BSPNode(new RectInt(node.Area.x, node.Area.y + splitY, node.Area.width, node.Area.height - splitY));
        }
        else
        {
            if (node.Area.width < minRoomSize * 2)
                return false; // Can't split safely

            int maxSplitX = node.Area.width - minRoomSize;
            if (maxSplitX <= minRoomSize)
                return false;

            int splitX = Random.Range(minRoomSize, maxSplitX);
            node.Left = new BSPNode(new RectInt(node.Area.x, node.Area.y, splitX, node.Area.height));
            node.Right = new BSPNode(new RectInt(node.Area.x + splitX, node.Area.y, node.Area.width - splitX, node.Area.height));
        }

        return true;
    }

    /// <summary>
    /// places a room with the given node params
    /// </summary>
    /// <param name="node"></param>
    private void PlaceRoom(BSPNode node)
    {
        int roomWidth = Random.Range(minRoomSize, Mathf.Min(maxRoomSize + 1, node.Area.width));
        int roomHeight = Random.Range(minRoomSize, Mathf.Min(maxRoomSize + 1, node.Area.height));

        int roomX = node.Area.x + Random.Range(0, Mathf.Max(1, node.Area.width - roomWidth + 1));
        int roomY = node.Area.y + Random.Range(0, Mathf.Max(1, node.Area.height - roomHeight + 1));

        // Clamp only if needed
        if (roomX + roomWidth > Rows)
        {
            roomWidth = Rows - roomX;
        }
        if (roomY + roomHeight > Cols)
        {
            roomHeight = Cols - roomY;
        }

        node.Room = new RectInt(roomX, roomY, roomWidth, roomHeight);

        for (int x = roomX; x < roomX + roomWidth; x++)
        {
            for (int y = roomY; y < roomY + roomHeight; y++)
            {
                grid[x, y].IsFloor = true;
                //set the tile to "Room"
                grid[x, y].Type = TileType.Room;

                if (x > roomX)
                {
                    grid[x, y].WestWall = false;
                    grid[x - 1, y].EastWall = false;
                }
                if (y > roomY)
                {
                    grid[x, y].SouthWall = false;
                    grid[x, y - 1].NorthWall = false;
                }
            }
        }
    }

    private void ConnectRooms(List<BSPNode> rooms)
    {
        for (int i = 1; i < rooms.Count; i++)
        {
            Vector2Int prevCenter = GetRoomCenter(rooms[i - 1].Room);
            Vector2Int currCenter = GetRoomCenter(rooms[i].Room);
            CarveCorridor(prevCenter, currCenter);
        }
    }

    private Vector2Int GetSafeRoomCenter(RectInt room)
    {
        List<Vector2Int> safeTiles = new List<Vector2Int>();

        for (int x = room.x; x < room.x + room.width; x++)
        {
            for (int y = room.y; y < room.y + room.height; y++)
            {
                if (DungeonUtils.IsSafeTile(Rows, Cols, x, y, Grid))
                {
                    safeTiles.Add(new Vector2Int(x, y));
                }
            }
        }

        if (safeTiles.Count > 0)
        {
            return safeTiles[Random.Range(0, safeTiles.Count)];
        }

        // fallback to original method if no safe tile found
        return GetRoomCenter(room);
    }

    private Vector2Int GetRoomCenter(RectInt room)
    {
        for (int attempt = 0; attempt < 10; attempt++) // Try multiple times
        {
            int centerX = room.x + Random.Range(0, room.width);
            int centerY = room.y + Random.Range(0, room.height);

            int safeX = Mathf.Clamp(centerX, 0, Rows - 1);
            int safeY = Mathf.Clamp(centerY, 0, Cols - 1);

            if (grid[safeX, safeY].IsFloor)
            {
                return new Vector2Int(safeX, safeY);
            }
        }

        // Fallback: pick the mathematical center
        int fallbackX = room.x + room.width / 2;
        int fallbackY = room.y + room.height / 2;
        fallbackX = Mathf.Clamp(fallbackX, 0, Rows - 1);
        fallbackY = Mathf.Clamp(fallbackY, 0, Cols - 1);
        return new Vector2Int(fallbackX, fallbackY);
    }

        private void CarveCorridor(Vector2Int start, Vector2Int end)
    {
        int x = start.x;
        int y = start.y;

        while (x != end.x)
        {
            int step = (end.x > x) ? 1 : -1;
            x += step;
            grid[x, y].IsFloor = true;
            // don't overwrite the tileType of room tiles
            if (grid[x, y].Type != TileType.Room)
            {
                grid[x, y].Type = TileType.Corridor;
            }
            if (step > 0)
            {
                grid[x, y].WestWall = false;
                grid[x - 1, y].EastWall = false;
            }
            else
            {
                grid[x, y].EastWall = false;
                grid[x + 1, y].WestWall = false;
            }
        }

        while (y != end.y)
        {
            int step = (end.y > y) ? 1 : -1;
            y += step;
            grid[x, y].IsFloor = true;
            if (step > 0)
            {
                grid[x, y].SouthWall = false;
                grid[x, y - 1].NorthWall = false;
            }
            else
            {
                grid[x, y].NorthWall = false;
                grid[x, y + 1].SouthWall = false;
            }
        }
    }

    #endregion

    #region Item and NPC placement
    private void ChoosePlayerAndStairs(List<BSPNode> leafNodes)
    {
        if (leafNodes.Count < 2)
        {
            Debug.LogWarning("Not enough rooms to place player and stairs.");
            return;
        }

        // Randomly pick player room
        BSPNode playerRoom = leafNodes[Random.Range(0, leafNodes.Count)];
        Vector2Int playerCoords = GetRoomCenter(playerRoom.Room);
        var spawnTilePosition = grid[playerCoords.x, playerCoords.y].Position;
        PlayerSpawnPosition = spawnTilePosition;
        PlayerStartTile = grid[playerCoords.x, playerCoords.y];

        // Pick a different random room for stairs
        BSPNode stairsRoom;
        do
        {
            stairsRoom = leafNodes[Random.Range(0, leafNodes.Count)];
        }
        while (stairsRoom == playerRoom);

        Vector2Int stairsCoords = GetRoomCenter(stairsRoom.Room);
        StairsPosition = grid[stairsCoords.x, stairsCoords.y].Position;
        StairsTile = grid[stairsCoords.x, stairsCoords.y];

        PlayerRoom = playerRoom;
        StairsRoom = stairsRoom;

        if (!PlayerStartTile.IsFloor)
        {
            Debug.LogWarning("PlayerStartTile is not on a floor! Trying to find fallback.");
            for (int x = 0; x < Rows; x++)
            {
                for (int y = 0; y < Cols; y++)
                {
                    if (grid[x, y].IsFloor)
                    {
                        PlayerStartTile = grid[x, y];
                        PlayerSpawnPosition = new Vector3(grid[x, y].Position.x, 1.5f, grid[x, y].Position.z);
                        break;
                    }
                }
            }
        }
    }

    private void MarkEligibleInteractableRooms(List<BSPNode> rooms)
    {
        //make sure this is clear (for resets etc)
        _eligibleInteractableRooms.Clear();
        //check the entire rooms collection
        foreach (var room in rooms)
        {
            //exclude player and stairs rooms
            if (room == null || room == PlayerRoom || room == StairsRoom)
            {
                continue;
            }
            //add any potential interactable rooms to the list
            _eligibleInteractableRooms.Add(room);
        }
    }

    public void MarkInteractables()
    {
        InteractableTiles.Clear();

        if (_eligibleInteractableRooms.Count < 2)
        {
            Debug.LogWarning("not enough rooms for all essential interactables. Make dungeon bigger");
            return;
        }

        // Guarantee at least one merchant/NPC room per floor
        BSPNode merchantRoom = PickAndRemoveRandomRoom(_eligibleInteractableRooms);
        Vector2Int merchantCoords = GetSafeRoomCenter(merchantRoom.Room);
        InteractableTiles.Add(grid[merchantCoords.x, merchantCoords.y]);

        // Guarantee at least one treasure room per floor
        BSPNode chestRoom = PickAndRemoveRandomRoom(_eligibleInteractableRooms);
        Vector2Int chestcoords = GetSafeRoomCenter(chestRoom.Room);
        InteractableTiles.Add(grid[chestcoords.x, chestcoords.y]);

        // Add random interactables to remaining rooms

        float fillChance = 0.3f;
        foreach (var room in _eligibleInteractableRooms)
        {
            if (Random.value < fillChance)
            {
                Vector2Int coords = GetSafeRoomCenter(room.Room);
                InteractableTiles.Add(grid[coords.x, coords.y]);
            }
        }
    }

    
    public void AddDoors()
    {
        Debug.Log("Adding doors");
        for (int x = 0; x < Rows; x++)
        {
            for (int y = 0; y < Cols; y++)
            {
                GameTile tile = grid[x, y];

                if (!tile.IsFloor) continue;

                // Check each direction
                TryPlaceDoor(x, y, Direction.North);
                TryPlaceDoor(x, y, Direction.South);
                TryPlaceDoor(x, y, Direction.East);
                TryPlaceDoor(x, y, Direction.West);
            }
        }
    }
    private void TryPlaceDoor(int x, int y, Direction dir)
    {

        Vector2Int offset = DungeonUtils.GetDirectionOffset(dir);
        int nx = x + offset.x;
        int ny = y + offset.y;

        if (nx < 0 || ny < 0 || nx >= Rows || ny >= Cols)
            return;

        GameTile currentTile = grid[x, y];
        GameTile neighborTile = grid[nx, ny];

        if (!currentTile.IsFloor || !neighborTile.IsFloor)
            return;

        bool oneRoom = DungeonUtils.IsRoomTile(currentTile) || DungeonUtils.IsRoomTile(neighborTile);
        bool oneCorridor = DungeonUtils.IsCorridorTile(currentTile) || DungeonUtils.IsCorridorTile(neighborTile);

        if (oneRoom && oneCorridor)
        {
            // Check if there's still a wall between the two tiles
            if (DungeonUtils.IsWallBetween(currentTile, neighborTile, dir))
                return;

            // Avoid duplicates: only place if no door exists on either side
            Direction opposite = DungeonUtils.GetOppositeDirection(dir);
            if ((currentTile.OccupiedByInteractable is Door d1 && d1.WallDirection == dir) ||
                (neighborTile.OccupiedByInteractable is Door d2 && d2.WallDirection == opposite))
                return;

            if (DungeonUtils.IsCorridorTile(currentTile))
            {
                PlaceDoor(currentTile, dir);
            }
            else
            {
                PlaceDoor(neighborTile, opposite);
            }
        }
    }

    private void PlaceDoor(GameTile tile, Direction wallDir)
    {
        if (tile.OccupiedByInteractable is Door)
            return;

        // Match the wall placement offset
        Vector3 positionOffset = Vector3.zero;
        Quaternion rotation = Quaternion.identity;

        switch (wallDir)
        {
            case Direction.North:
                positionOffset = new Vector3(0, 0, 2);
                rotation = Quaternion.Euler(0, 180, 0);
                break;
            case Direction.South:
                positionOffset = new Vector3(0, 0, -2);
                rotation = Quaternion.Euler(0, 0, 0);
                break;
            case Direction.East:
                positionOffset = new Vector3(2, 0, 0);
                rotation = Quaternion.Euler(0, -90, 0);
                break;
            case Direction.West:
                positionOffset = new Vector3(-2, 0, 0);
                rotation = Quaternion.Euler(0, 90, 0);
                break;
        }

        GameObject doorGO = GameObject.Instantiate(_doorPrefab, tile.Position + positionOffset, rotation, _dungeonRoot);
        Door door = doorGO.GetComponent<Door>();
        if (door == null)
        {
            Debug.LogError("Door prefab missing Door component!");
            Destroy(doorGO);
            return;
        }

        door.TileData = tile;
        door.WallDirection = wallDir;

        tile.SetBlocked(wallDir, true); // block movement initially
        tile.OccupiedByInteractable = door;
    }

    /// <summary>
    /// removes one random room from the rooms list
    /// </summary>
    /// <param name="rooms"></param>
    /// <returns></returns>
    private BSPNode PickAndRemoveRandomRoom(List<BSPNode> rooms)
    {
        if (rooms.Count == 0) return null;
        int index = Random.Range(0, rooms.Count);
        BSPNode selected = rooms[index];
        rooms.RemoveAt(index);
        return selected;
    }
    #endregion

    #region Instantiation 

    private void InstantiateFloorTiles()
    {
        GameObject floorRoot = new GameObject("FloorTiles");
        floorRoot.transform.parent = _dungeonRoot;

        for (int x = 0; x < Rows; x++)
        {
            for (int y = 0; y < Cols; y++)
            {
                if (grid[x, y].IsFloor)
                {
                    Instantiate(DungeonUtils.GetRandomFloorTile(_floorTiles), grid[x, y].Position, Quaternion.identity, floorRoot.transform);
                }
            }
        }
    }

    private void InstantiateWalls()
    {
        GameObject wallRoot = new GameObject("Walls");
        wallRoot.transform.parent = _dungeonRoot;

        for (int x = 0; x < Rows; x++)
        {
            for (int y = 0; y < Cols; y++)
            {
                GameTile tile = grid[x, y];
                GameObject prefab = DungeonUtils.GetRandomWallTile(_wallTiles);

                // Only instantiate a wall if no door is blocking that direction
                if (tile.NorthWall && !(tile.OccupiedByInteractable is Door d1 && d1.WallDirection == Direction.North))
                {
                    Vector3 wallPos = tile.Position + new Vector3(0, 0, 2);
                    Instantiate(prefab, wallPos, Quaternion.Euler(0, 180, 0), wallRoot.transform);
                }
                if (tile.SouthWall && !(tile.OccupiedByInteractable is Door d2 && d2.WallDirection == Direction.South))
                {
                    Vector3 wallPos = tile.Position + new Vector3(0, 0, -2);
                    Instantiate(prefab, wallPos, Quaternion.identity, wallRoot.transform);
                }
                if (tile.EastWall && !(tile.OccupiedByInteractable is Door d3 && d3.WallDirection == Direction.East))
                {
                    Vector3 wallPos = tile.Position + new Vector3(2, 0, 0);
                    Instantiate(prefab, wallPos, Quaternion.Euler(0, -90, 0), wallRoot.transform);
                }
                if (tile.WestWall && !(tile.OccupiedByInteractable is Door d4 && d4.WallDirection == Direction.West))
                {
                    Vector3 wallPos = tile.Position + new Vector3(-2, 0, 0);
                    Instantiate(prefab, wallPos, Quaternion.Euler(0, 90, 0), wallRoot.transform);
                }
            }
        }
    }

    /// <summary>
    /// sets the walls to be blocking objects
    /// </summary>
    private void BlockWalls()
    {
        for (int x = 0; x < Rows; x++)
        {
            for (int y = 0; y < Cols; y++)
            {
                GameTile tile = grid[x, y];

                if (tile.NorthWall)
                    tile.SetBlocked(Direction.North, true);
                if (tile.SouthWall)
                    tile.SetBlocked(Direction.South, true);
                if (tile.EastWall)
                    tile.SetBlocked(Direction.East, true);
                if (tile.WestWall)
                    tile.SetBlocked(Direction.West, true);
            }
        }
    }
    


    #endregion
}
