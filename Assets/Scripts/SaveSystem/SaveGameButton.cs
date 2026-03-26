using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class SaveGameButton : MonoBehaviour
{
    public int saveSlotNumber;
    public Image saveSpritePreview;
    public TextMeshProUGUI saveID_Text;
    //public TextMeshProUGUI chapterName_Text;
    //public TextMeshProUGUI saveTimeStamp_Text;

    public void SaveGame(int saveFileNumber)
    {
        //GameSingleton.instance.gameStateManager?.SaveGame(saveFileNumber);

        StartCoroutine(DelayedSpritePreview(0.05f));
    }

    IEnumerator DelayedSpritePreview(float delay)
    {
        yield return new WaitForSeconds(delay);

        //saveSpritePreview.sprite = GameSingleton.instance.gameStateManager.GetLoadedScreenshotSprite();
        //GameSingleton.instance.sceneLoaderManager.uiController.saveLoadMenu.UpdateSaveLoadSlots();
    }

}
