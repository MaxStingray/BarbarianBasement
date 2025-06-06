using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public enum Turn
{
    Player,
    Enemy,
}

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public Turn CurrentTurn { get; private set; }

    public UnityEvent OnPlayerTurnStart = new UnityEvent();
    public UnityEvent OnPlayerTurnEnd = new UnityEvent();
    public UnityEvent OnEnemyTurnStart = new UnityEvent();
    public UnityEvent OnEnemyTurnEnd = new UnityEvent();

    private Turn[] _turnOrder;

    private int _turnIterator = 0;
    private bool _turnEnding = false;


    void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance);
        }

        Instance = this;
    }

    IEnumerator Start()
    {
        //establish the turn order (hardcoded for now)
        _turnOrder = new Turn[] { Turn.Player, Turn.Enemy };
        _turnIterator = 0;

        while (GameManager.Instance.GameReady == false)
        {
            yield return null;
        }

        //one extra frame so the playermanager can catch up
        yield return null;

        CurrentTurn = _turnOrder[_turnIterator];
        EnterTurn();
    }

    private void EnterTurn()
    {
        switch (_turnOrder[_turnIterator])
        {
            case Turn.Player:
                Debug.Log("player's turn");
                OnPlayerTurnStart.Invoke();
                break;
            case Turn.Enemy:
                Debug.Log("enemy turn");
                OnEnemyTurnStart.Invoke();
                break;
        }
    }

    private void EnterNextTurn()
    {
        _turnIterator++;

        if (_turnIterator >= _turnOrder.Length)
        {
            _turnIterator = 0;
        }

        EnterTurn();
    }

    public void EndTurn()
    {
        if (_turnEnding)
        {
            return;
        }

        _turnEnding = true;

        switch (CurrentTurn)
        {
            case Turn.Player:
                OnPlayerTurnEnd.Invoke();
                break;
            case Turn.Enemy:
                OnEnemyTurnEnd.Invoke();
                break;
        }

        StartCoroutine(WaitThenEnterNextTurn());
    }

    IEnumerator WaitThenEnterNextTurn()
    {
        yield return new WaitForSeconds(0.1f);
        _turnEnding = false;
        EnterNextTurn();
    }
}
