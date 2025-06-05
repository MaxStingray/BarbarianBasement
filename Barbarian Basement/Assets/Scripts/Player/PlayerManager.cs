using System.Collections;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private bool _playerMoved;
    private bool _playerUsedAction;

    [SerializeField] private CharacterSheet _character;

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
            Debug.Log("AAA");
            yield return null;
        }

        TurnManager.Instance.OnPlayerTurnStart.AddListener(HandleTurnStart);
    }

    private void HandleTurnStart()
    {
        _playerMoved = false;
        _playerUsedAction = false;
        StartCoroutine(AwaitAction());
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
                    }
                }
                else
                {
                    Debug.Log("No valid tile to move to.");
                }
            }

            yield return null;
        }

        TurnManager.Instance.EndTurn();
    }
}
