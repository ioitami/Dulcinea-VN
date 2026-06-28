using Mirror;
using TMPro;
using UnityEngine;

public class NVLSyncText : NetworkBehaviour
{
    public TextMeshProUGUI text;

    [SyncVar(hook = nameof(OnTextChanged))]
    private string syncedText = "";

    private void Awake()
    {
        if (text == null)
            text = GetComponent<TextMeshProUGUI>();
    }

    // Called by server to update text
    [Server]
    public void SetText(string newText)
    {
        syncedText = newText;
    }

    // Automatically called on client when syncedText changes
    private void OnTextChanged(string oldText, string newText)
    {
        if (text != null)
            text.text = newText;
    }

    // Keep server text in sync if changed directly
    private void Update()
    {
        if (!isServer) return;
        if (text == null) return;

        if (text.text != syncedText)
            SetText(text.text);
    }
}