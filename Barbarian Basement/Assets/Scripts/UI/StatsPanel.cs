using TMPro;
using UnityEngine;

public class StatsPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameField;
    [SerializeField] private TextMeshProUGUI _bodyPointsField;
    [SerializeField] private TextMeshProUGUI _attackDiceField;
    [SerializeField] private TextMeshProUGUI _defendDice;

    public void UpdateStatsPanel(Player player)
    {
        _nameField.text = $"{player.CharacterName}";
        _bodyPointsField.text = $"Body points: {player.CurrentBodyPoints}";
        _attackDiceField.text = $"Attack dice: {player.AttackDice}";
        _defendDice.text = $"Defend Dice: {player.DefendDice}";
    }
}
