using System.Collections;
using UnityEngine;

/// <summary>
/// represents a character (player or enemy) - uses the basic HeroQuest structure minus the movement stat
/// </summary>
public abstract class CharacterSheet : MonoBehaviour
{
    //The currently facing direction
    public Direction FacingDirection { get; set; }

    //The current tile this character is standing on
    public GameTile CurrentTile { get; set; }

    [SerializeField] protected string characterName;

    public string CharacterName => characterName;

    //Basic stats (yeah it's HeroQuest)
    [SerializeField] protected int BodyPoints = 8;
    [SerializeField] public int DefendDice = 2;
    [SerializeField] public int AttackDice = 3;

    //current stats - defend and attack dice can be influenced by equipped items
    public int CurrentBodyPoints { get; protected set; }
    public int CurrentDefendDice { get; protected set; }
    public int CurrentAttackDice { get; protected set; }

    private Coroutine _rotationCoroutine;

    protected virtual void Awake()
    {
        CurrentBodyPoints = BodyPoints;
        CurrentDefendDice = DefendDice;
        CurrentAttackDice = AttackDice;
    }

    public virtual void OnTurnStart() { }
    public virtual void OnTurnEnd() { }

    public virtual void TakeHits(int numHits)
    {
        CurrentBodyPoints -= numHits;
        if (CurrentBodyPoints <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Debug.Log(characterName + " has perished");
        CurrentTile.IsOccupied = false;
        CurrentTile.OccupiedByCharacter = null;
    }

    public bool AttemptMove(GameTile targetTile)
    {
        if (targetTile == null)
        {
            return false;
        }

        if (MoveUtils.CanMoveToTile(CurrentTile, FacingDirection, GameManager.Instance.FinalGrid))
        {
            var previousPosition = CurrentTile.Position;
            CurrentTile.IsOccupied = false;
            CurrentTile = targetTile;
            // set the new tile as occupied and update character info
            CurrentTile.IsOccupied = true;
            CurrentTile.OccupiedByCharacter = this;
            StartCoroutine(Slide(previousPosition, CurrentTile.Position, 0.2f));
            return true;
        }

        if (targetTile.IsOccupied)
        {
            if (targetTile.OccupiedByCharacter)
            {
                Debug.Log(characterName + " is blocked by " + targetTile.OccupiedByCharacter.characterName);
            }
            else if (targetTile.OccupiedByInteractable != null)
            {
                Debug.Log(characterName + " is blocked by " + targetTile.OccupiedByInteractable.name);
            }
        }
        else
        {
            Debug.Log(characterName + " is blocked by wall or obstacle");
        }

        return false;
    }

    private IEnumerator Slide(Vector3 previousPosition, Vector3 newPosition, float duration)
    {
        float currentTime = 0;

        while (currentTime <= duration)
        {
            currentTime += Time.deltaTime;

            float percent = currentTime / duration;

            transform.position = Vector3.Lerp(previousPosition, newPosition, percent);

            yield return null;
        }
    }

    public void Turn(bool clockwise)
    {
        switch (FacingDirection)
        {
            case Direction.North:
                FacingDirection = clockwise ? Direction.East : Direction.West;
                break;
            case Direction.East:
                FacingDirection = clockwise ? Direction.South : Direction.North;
                break;
            case Direction.South:
                FacingDirection = clockwise ? Direction.West : Direction.East;
                break;
            case Direction.West:
                FacingDirection = clockwise ? Direction.North : Direction.South;
                break;
        }

        UpdateRotation();
    }
    
    private void UpdateRotation()
    {
        float yRotation = GetYRotationForDirection(FacingDirection);

        if (_rotationCoroutine != null)
        {
            StopCoroutine(_rotationCoroutine);
        }

        _rotationCoroutine = StartCoroutine(SmoothRotate(yRotation, 0.2f));
    }

    private float GetYRotationForDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.North: return 0f;
            case Direction.East: return 90f;
            case Direction.South: return 180f;
            case Direction.West: return 270f;
            default: return 0f;
        }
    }

    private IEnumerator SmoothRotate(float targetYRotation, float duration)
    {
        Quaternion initialRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0f, targetYRotation, 0f);

        float currentTime = 0f;
        while (currentTime < duration)
        {
            transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, currentTime / duration);
            currentTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;
    }
}
