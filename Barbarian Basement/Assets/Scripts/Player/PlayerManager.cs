using System.Collections;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private bool _playerMoved;
    private bool _playerUsedAction;

    [SerializeField] private Player _character;

    [SerializeField] private GameObject _inventoryWindow;

    private Coroutine _playerActionCoroutine;

    void Awake()
    {
        if (TurnManager.Instance == null)
        {
            StartCoroutine(ValidateTurnManager());
        }
        else
        {
            TurnManager.Instance.OnPlayerTurnStart.AddListener(HandleTurnStart);
            TurnManager.Instance.OnPlayerTurnEnd.AddListener(HandleTurnEnd);
        }
    }

    IEnumerator ValidateTurnManager()
    {
        while (TurnManager.Instance == null)
        {
            yield return null;
        }

        TurnManager.Instance.OnPlayerTurnStart.AddListener(HandleTurnStart);
        TurnManager.Instance.OnPlayerTurnEnd.AddListener(HandleTurnEnd);
    }

    private void HandleTurnEnd()
    {
        if (_playerActionCoroutine != null)
        {
            StopCoroutine(_playerActionCoroutine);
            _playerActionCoroutine = null;
        }
    }

    private void HandleTurnStart()
    {
        _playerMoved = false;
        _playerUsedAction = false;

        // Stop any existing coroutine
        if (_playerActionCoroutine != null)
        {
            StopCoroutine(_playerActionCoroutine);
            _playerActionCoroutine = null;
        }

        _playerActionCoroutine = StartCoroutine(AwaitAction());
    }

    IEnumerator AwaitAction()
    {
        while (!_playerMoved && !_playerUsedAction)
        {

            if (Input.GetKeyDown(KeyCode.I))
            {
                var windowState = _inventoryWindow.activeInHierarchy;

                _inventoryWindow.SetActive(!windowState);
            }


            if (Input.GetKeyDown(KeyCode.D))
            {
                _character.Turn(clockwise: true);
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                _character.Turn(clockwise: false);
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                var targetTile = MoveUtils.GetTargetTile(
                    _character.CurrentTile,
                    _character.FacingDirection,
                    GameManager.Instance.FinalGrid);

                if (targetTile != null)
                {
                    bool moved = _character.AttemptMove(targetTile);
                    if (moved)
                    {
                        _playerMoved = true;
                        break;
                    }
                }
                else
                {
                    Debug.Log("No valid tile to move to.");
                }
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                yield return StartCoroutine(AttackRoutine());
            }
            // attempt interaction
            if (Input.GetKeyDown(KeyCode.E))
            {
                AttemptInteraction();
            }

            yield return null;
        }
        TurnManager.Instance.EndTurn();
    }

    /// <summary>
    /// attempt to interact with the adjacent tile (special rules for doors)
    /// </summary>
    private void AttemptInteraction()
    {
        var facing = _character.FacingDirection;
        var currentTile = _character.CurrentTile;
        var grid = GameManager.Instance.FinalGrid;
        var targetTile = MoveUtils.GetTargetTile(currentTile, facing, grid);

        GameTile interactionTile = null;

        //Case 1: Interactable in front
        if (targetTile != null && targetTile.IsOccupied && targetTile.OccupiedByInteractable != null)
        {
            interactionTile = targetTile;
        }
        //Case 2: Door on the current tile's wall (e.g. facing out from this tile)
        else if (currentTile.HasDoor &&
                currentTile.OccupiedByInteractable is Door door &&
                door.WallDirection == facing)
        {
            interactionTile = currentTile;
        }
        //Case 3: Door on the target tile, facing bacl toward us
        else if (targetTile != null &&
                targetTile.HasDoor &&
                targetTile.OccupiedByInteractable is Door doorBack &&
                doorBack.WallDirection == MoveUtils.GetOppositeDirection(facing))
        {
            interactionTile = targetTile;
        }

        if (interactionTile != null)
        {
            interactionTile.OccupiedByInteractable.StartInteraction();
            SetUsedAction();
        }
        else
        {
            Debug.Log("nothing to interact with");
        }
    }

    /// <summary>
    /// attacks with the currently active player - standard attack (not using a weapon)
    /// </summary>
    /// <returns></returns>
    private IEnumerator AttackRoutine()
    {
        var attackTargetTile = MoveUtils.GetTargetTile(_character.CurrentTile,
        _character.FacingDirection, GameManager.Instance.FinalGrid);

        //if there is a valid tile and something is standing on it
        if (attackTargetTile != null && attackTargetTile.IsOccupied && attackTargetTile.OccupiedByCharacter)
        {

            var target = attackTargetTile.OccupiedByCharacter;
            bool hit;
            CombatUtils.Attack(_character.CurrentAttackDice, _character, target, out hit);
            yield return StartCoroutine(CombatUtils.PlayCharacterAttackEffects(_character, hit));
            SetUsedAction();
        }
        else
        {
            Debug.Log("no valid target");
        }
    }

    public void SetUsedAction()
    {
        _playerUsedAction = true;
    }
}