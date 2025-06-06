using System.Collections;
using UnityEditor.ShaderGraph;
using UnityEngine;
[System.Serializable]
public enum EnemyStates
{
    Idle,
    Persuing,
    Attacking,
}

public class Enemy : CharacterSheet
{
    [SerializeField] protected string[] names;

    public EnemyStates State { get; set; } = EnemyStates.Idle;

    public bool IsDead { get; private set; }

    [SerializeField] protected Animator animator;

    protected override void Awake()
    {
        base.Awake();

        characterName = GetName();
    }

    // Randomly select a name from the collection if any have been provided
    private string GetName()
    {
        if (names.Length == 0)
        {
            return "defaultName";
        }

        var index = Random.Range(0, names.Length);

        return names[index];
    }

    protected override void Die()
    {
        base.Die();
        IsDead = true;
    }

    public IEnumerator PersuePlayer()
    {
        // Decide next step
        Direction bestDir = DeterminePlayerDirection();

        GameTile targetTile = MoveUtils.GetTargetTile(CurrentTile, bestDir, GameManager.Instance.FinalGrid);

        if (targetTile != null && !targetTile.IsOccupied && MoveUtils.CanMoveToTile(CurrentTile, bestDir, GameManager.Instance.FinalGrid))
        {
            if (FacingDirection != bestDir)
            {
                yield return StartCoroutine(TurnToTargetDirection(bestDir));
            }

            // We're facing the correct direction!
            if (targetTile != null && !targetTile.IsOccupied && MoveUtils.CanMoveToTile(CurrentTile, bestDir, GameManager.Instance.FinalGrid))
            {
                AttemptMove(targetTile);
            }
        }

        //at the end of this persue phase, check if we reached the target
        Direction requiredDirectionIfReachedTarget;

        if (MoveUtils.TargetTileReached(CurrentTile, GameManager.Instance.Player.CurrentTile, GameManager.Instance.FinalGrid, out requiredDirectionIfReachedTarget))
        {
            yield return StartCoroutine(TurnToTargetDirection(requiredDirectionIfReachedTarget));
        }

        yield return null;
    }

    /// <summary>
    /// determines the direction needed to face the player
    /// </summary>
    /// <returns></returns>
    private Direction DeterminePlayerDirection()
    {
        var playerTile = GameManager.Instance.Player.CurrentTile;
        Vector2Int enemyPos = new Vector2Int(CurrentTile.x, CurrentTile.y);
        Vector2Int playerPos = new Vector2Int(playerTile.x, playerTile.y);

        // Decide next step
        Vector2Int delta = playerPos - enemyPos;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            return delta.x > 0 ? Direction.East : Direction.West;
        else
            return delta.y > 0 ? Direction.North : Direction.South;
    }

    public IEnumerator TurnToTargetDirection(Direction targetDirection)
    {
        // decide whether to turn clockwise or anticlockwise
        // so we can reach bestDir in the fewest steps.
        // Directions in enum order: North, East, South, West
        int currentIndex = (int)FacingDirection;
        int targetIndex = (int)targetDirection;

        // Calculate difference clockwise and counterclockwise
        int clockwiseSteps = (targetIndex - currentIndex + 4) % 4;
        int anticlockwiseSteps = (currentIndex - targetIndex + 4) % 4;

        // Choose the shortest rotation direction
        bool clockwise = clockwiseSteps <= anticlockwiseSteps;

        // turn until we reach the required direction
        while (FacingDirection != targetDirection)
        {
            Turn(clockwise);

            yield return null;
        }
    }

    public IEnumerator AttackPlayer()
    {
        //double check we're facing the right direction when we're in a square adjacent to the player
        Direction bestDir = DeterminePlayerDirection();

        if (FacingDirection != bestDir)
        {
            yield return StartCoroutine(TurnToTargetDirection(bestDir));
        }
        
        //since we already verified that we're next to the player, there's no need to get the tile data
        var target = GameManager.Instance.Player;

        CombatUtils.Attack(this, target);
        
        yield return null;
    }
    
}
