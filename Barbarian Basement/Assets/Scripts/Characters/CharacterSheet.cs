using System.Collections;
using Mono.Cecil.Cil;
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

    public int CurrentBodyPoints { get; private set; }

    protected virtual void Awake()
    {
        CurrentBodyPoints = BodyPoints;
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
        CurrentTile.OccupiedBy = null;
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
            CurrentTile.OccupiedBy = this;
            StartCoroutine(Slide(previousPosition, CurrentTile.Position, 0.2f));
            return true;
        }

        if (targetTile.IsOccupied)
        {
            Debug.Log(characterName + " is blocked by " + targetTile.OccupiedBy.characterName);
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
        // We'll assume y-axis rotation only
        float yRotation = 0f;

        switch (FacingDirection)
        {
            case Direction.North:
                yRotation = 0f;
                break;
            case Direction.East:
                yRotation = 90f;
                break;
            case Direction.South:
                yRotation = 180f;
                break;
            case Direction.West:
                yRotation = 270f;
                break;
        }

        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }
}
