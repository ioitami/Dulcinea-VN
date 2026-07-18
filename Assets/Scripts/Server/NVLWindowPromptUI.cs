using TMPro;
using UnityEngine;

// Per-window prompt panel shown while the game is waiting on the other
// window (either for it to open, or for the player to close it).
// Message text is editable per-window in the Inspector.
public class NVLWindowPromptUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject panelRoot;
    public TextMeshProUGUI messageText;

    [Header("Adjustable Text")]
    [TextArea(2, 4)]
    public string waitingForSecondWindowText = "Waiting for the second window to open...";

    [TextArea(2, 4)]
    public string pleaseCloseSecondWindowText = "Please close the second window to continue.";

    private void Awake()
    {
        Hide();
    }

    public void ShowWaitingForSecondWindow()
    {
        Show(waitingForSecondWindowText);
    }

    public void ShowPleaseCloseSecondWindow()
    {
        Show(pleaseCloseSecondWindowText);
    }

    public void Show(string message)
    {
        if (panelRoot != null)
            panelRoot.SetActive(true);

        if (messageText != null)
            messageText.text = message;
    }

    public void Hide()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }
}
