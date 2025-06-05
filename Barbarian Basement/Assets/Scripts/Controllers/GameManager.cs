using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private DunGen _dungeonGenerator;
    [SerializeField] private GameObject _player;
    IEnumerator Start()
    {
        _dungeonGenerator.GenerateDungeon();
        while (!_dungeonGenerator.DungeonGenerated)
        {
            yield return null;
        }

        _player.transform.position = _dungeonGenerator.PlayerSpawnPosition;
    }
}
