using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugLogger : MonoBehaviour
{
    public static DebugLogger instance { get; private set; }

    public TextMeshProUGUI logText;
    public int maxLines = 20;

    private Queue<string> logLines = new Queue<string>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        Application.logMessageReceived += OnLogReceived;

        if (logText != null)
            logText.text = "";
        else
            Debug.LogError("[DebugLogger] logText is not assigned.");
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= OnLogReceived;
    }

    private void OnLogReceived(string condition, string stackTrace, LogType type)
    {
        string prefix;

        if (type == LogType.Error || type == LogType.Exception)
            prefix = "[ERROR] ";
        else if (type == LogType.Warning)
            prefix = "[WARN] ";
        else
            prefix = "[LOG] ";

        logLines.Enqueue(prefix + condition);

        while (logLines.Count > maxLines)
            logLines.Dequeue();

        if (logText != null)
            logText.text = string.Join("\n", logLines);
    }

    public void Log(string message)
    {
        logLines.Enqueue("[LOG] " + message);

        while (logLines.Count > maxLines)
            logLines.Dequeue();

        if (logText != null)
            logText.text = string.Join("\n", logLines);
    }

    public void Clear()
    {
        logLines.Clear();

        if (logText != null)
            logText.text = "";
    }
}