using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class InteractablePrefabs
{
    public GameObject Prefab;
    public int Weight;
    public string Name;
}

public class InteractableManager : MonoBehaviour
{
    [SerializeField] private GameObject _treasureChestPrefab;
    public GameObject TreasureChestPrefab => _treasureChestPrefab;

    [SerializeField] private GameObject _merchantPrefab;
    public GameObject MerchantPrefab => _merchantPrefab;

    [SerializeField] private InteractablePrefabs[] _interactablePrefabs;

    private List<GameObject> _spawnedInteractables = new List<GameObject>();

    public GameObject GetRandomInteractable()
    {
        int totalWeight = 0;
        foreach (var interactable in _interactablePrefabs)
        {
            totalWeight += interactable.Weight;
        }

        int rand = Random.Range(0, totalWeight);
        int cumulative = 0;
        foreach (var interactable in _interactablePrefabs)
        {
            cumulative += interactable.Weight;
            if (rand < cumulative)
            {
                return interactable.Prefab;
            }
        }

        // Fallback (shouldn't happen)
        return _interactablePrefabs[0].Prefab;
    }

    public void SpawnInteractables(List<GameTile> tiles)
    {
        //clean up any existing interactables
        if (_spawnedInteractables.Count > 0)
        {
            foreach (var spawned in _spawnedInteractables)
            {
                Destroy(spawned);
            }

            _spawnedInteractables.Clear();
        }

        if (tiles.Count <= 0)
        {
            return;
        }
        var max = tiles.Count - 1;
        //place the merchant and the single chest (random places for now)
        int merchantIndex = Random.Range(0, max);
        PlaceInteractable(MerchantPrefab, tiles[merchantIndex]);
        //remove the tile from the collection so it can't be used again
        tiles.RemoveAt(merchantIndex);
        int chestIndex = Random.Range(0, max);
        PlaceInteractable(TreasureChestPrefab, tiles[chestIndex]);
        tiles.RemoveAt(chestIndex);

        //fill the rest (if any)
        foreach (var tile in tiles)
        {
            if (!tile.IsOccupied)
            {
                PlaceInteractable(GetRandomInteractable(), tile);
            }
        }
    }

    private void PlaceInteractable(GameObject prefab, GameTile tile)
    {
        GameObject prefabGO = Instantiate(prefab, tile.Position, Quaternion.identity);
        var interactable = prefabGO.GetComponent<Interactable>();

        if (!interactable)
        {
            Debug.LogError($"Error while placing prefab: {prefab.name}, no valid Interactable component found");
            return;
        }

        tile.IsOccupied = true;
        tile.OccupiedByInteractable = interactable;

        _spawnedInteractables.Add(prefabGO);
    }
}
