using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadGameButton : MonoBehaviour
{
    public int loadSlotNumber;
    public Image loadSpritePreview;
    public TextMeshProUGUI saveID_Text;
    public Sprite emptySaveSprite;
    //public TextMeshProUGUI chapterName_Text;
    //public TextMeshProUGUI saveTimeStamp_Text;

    private void Start()
    {
        RefreshButton();
    }

    private void RefreshButton()
    {
        SaveData data = GameSingleton.instance.gameStateManager.LoadSaveID(loadSlotNumber);

        if (data == null)
        {
            loadSpritePreview.sprite = emptySaveSprite;
            saveID_Text.text = "Empty";
            return;
        }

        Sprite screenshot = GameSingleton.instance.gameStateManager.GetSaveScreenshotSprite(loadSlotNumber);

        if (screenshot != null)
            loadSpritePreview.sprite = screenshot;
        else
            loadSpritePreview.sprite = emptySaveSprite;

        saveID_Text.text = "Save " + data.saveID.ToString();
    }

    public void LoadGame()
    {
        GameSingleton.instance.gameStateManager.LoadGame(loadSlotNumber);
    }
}