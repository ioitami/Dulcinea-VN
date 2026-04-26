using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class SaveGameButton : MonoBehaviour
{
    public int saveSlotNumber;
    public Image thumbnailImage;
    public Sprite emptySaveSprite;
    public TextMeshProUGUI saveID_Text;
    public TextMeshProUGUI chapterName_Text;
    public TextMeshProUGUI saveTimeStamp_Text;

    private void OnEnable()
    {
        RefreshButton();
    }

    private void RefreshButton()
    {
        SaveData data = GameSingleton.instance.gameStateManager.Load(saveSlotNumber);

        if (data == null)
        {
            thumbnailImage.sprite = emptySaveSprite;
            saveID_Text.text = "";
            chapterName_Text.text = "";
            saveTimeStamp_Text.text = "";
            return;
        }

        Sprite screenshot = GameSingleton.instance.gameStateManager.GetSaveScreenshotSprite(saveSlotNumber);

        if (screenshot != null)
            thumbnailImage.sprite = screenshot;
        else
            thumbnailImage.sprite = emptySaveSprite;

        saveID_Text.text = "Save " + data.saveID.ToString();
        chapterName_Text.text = data.chapterName;
        saveTimeStamp_Text.text = data.saveTimeStamp;
    }

    public void OnSaveClicked()
    {
        GameSingleton.instance.gameStateManager.Save(saveSlotNumber, OnSaveComplete);
    }

    private void OnSaveComplete(SaveData data)
    {
        RefreshButton();
    }
}