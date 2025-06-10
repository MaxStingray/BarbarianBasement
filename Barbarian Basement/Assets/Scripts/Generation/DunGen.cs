using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

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

public class DunGen : MonoBehaviour
{
    [SerializeField] private Transform _dungeonRoot;
    public int Rows = 15;
    public int Cols = 15;
    [SerializeField] private int minRoomSize = 3;
    [SerializeField] private int maxRoomSize = 5;
    [SerializeField] private FloorTile[] _floorTiles;
    [SerializeField] private WallTile[] _wallTiles;

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
    public BSPNode StairsRoom{ get; private set; }


    [ContextMenu("Generate Dungeon")]
    public void GenerateDungeon()
    {
        InitializeGrid();
        SplitAndCreateRooms();
        InstantiateFloorTiles();
        InstantiateWalls();
        // Optional: Remove ALL irrelevant walls from corridor tiles
        for (int x = 0; x < Rows; x++)
        {
            for (int y = 0; y < Cols; y++)
            {
                if (grid[x, y].IsFloor)
                {
                    // Only keep relevant walls for room edges; corridors are open
                    // Optionally, reset all walls for corridor tiles
                    grid[x, y].NorthWall = false;
                    grid[x, y].SouthWall = false;
                    grid[x, y].EastWall = false;
                    grid[x, y].WestWall = false;
                }
            }
        }
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

    #region BSP Splitting

    public class BSPNode
    {
        public RectInt Area;
        public BSPNode Left;
        public BSPNode Right;
        public RectInt Room;

        public bool IsLeaf => Left == null && Right == null;

        public BSPNode(RectInt area)
        {
            Area = area;
        }
    }

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

        List<BSPNode> leafNodes = GetLeafNodes(rootNode);

        foreach (var leaf in leafNodes)
        {
            PlaceRoom(leaf);
        }

        ConnectRooms(leafNodes);
        _rooms = leafNodes;
        ChoosePlayerAndStairs(leafNodes);
        MarkEligibleInteractableRooms(_rooms);
    }

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
                        PlayerSpawnPosition = new Vector3(grid[x, y].Position.x, 1.7f, grid[x, y].Position.z);
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
        Vector2Int merchantCoords = GetRoomCenter(merchantRoom.Room);
        InteractableTiles.Add(grid[merchantCoords.x, merchantCoords.y]);

        // Guarantee at least one treasure room per floor
        BSPNode chestRoom = PickAndRemoveRandomRoom(_eligibleInteractableRooms);
        Vector2Int chestcoords = GetRoomCenter(chestRoom.Room);
        InteractableTiles.Add(grid[chestcoords.x, chestcoords.y]);

        // Add random interactables to remaining rooms

        float fillChance = 0.3f;
        foreach (var room in _eligibleInteractableRooms)
        {
            if (Random.value < fillChance)
            {
                Vector2Int coords = GetRoomCenter(room.Room);
                InteractableTiles.Add(grid[coords.x, coords.y]);
            }
        }
    }

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

    private List<BSPNode> GetLeafNodes(BSPNode root)
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

    private void ConnectRooms(List<BSPNode> rooms)
    {
        for (int i = 1; i < rooms.Count; i++)
        {
            Vector2Int prevCenter = GetRoomCenter(rooms[i - 1].Room);
            Vector2Int currCenter = GetRoomCenter(rooms[i].Room);
            CarveCorridor(prevCenter, currCenter);
        }
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

    #endregion

    private void CarveCorridor(Vector2Int start, Vector2Int end)
    {
        int x = start.x;
        int y = start.y;

        while (x != end.x)
        {
            int step = (end.x > x) ? 1 : -1;
            x += step;
            grid[x, y].IsFloor = true;
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
                    Instantiate(GetRandomFloorTile(), grid[x, y].Position, Quaternion.identity, floorRoot.transform);
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
                GameObject prefab = GetRandomWallTile();

                if (tile.NorthWall)
                {
                    Vector3 wallPos = tile.Position + new Vector3(0, 0, 2);
                    Instantiate(prefab, wallPos, Quaternion.Euler(0, 180, 0), wallRoot.transform);
                }
                if (tile.SouthWall)
                {
                    Vector3 wallPos = tile.Position + new Vector3(0, 0, -2);
                    Instantiate(prefab, wallPos, Quaternion.identity, wallRoot.transform);
                }
                if (tile.EastWall)
                {
                    Vector3 wallPos = tile.Position + new Vector3(2, 0, 0);
                    Instantiate(prefab, wallPos, Quaternion.Euler(0, -90, 0), wallRoot.transform);
                }
                if (tile.WestWall)
                {
                    Vector3 wallPos = tile.Position + new Vector3(-2, 0, 0);
                    Instantiate(prefab, wallPos, Quaternion.Euler(0, 90, 0), wallRoot.transform);
                }
            }
        }
    }

    private GameObject GetRandomFloorTile()
    {
        int totalWeight = 0;
        foreach (var tile in _floorTiles) totalWeight += tile.Weight;
        int rand = Random.Range(0, totalWeight);
        int runningWeight = 0;
        foreach (var tile in _floorTiles)
        {
            runningWeight += tile.Weight;
            if (rand < runningWeight) return tile.Prefab;
        }
        return _floorTiles[0].Prefab;
    }

    private GameObject GetRandomWallTile()
    {
        int totalWeight = 0;
        foreach (var tile in _wallTiles) totalWeight += tile.Weight;
        int rand = Random.Range(0, totalWeight);
        int runningWeight = 0;
        foreach (var tile in _wallTiles)
        {
            runningWeight += tile.Weight;
            if (rand < runningWeight) return tile.Prefab;
        }
        return _wallTiles[0].Prefab;
    }

    #endregion
}
