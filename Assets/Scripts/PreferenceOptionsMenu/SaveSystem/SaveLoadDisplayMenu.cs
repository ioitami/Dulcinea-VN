using Ink.Parsed;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SaveLoadDisplayMenu : MonoBehaviour
{
    public int lastVisitedPage = 1;
    public int numPages = 9;
    [Space]
    public GameObject savePagePrefab;
    public GameObject loadPagePrefab;
    [Space]
    public GameObject saveLoadGameButtonPrefab;
    [Space]
    public GameObject saveLoadPageListBar;
    public GameObject savePage_Parent;
    public GameObject loadPage_Parent;
    [Space]
    public List<GameObject> savePageList;
    public List<GameObject> loadPageList;

    private void Awake()
    {
        RefreshAllSaveLoadSlots();
    }

    public void RefreshAllSaveLoadSlots()
    {
        // Clear existing save/load slots from save/loadpage parents
        foreach (Transform child in savePage_Parent.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in loadPage_Parent.transform)
        {
            Destroy(child.gameObject);
        }


        // spawn prefab for save and load pages, each spawned page needs to set the game slot number in the SaveLoadGameButton
        // script, then load the save/load thumbnail image, sprite, text etc.
        for (int i = 0; i < numPages; i++)
        {
            GameObject savePage = Instantiate(savePagePrefab, savePage_Parent.transform);

        }


        // Hide all save/load pages, then show last visited page for both save/load.


        for (int i = 0; i < savePageList.Count; i++)
        {
            //saveGameButtonSlots[i].saveSpritePreview.sprite = GameSingleton.instance.gameStateManager.LoadScreenshotSprite(i + 1);
        }

        for (int i = 0; i < loadPageList.Count; i++)
        {
            //loadGameButtonSlots[i].loadSpritePreview.sprite = GameSingleton.instance.gameStateManager.LoadScreenshotSprite(i + 1);
        }
    }

    //private void RefreshButton()
    //{
    //    SaveData data = GameSingleton.instance.gameStateManager.LoadSaveID(loadSlotNumber);

    //    if (data == null)
    //    {
    //        loadSpritePreview.sprite = emptySaveSprite;
    //        saveID_Text.text = "Empty";
    //        return;
    //    }

    //    Sprite screenshot = GameSingleton.instance.gameStateManager.GetSaveScreenshotSprite(loadSlotNumber);

    //    if (screenshot != null)
    //        loadSpritePreview.sprite = screenshot;
    //    else
    //        loadSpritePreview.sprite = emptySaveSprite;

    //    saveID_Text.text = "Save " + data.saveID.ToString();
    //}
}
