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
    public GameObject saveLoadPageButtonPrefab;
    [Space]
    public Transform savePageList_Parent;
    public Transform loadPageList_Parent;
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

        GameObject autoSavePage = Instantiate(saveLoadPagePrefab, savePage_Parent);
        GameObject quickSavePage = Instantiate(saveLoadPagePrefab, savePage_Parent);

        autoLoadPage.name = "LoadPage_Auto";
        quickLoadPage.name = "LoadPage_Quick";

        quickSavePage.name = "SavePage_Quick";
        autoSavePage.name = "SavePage_Auto";

        loadPageList.Add(autoLoadPage);
        loadPageList.Add(quickLoadPage);

        savePageList.Add(autoSavePage);
        savePageList.Add(quickSavePage);

        for (int i = 0; i < saveSlotsPerPage; i++)
        {
            autoLoadPage.transform.GetChild(i).GetComponent<SaveLoadGameButton>().isSaveBtn = false;
            quickLoadPage.transform.GetChild(i).GetComponent<SaveLoadGameButton>().isSaveBtn = false;

            autoSavePage.transform.GetChild(i).GetComponent<SaveLoadGameButton>().isSaveBtn = true;
            quickSavePage.transform.GetChild(i).GetComponent<SaveLoadGameButton>().isSaveBtn = true;

            autoLoadPage.transform.GetChild(i).GetComponent<SaveLoadGameButton>().saveLoadSlotNumber = -1 - i;
            quickLoadPage.transform.GetChild(i).GetComponent<SaveLoadGameButton>().saveLoadSlotNumber = -1 - saveSlotsPerPage - i;

            autoSavePage.transform.GetChild(i).GetComponent<SaveLoadGameButton>().saveLoadSlotNumber = -1 - i;
            quickSavePage.transform.GetChild(i).GetComponent<SaveLoadGameButton>().saveLoadSlotNumber = -1 - saveSlotsPerPage - i;
        }

        // spawn prefab for save and load pages, each spawned page needs to set the game slot number in the SaveLoadGameButton
        // script, then load the save/load thumbnail image, sprite, text etc.
        for (int i = 0; i < numPages; i++)
        {
            GameObject savePage = Instantiate(saveLoadPagePrefab, savePage_Parent);
            GameObject loadPage = Instantiate(saveLoadPagePrefab, loadPage_Parent);

            savePage.name = "SavePage_" + (i + 1);
            loadPage.name = "LoadPage_" + (i + 1);

            savePageList.Add(savePage);
            loadPageList.Add(loadPage);

            for (int j = 0; j < saveSlotsPerPage; j++)
            {
                Transform currentSaveSlot = savePage.transform.GetChild(j);
                Transform currentLoadSlot = loadPage.transform.GetChild(j);
                currentSaveSlot.GetComponent<SaveLoadGameButton>().isSaveBtn = true;
                currentLoadSlot.GetComponent<SaveLoadGameButton>().isSaveBtn = false;

                currentSaveSlot.GetComponent<SaveLoadGameButton>().saveLoadSlotNumber = i * saveSlotsPerPage + j + 1;
                currentLoadSlot.GetComponent<SaveLoadGameButton>().saveLoadSlotNumber = i * saveSlotsPerPage + j + 1;

                currentSaveSlot.name = "SaveSlot_" + (i * saveSlotsPerPage + j + 1);
                currentLoadSlot.name = "LoadSlot_" + (i * saveSlotsPerPage + j + 1);

                // Load each savedata here


            }
        }

        // Delete all Page List Buttons in SavePageList_Parent and LoadPageList_Parent, then spawn in max page number,
        // including A and Q buttons, then link them all to each save/load page
        foreach (Transform child in savePageList_Parent)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in loadPageList_Parent)
        {
            Destroy(child.gameObject);
        }

        GameObject AutoSavePageBtn = Instantiate(saveLoadPageButtonPrefab, savePageList_Parent);
        GameObject AutoLoadPageBtn = Instantiate(saveLoadPageButtonPrefab, loadPageList_Parent);

        GameObject QuickSavePageBtn = Instantiate(saveLoadPageButtonPrefab, savePageList_Parent);
        GameObject QuickLoadPageBtn = Instantiate(saveLoadPageButtonPrefab, loadPageList_Parent);

        for (int i = 0; i < numPages; i++)
        {
            GameObject savePageBtn = Instantiate(saveLoadPageButtonPrefab, savePageList_Parent);
            GameObject loadPageBtn = Instantiate(saveLoadPageButtonPrefab, loadPageList_Parent);
        }



        HideAllPages();
    }

    public void OpenSavePage(int pageNum)
    {
        foreach (GameObject savePage in savePageList)
        {
            savePage.SetActive(false);
        }

        savePageList[pageNum].SetActive(true);
        lastVisitedPage = pageNum;
    }

    public void OpenLoadPage(int pageNum)
    {
        foreach (GameObject loadPage in loadPageList)
        {
            loadPage.SetActive(false);
        }

        loadPageList[pageNum].SetActive(true);
        lastVisitedPage = pageNum;
    }

    public void HideAllPages()
    {
        foreach (GameObject savePage in savePageList)
        {
            savePage.SetActive(false);
        }
        foreach (GameObject loadPage in loadPageList)
        {
            loadPage.SetActive(false);
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
