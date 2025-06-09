using System.Collections.Generic;
using System.Collections;
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

    private List<Enemy> spawnedEnemies = new List<Enemy>();

    private Coroutine _enemyStateCheck;

    private bool _enemyTurnEnding;

    void Awake()
    {
        if (TurnManager.Instance == null)
        {
            StartCoroutine(ValidateTurnManager());
        }
        else
        {
            TurnManager.Instance.OnEnemyTurnStart.AddListener(HandleTurnStart);
        }
    }

    IEnumerator ValidateTurnManager()
    {
        while (TurnManager.Instance == null)
        {
            yield return null;
        }

        TurnManager.Instance.OnEnemyTurnStart.AddListener(HandleTurnStart);
        TurnManager.Instance.OnEnemyTurnEnd.AddListener(HandleTurnEnd);
    }

    private void HandleTurnStart()
    {
        if (_enemyStateCheck != null)
        {
            StopCoroutine(_enemyStateCheck);
            _enemyStateCheck = null;
        }
        _enemyStateCheck = StartCoroutine(TurnStartSequence());
    }

    private IEnumerator TurnStartSequence()
    {
        _enemyTurnEnding = false;

        foreach (var enemy in spawnedEnemies)
        {
            if (enemy.IsDead)
            {
                continue;
            }

            var lastEnemyState = enemy.State;

            //first, check if we're next to a target
            if (IsNextToTarget(enemy, out Direction adjacentDirection))
            {
                yield return HandleEnemyAttack(enemy);
                continue;
            }
            //next, check if we have line of sight and pursue if so
            if (HasLineOfSightToPlayer(enemy))
            {
                //make sure the pursuit counter is reset if we regain line of sight
                enemy.StopPursuit();
                yield return HandleEnemyPursuit(enemy);
                continue;
            }
            //otherwise, continue persuing if we previously had line of sight
            if (lastEnemyState == EnemyStates.Persuing)
            {
                //reset the enemy if the pursuit is finished
                if (enemy.PursueFinished())
                {
                    //reset pursuit counter
                    enemy.StopPursuit();
                    //return to idle
                    HandleEnemyIdle(enemy);
                    continue;
                }

                yield return HandleEnemyContinuePursuit(enemy);
                continue;
            }

            HandleEnemyIdle(enemy);
        }

        if (!_enemyTurnEnding)
        {
            _enemyTurnEnding = true;
            TurnManager.Instance.EndTurn();
        }
    }

    #region AI helpers
    private bool IsNextToTarget(Enemy enemy, out Direction adjacentDirection)
    {
        return MoveUtils.TargetTileReached(
            enemy.CurrentTile,
            GameManager.Instance.Player.CurrentTile,
            GameManager.Instance.FinalGrid,
            out adjacentDirection);
    }

    private bool HasLineOfSightToPlayer(Enemy enemy)
    {
        return CombatUtils.HasLineOfSight(
            enemy.CurrentTile,
            GameManager.Instance.Player.CurrentTile,
            GameManager.Instance.FinalGrid);
    }

    private IEnumerator HandleEnemyAttack(Enemy enemy)
    {
        Debug.Log($"{enemy.CharacterName} reached target");
        enemy.State = EnemyStates.Attacking;
        yield return StartCoroutine(enemy.AttackPlayer());
    }

    private IEnumerator HandleEnemyPursuit(Enemy enemy)
    {
        enemy.State = EnemyStates.Persuing;
        Debug.Log($"{enemy.CharacterName} is pursuing {GameManager.Instance.Player.CharacterName}");
        yield return StartCoroutine(enemy.PursuePlayer());
        yield return null;
    }

    private IEnumerator HandleEnemyContinuePursuit(Enemy enemy)
    {
        enemy.State = EnemyStates.Persuing;
        Debug.Log($"{enemy.CharacterName} continues pursuing {GameManager.Instance.Player.CharacterName}");
        yield return StartCoroutine(enemy.PursuePlayer());
        yield return null;
    }

    private void HandleEnemyIdle(Enemy enemy)
    {
        enemy.State = EnemyStates.Idle;
    }
    #endregion

    private void HandleTurnEnd()
    {
        if (_enemyStateCheck != null)
        {
            StopCoroutine(_enemyStateCheck);
            _enemyStateCheck = null;
        }
    }

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
            Enemy enemy = enemyGO.GetComponent<Enemy>();
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
