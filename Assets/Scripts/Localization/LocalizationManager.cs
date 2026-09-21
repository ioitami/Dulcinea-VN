using System.Collections.Generic;
using Mirror;
using UnityEngine;

// No translated content exists yet, so Get() will always returns the authored fallback text) until a CSV table is added
// for a given language under Resources/Localization/<code>.csv.
public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager instance { get; private set; }

    [Header("Language")]
    public string currentLanguageCode = "en";

    private const string TableResourceFolder = "Localization/";

    private readonly Dictionary<string, string> currentTable = new Dictionary<string, string>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void Start()
    {
        ApplyLanguageLocally(currentLanguageCode);
    }

    // Returns the localized string for key in the current language, or
    // fallback unchanged if no table is loaded or the key isn't in it.
    public string Get(string key, string fallback)
    {
        if (string.IsNullOrEmpty(key)) return fallback;

        if (currentTable.TryGetValue(key, out string value) && !string.IsNullOrEmpty(value))
            return value;

        return fallback;
    }

    // Only window 1 (the host) should ever decide the language, but guard
    // anyway: a pure client should never set this itself.
    public void SetLanguage(string languageCode)
    {
        if (NetworkClient.active && !NetworkServer.active) return;

        ApplyLanguageLocally(languageCode);

        if (NetworkServer.active)
            NVLNetworkPlayer.hostInstance?.SetLanguage(languageCode);
    }

    // Applied on this window directly, and on window 2 via NVLNetworkPlayer's
    // synced language hook — same method, so both windows always agree.
    public void ApplyLanguageLocally(string languageCode)
    {
        if (string.IsNullOrEmpty(languageCode)) return;

        currentLanguageCode = languageCode;
        currentTable.Clear();

        TextAsset table = Resources.Load<TextAsset>(TableResourceFolder + languageCode);

        if (table == null)
        {
            Debug.Log($"[LocalizationManager] No string table found for '{languageCode}' — using authored default text.");
            return;
        }

        ParseCsv(table.text);
    }

    // Simple "key,value" per line, with optional double-quoting for values
    // that contain commas (standard CSV quoting, doubled-quote escaping).
    private void ParseCsv(string csvText)
    {
        string[] lines = csvText.Split('\n');

        foreach (string rawLine in lines)
        {
            string line = rawLine.TrimEnd('\r');
            if (string.IsNullOrWhiteSpace(line)) continue;

            int separatorIndex = line.IndexOf(',');
            if (separatorIndex < 0) continue;

            string key = line.Substring(0, separatorIndex).Trim();
            string value = line.Substring(separatorIndex + 1).Trim();

            if (value.Length >= 2 && value.StartsWith("\"") && value.EndsWith("\""))
            {
                value = value.Substring(1, value.Length - 2).Replace("\"\"", "\"");
            }

            if (!string.IsNullOrEmpty(key))
                currentTable[key] = value;
        }
    }

    // Stable, deterministic keys derived purely from existing IDs/position —
    // no new serialized fields are needed on DialogueBlocks/nodes to make
    // their content localizable.
    public static string MakeDialogueTextKey(string blockID, int nodeIndex) => $"line_{blockID}_{nodeIndex}";
    public static string MakeChoiceKey(string blockID, int nodeIndex, int choiceIndex) => $"choice_{blockID}_{nodeIndex}_{choiceIndex}";
    public static string MakeBlockKey(string prefix, string blockID) => $"{prefix}_{blockID}";
}