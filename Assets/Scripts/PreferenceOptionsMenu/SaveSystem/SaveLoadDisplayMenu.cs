using Ink.Parsed;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SaveLoadDisplayMenu : MonoBehaviour
{
    public int lastVisitedPage = 1;
    public int numPages = GlobalVariables.totalSavePageNumber;
    [Space]
    public GameObject saveLoadPagePrefab;
    [Space]
    public GameObject saveLoadPageListBar;
    public Transform saveLoadPageList_Parent;
    public Transform savePage_Parent;
    public Transform loadPage_Parent;
    [Space]
    public List<GameObject> savePageList;
    public List<GameObject> loadPageList;

    private void Awake()
    {
        RefreshAllSaveLoadSlots();
    }

    public void RefreshAllSaveLoadSlots()
    {
        int saveSlotsPerPage = saveLoadPagePrefab.transform.childCount;


        // Clear existing save/load slots from save/loadpage parents
        foreach (Transform child in savePage_Parent)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in loadPage_Parent)
        {
            Destroy(child.gameObject);
        }

        // Set up save/load auto and quicksave pages
        GameObject autoLoadPage = Instantiate(saveLoadPagePrefab, loadPage_Parent);
        GameObject quickLoadPage = Instantiate(saveLoadPagePrefab, loadPage_Parent);
        autoLoadPage.name = "LoadPage_Auto";
        quickLoadPage.name = "LoadPage_Quick";
        loadPageList.Add(autoLoadPage);
        loadPageList.Add(quickLoadPage);

        for (int i = 0; i < saveSlotsPerPage; i++)
        {
            autoLoadPage.transform.GetChild(i).GetComponent<SaveLoadGameButton>().isSaveBtn = false;
            quickLoadPage.transform.GetChild(i).GetComponent<SaveLoadGameButton>().isSaveBtn = false;

            autoLoadPage.transform.GetChild(i).GetComponent<SaveLoadGameButton>().saveLoadSlotNumber = -1 - i;
            quickLoadPage.transform.GetChild(i).GetComponent<SaveLoadGameButton>().saveLoadSlotNumber = -1 - saveSlotsPerPage - i;
        }

        // spawn prefab for save and load pages, each spawned page needs to set the game slot number in the SaveLoadGameButton
        // script, then load the save/load thumbnail image, sprite, text etc.
        for (int i = 0; i < numPages; i++)
        {
            //GameObject savePage = Instantiate(savePagePrefab, savePage_Parent.transform);

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
