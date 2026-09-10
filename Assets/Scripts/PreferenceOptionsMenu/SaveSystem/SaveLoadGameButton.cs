using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class SaveLoadGameButton : MonoBehaviour
{
    public bool isSaveBtn;
    public int saveLoadSlotNumber;
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
        SaveData data = GameSingleton.instance.gameStateManager.LoadSaveID(saveLoadSlotNumber);

        if (data == null)
        {
            thumbnailImage.sprite = emptySaveSprite;
            saveID_Text.text = "";
            chapterName_Text.text = "";
            saveTimeStamp_Text.text = "";
            return;
        }

        Sprite screenshot = GameSingleton.instance.gameStateManager.GetSaveScreenshotSprite(saveLoadSlotNumber);

        if (screenshot != null)
            thumbnailImage.sprite = screenshot;
        else
            thumbnailImage.sprite = emptySaveSprite;

        saveID_Text.text = "Save " + data.saveID.ToString();
        chapterName_Text.text = data.chapterName;
        saveTimeStamp_Text.text = data.saveTimeStamp;
    }

    public void OnSaveLoadButtonClicked()
    {
        if (isSaveBtn == true)
        {
            GameSingleton.instance.gameStateManager.Save(saveLoadSlotNumber, OnSaveComplete);
        }
        else
        {
            GameSingleton.instance.gameStateManager.LoadGame(saveLoadSlotNumber);
        }

    }

    private void OnSaveComplete(SaveData data)
    {
        RefreshButton();
    }

}