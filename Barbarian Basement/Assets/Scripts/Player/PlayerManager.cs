using System.Collections;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private bool _playerMoved;
    private bool _playerUsedAction;

    [SerializeField] private Player _character;

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
                        //yield return new WaitUntil(() => !Input.GetKey(KeyCode.W));
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
                //get the next tile
                var attackTargetTile = MoveUtils.GetTargetTile(_character.CurrentTile,
                    _character.FacingDirection, GameManager.Instance.FinalGrid);

                //if there is a valid tile and something is standing on it
                if (attackTargetTile != null && attackTargetTile.IsOccupied && attackTargetTile.OccupiedByCharacter)
                {
                    //attack the target
                    _character.PlayHitEffect();
                    var target = attackTargetTile.OccupiedByCharacter;
                    CombatUtils.Attack(_character, target);
                    _playerUsedAction = true;
                    yield return new WaitForSeconds(CombatUtils.CombatTurnStartDelay);
                    break;
                }
                else
                {
                    Debug.Log("no valid target");
                }
            }
            // attempt interaction
            if (Input.GetKeyDown(KeyCode.E))
            {
                //get the tile we're facing
                var interactableTargetTile = MoveUtils.GetTargetTile(_character.CurrentTile,
                    _character.FacingDirection, GameManager.Instance.FinalGrid);

                //check it has an interactable
                if (interactableTargetTile != null && interactableTargetTile.IsOccupied && interactableTargetTile.OccupiedByInteractable)
                {
                    interactableTargetTile.OccupiedByInteractable.StartInteraction();
                    _playerUsedAction = true;
                    break;
                }
                else
                {
                    Debug.Log("nothing to interact with");
                }
            }

            yield return null;
        }
        TurnManager.Instance.EndTurn();
    }
}