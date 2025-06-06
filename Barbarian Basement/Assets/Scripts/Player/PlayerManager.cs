using System.Collections;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private bool _playerMoved;
    private bool _playerUsedAction;

    [SerializeField] private CharacterSheet _character;

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
        }
    }

    IEnumerator ValidateTurnManager()
    {
        while (TurnManager.Instance == null)
        {
            yield return null;
        }

        Debug.Log("turn manager found");
        TurnManager.Instance.OnPlayerTurnStart.AddListener(HandleTurnStart);
        TurnManager.Instance.OnPlayerTurnEnd.AddListener(HandleTurnEnd);
    }

    private void HandleTurnEnd()
    {
        if (_playerActionCoroutine != null)
        {
            StopCoroutine(_playerActionCoroutine);
        }
    }

    private void HandleTurnStart()
    {
        Debug.Log("player turn start");
        _playerMoved = false;
        _playerUsedAction = false;

        // Stop any existing coroutine
        if (_playerActionCoroutine != null)
        {
            StopCoroutine(_playerActionCoroutine);
        }

        _playerActionCoroutine = StartCoroutine(AwaitAction());
    }

    IEnumerator AwaitAction()
    {
        Debug.Log("awaiting player input");
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
                    Debug.Log($"CurrentTile: NorthWall: {_character.CurrentTile.NorthWall}");
                    Debug.Log($"TargetTile: SouthWall: {targetTile.SouthWall}");
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
                if (attackTargetTile != null && attackTargetTile.IsOccupied)
                {
                    //attack the target
                    var target = attackTargetTile.OccupiedBy;
                    CombatUtils.Attack(_character, target);
                    _playerUsedAction = true;
                    break;
                }
                else
                {
                    Debug.Log("no valid target");
                }
            }

            yield return null;
        }

        TurnManager.Instance.EndTurn();
    }
}