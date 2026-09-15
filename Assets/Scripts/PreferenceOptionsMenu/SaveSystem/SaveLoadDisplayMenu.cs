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

            autoSavePage.transform.GetChild(i).GetComponent<SaveLoadGameButton>().saveSlotNumber_Text.text = "A " + (i + 1);
            quickSavePage.transform.GetChild(i).GetComponent<SaveLoadGameButton>().saveSlotNumber_Text.text = "Q " + (i + 1);

            autoLoadPage.transform.GetChild(i).GetComponent<SaveLoadGameButton>().saveSlotNumber_Text.text = "A " + (i + 1);
            quickLoadPage.transform.GetChild(i).GetComponent<SaveLoadGameButton>().saveSlotNumber_Text.text = "Q " + (i + 1);
        }

        for (int i = -1; i >= -saveSlotsPerPage * 2; i--)
        {
            RefreshSaveLoadButton(i);
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

                currentSaveSlot.GetComponent<SaveLoadGameButton>().saveSlotNumber_Text.text = "" + (i * saveSlotsPerPage + j + 1);
                currentLoadSlot.GetComponent<SaveLoadGameButton>().saveSlotNumber_Text.text = "" + (i * saveSlotsPerPage + j + 1);

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

    private void RefreshSaveLoadButton(int saveSlotNum)
    {
        print("saveslot num:" + saveSlotNum);
        SaveLoadGameButton saveGameBtn;
        SaveLoadGameButton loadGameBtn;

        int saveSlotsPerPage = saveLoadPagePrefab.transform.childCount;

        if (saveSlotNum < 0)
        {
            // AUTO SAVE/LOAD SLOTS
            if (saveSlotNum >= -saveSlotsPerPage)
            {
                saveGameBtn = savePageList[0].transform.GetChild(-saveSlotNum - 1).GetComponent<SaveLoadGameButton>();
                loadGameBtn = loadPageList[0].transform.GetChild(-saveSlotNum - 1).GetComponent<SaveLoadGameButton>();
            }
            else if (saveSlotNum >= -saveSlotsPerPage * 2)
            {
                saveGameBtn = savePageList[1].transform.GetChild(-saveSlotNum - 1 - saveSlotsPerPage).GetComponent<SaveLoadGameButton>();
                loadGameBtn = loadPageList[1].transform.GetChild(-saveSlotNum - 1 - saveSlotsPerPage).GetComponent<SaveLoadGameButton>();
            }
            else
            {
                Debug.Log("Not within Auto/Quick Save Index Bounds!");
                return;
            }
        }
        // NORMAL SAVE/LOAD SLOTS
        else
        {
            int pageIndex = (saveSlotNum -1) / saveSlotsPerPage;
            int slotIndex = (saveSlotNum -1) % saveSlotsPerPage;

            saveGameBtn = savePageList[pageIndex].transform.GetChild(slotIndex).GetComponent<SaveLoadGameButton>();
            loadGameBtn = loadPageList[pageIndex].transform.GetChild(slotIndex).GetComponent<SaveLoadGameButton>();
        }


        SaveData data = GameSingleton.instance.gameStateManager.LoadSaveID(saveSlotNum);

        // If Empty Save
        if (data == null)
        {
            saveGameBtn.saveChapterName_Text.gameObject.SetActive(false);
            saveGameBtn.saveDescription_Text.gameObject.SetActive(false);
            saveGameBtn.saveTimeStamp_Text.gameObject.SetActive(false);

            saveGameBtn.empty_Text.gameObject.SetActive(true);

            loadGameBtn.saveChapterName_Text.gameObject.SetActive(false);
            loadGameBtn.saveDescription_Text.gameObject.SetActive(false);
            loadGameBtn.saveTimeStamp_Text.gameObject.SetActive(false);

            loadGameBtn.empty_Text.gameObject.SetActive(true);

            return;
        }
        saveGameBtn.saveChapterName_Text.gameObject.SetActive(true);
        saveGameBtn.saveDescription_Text.gameObject.SetActive(true);
        saveGameBtn.saveTimeStamp_Text.gameObject.SetActive(true);

        saveGameBtn.empty_Text.gameObject.SetActive(false);

        loadGameBtn.saveChapterName_Text.gameObject.SetActive(true);
        loadGameBtn.saveDescription_Text.gameObject.SetActive(true);
        loadGameBtn.saveTimeStamp_Text.gameObject.SetActive(true);

        loadGameBtn.empty_Text.gameObject.SetActive(false);


        Sprite screenshot = GameSingleton.instance.gameStateManager.GetSaveScreenshotSprite(saveGameBtn.saveLoadSlotNumber);

        //if (screenshot != null)
        //    loadSpritePreview.sprite = screenshot;
        //else
        //    loadSpritePreview.sprite = emptySaveSprite;

    }
}
