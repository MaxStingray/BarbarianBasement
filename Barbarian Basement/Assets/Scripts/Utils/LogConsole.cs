using UnityEngine;
using TMPro;

public class LogConsole : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI logText;
    [SerializeField] private int maxLines = 10;  // Limit to avoid memory bloat

    private string _logCache = "";

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        _logCache += logString + "\n";
        string[] lines = _logCache.Split('\n');
        if (lines.Length > maxLines)
        {
            _logCache = string.Join("\n", lines, lines.Length - maxLines, maxLines);
        }
        logText.text = _logCache;
    }
}
