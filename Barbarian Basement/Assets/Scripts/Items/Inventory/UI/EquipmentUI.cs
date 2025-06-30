using System.Collections.Generic;
using UnityEngine;

public class EquipmentUI : MonoBehaviour
{
    //the equipList this inventory is referencing
    public EquipList EquipList;
    [SerializeField] private EquipmentSlotUI _headSlot;
    [SerializeField] private EquipmentSlotUI _bodySlot;
    [SerializeField] private GameObject _equipSlotUIprefab;
    [SerializeField] private Transform _accessoryRoot;

    private List<EquipmentSlotUI> _accessorySlotButtons = new List<EquipmentSlotUI>();

    void Start()
    {
        // initialise head and body slots as they are persistent
        _headSlot.Init(EquipList.HeadSlot, GameManager.Instance.Player);
        _bodySlot.Init(EquipList.BodySlot, GameManager.Instance.Player);
    }

    public void OnClickEquip(Equipment equipment)
    {
        CharacterSheet character = GameManager.Instance.Player;
        // Equip the item logically
        EquipList.EquipItem(equipment, character);

        // Recalculate stats
        EquipUtils.CalculateStats(character);
        GameManager.Instance.StatsPanel.UpdateStatsPanel(GameManager.Instance.Player);
    }

    void OnEnable()
    {
        if (EquipList)
        {
            EquipList.OnEquipmentChanged.AddListener(UpdateUI);
        }

        UpdateUI();
    }

    void OnDisable()
    {
        if (EquipList)
        {
            EquipList.OnEquipmentChanged.RemoveListener(UpdateUI);
        }
    }

    public void UpdateUI()
    {
        CharacterSheet character = GameManager.Instance.Player;

        _headSlot.Refresh();
        _bodySlot.Refresh();

        // Clear existing accessory UI
        foreach (var btn in _accessorySlotButtons)
        {
            Destroy(btn.gameObject);
        }
        _accessorySlotButtons.Clear();

        // Rebuild accessory UI
        foreach (var slot in EquipList.AccessorySlots)
        {
            GameObject go = Instantiate(_equipSlotUIprefab, _accessoryRoot);
            EquipmentSlotUI ui = go.GetComponent<EquipmentSlotUI>();
            ui.Init(slot, character);
            ui.Refresh();
            _accessorySlotButtons.Add(ui);
        }
    }
}
