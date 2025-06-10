using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private TMP_InputField _nameEntryField;
    [SerializeField] private Button _startButton;

    [SerializeField] private GameObject _menuRoot;

    void Start()
    {
        _startButton.onClick.AddListener(HandleStartPressed);
    }

    private void HandleStartPressed()
    {
        string newName = _nameEntryField.text;
        GameManager.Instance.Player.UpdateName(newName);
        GameManager.Instance.StartNewGame();
        _menuRoot.SetActive(false);
    }
}
