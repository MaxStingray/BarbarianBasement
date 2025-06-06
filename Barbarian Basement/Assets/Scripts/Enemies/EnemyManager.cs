using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyPrefab
{
    public GameObject Prefab;
    public int Weight;
    public string Name;
}

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private int numberOfEnemies = 5;
    [SerializeField] private EnemyPrefab[] enemyPrefabs;

    private List<CharacterSheet> spawnedEnemies = new List<CharacterSheet>();

    public void SpawnEnemies(GameTile[,] grid)
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogError("No enemy prefabs assigned!");
            return;
        }

        for (int i = 0; i < numberOfEnemies; i++)
        {
            GameObject enemyPrefab = GetRandomEnemyPrefab();
            GameTile spawnTile = GetRandomUnoccupiedTile(grid);

            if (spawnTile == null)
            {
                Debug.LogWarning("No unoccupied tile available for enemy spawn.");
                continue;
            }

            GameObject enemyGO = Instantiate(enemyPrefab, spawnTile.Position, Quaternion.identity);
            CharacterSheet enemy = enemyGO.GetComponent<CharacterSheet>();
            if (enemy == null)
            {
                Debug.LogError($"Enemy prefab {enemyPrefab.name} missing CharacterSheet component!");
                continue;
            }

            // Mark the tile as occupied
            spawnTile.IsOccupied = true;
            spawnTile.OccupiedBy = enemy;
            enemy.CurrentTile = spawnTile;

            spawnedEnemies.Add(enemy);
        }
    }

    private GameObject GetRandomEnemyPrefab()
    {
        int totalWeight = 0;
        foreach (var enemy in enemyPrefabs)
        {
            totalWeight += enemy.Weight;
        }

        int rand = Random.Range(0, totalWeight);
        int cumulative = 0;
        foreach (var enemy in enemyPrefabs)
        {
            cumulative += enemy.Weight;
            if (rand < cumulative)
            {
                return enemy.Prefab;
            }
        }

        // Fallback (shouldn't happen)
        return enemyPrefabs[0].Prefab;
    }

    /// <summary>
    /// Spawn randomly for now, but we can add rules later
    /// </summary>
    /// <param name="grid"></param>
    /// <returns></returns>
    private GameTile GetRandomUnoccupiedTile(GameTile[,] grid)
    {
        List<GameTile> unoccupiedTiles = new List<GameTile>();

        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                if (grid[x, y].IsFloor && !grid[x, y].IsOccupied)
                {
                    unoccupiedTiles.Add(grid[x, y]);
                }
            }
        }

        if (unoccupiedTiles.Count == 0) return null;

        int index = Random.Range(0, unoccupiedTiles.Count);
        return unoccupiedTiles[index];
    }
}
