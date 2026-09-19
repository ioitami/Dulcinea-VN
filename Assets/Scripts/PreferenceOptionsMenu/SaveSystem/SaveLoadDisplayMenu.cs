using Ink.Parsed;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SaveLoadDisplayMenu : MonoBehaviour
{
    public int lastVisitedPage = 2;
    public int numPages = GlobalVariables.totalSavePageNumber;
    [Space]
    public GameObject saveLoadPagePrefab;
    public GameObject saveLoadPageButtonPrefab;
    [Space]
    public GameObject savePageHeaderText;
    public GameObject loadPageHeaderText;
    [Space]
    public Transform savePageList_Parent;
    public Transform loadPageList_Parent;
    public Transform savePage_Parent;
    public Transform loadPage_Parent;
    [Space]
    public Image savePreview_Screen1;
    public Image savePreview_Screen2;
    public Sprite emptySavePreviewSprite;
    [Space]
    public List<GameObject> savePageList;
    public List<GameObject> loadPageList;

    private void OnEnable()
    {
        
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
                RefreshSaveLoadButton(i * saveSlotsPerPage + j + 1);
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

        AutoSavePageBtn.GetComponentInChildren<TextMeshProUGUI>().text = "A";
        AutoLoadPageBtn.GetComponentInChildren<TextMeshProUGUI>().text = "A";

        QuickSavePageBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Q";
        QuickLoadPageBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Q";

        AutoSavePageBtn.GetComponent<Button>().onClick.RemoveAllListeners();
        AutoSavePageBtn.GetComponent<Button>().onClick.AddListener(() => OpenSavePage(0));
        AutoLoadPageBtn.GetComponent<Button>().onClick.RemoveAllListeners();
        AutoLoadPageBtn.GetComponent<Button>().onClick.AddListener(() => OpenLoadPage(0));

        QuickSavePageBtn.GetComponent<Button>().onClick.RemoveAllListeners();
        QuickSavePageBtn.GetComponent<Button>().onClick.AddListener(() => OpenSavePage(1));
        QuickLoadPageBtn.GetComponent<Button>().onClick.RemoveAllListeners();
        QuickLoadPageBtn.GetComponent<Button>().onClick.AddListener(() => OpenLoadPage(1));

        for (int i = 0; i < numPages; i++)
        {
            int pageIndex = i;

            GameObject savePageBtn = Instantiate(saveLoadPageButtonPrefab, savePageList_Parent);
            GameObject loadPageBtn = Instantiate(saveLoadPageButtonPrefab, loadPageList_Parent);

            savePageBtn.GetComponentInChildren<TextMeshProUGUI>().text = (pageIndex + 1).ToString();
            loadPageBtn.GetComponentInChildren<TextMeshProUGUI>().text = (pageIndex + 1).ToString();

            savePageBtn.GetComponent<Button>().onClick.RemoveAllListeners();
            savePageBtn.GetComponent<Button>().onClick.AddListener(() => OpenSavePage(pageIndex + 2));
            loadPageBtn.GetComponent<Button>().onClick.RemoveAllListeners();
            loadPageBtn.GetComponent<Button>().onClick.AddListener(() => OpenLoadPage(pageIndex + 2));
        }

        OpenLastVisitedSavePage();
    }

    public void OpenSavePage(int pageNum)
    {
        savePageHeaderText.gameObject.SetActive(true);
        loadPageHeaderText.gameObject.SetActive(false);

        savePage_Parent.gameObject.SetActive(true);
        loadPage_Parent.gameObject.SetActive(false);

        foreach (GameObject savePage in savePageList)
        {
            savePage.SetActive(false);
        }

        savePageList_Parent.gameObject.SetActive(true);
        loadPageList_Parent.gameObject.SetActive(false);

        savePageList[pageNum].SetActive(true);
        lastVisitedPage = pageNum;
    }

    public void OpenLoadPage(int pageNum)
    {
        savePageHeaderText.gameObject.SetActive(false);
        loadPageHeaderText.gameObject.SetActive(true);

        savePage_Parent.gameObject.SetActive(false);
        loadPage_Parent.gameObject.SetActive(true);

        foreach (GameObject loadPage in loadPageList)
        {
            loadPage.SetActive(false);
        }

        loadPageList_Parent.gameObject.SetActive(true);
        savePageList_Parent.gameObject.SetActive(false);

        loadPageList[pageNum].SetActive(true);
        lastVisitedPage = pageNum;
    }

    public void OpenLastVisitedSavePage()
    {
        savePageHeaderText.gameObject.SetActive(true);
        loadPageHeaderText.gameObject.SetActive(false);

        savePage_Parent.gameObject.SetActive(true);
        loadPage_Parent.gameObject.SetActive(false);

        foreach (GameObject savePage in savePageList)
        {
            savePage.SetActive(false);
        }

        savePageList_Parent.gameObject.SetActive(true);
        loadPageList_Parent.gameObject.SetActive(false);

        savePageList[lastVisitedPage].SetActive(true);
    }

    public void OpenLastVisitedLoadPage() 
    {
        savePageHeaderText.gameObject.SetActive(false);
        loadPageHeaderText.gameObject.SetActive(true);

        savePage_Parent.gameObject.SetActive(false);
        loadPage_Parent.gameObject.SetActive(true);

        foreach (GameObject loadPage in loadPageList)
        {
            loadPage.SetActive(false);
        }

        loadPageList_Parent.gameObject.SetActive(true);
        savePageList_Parent.gameObject.SetActive(false);

        loadPageList[lastVisitedPage].SetActive(true);
    }

    public void HideAllPages()
    {
        savePageHeaderText.gameObject.SetActive(false);
        loadPageHeaderText.gameObject.SetActive(false);

        savePage_Parent.gameObject.SetActive(false);
        loadPage_Parent.gameObject.SetActive(false);

        foreach (GameObject savePage in savePageList)
        {
            savePage.SetActive(false);
        }
        foreach (GameObject loadPage in loadPageList)
        {
            loadPage.SetActive(false);
        }

        savePageList_Parent.gameObject.SetActive(false);
        loadPageList_Parent.gameObject.SetActive(false);
    }

    private void RefreshSaveLoadButton(int saveSlotNum)
    {
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
            // QUICK SAVE/LOAD SLOTS
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
            int pageIndex = (saveSlotNum -1) / saveSlotsPerPage + 2; // savePageList[0]=Auto, [1]=Quick, numbered pages start at [2]
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


            saveGameBtn.hasSave = false;
            loadGameBtn.hasSave = false;

            // Destroy old thumbnails to prevent overuse of memory storage
            DestroyDynamicThumbnail(saveGameBtn.thumbnailImage);

            saveGameBtn.thumbnailImage = emptySavePreviewSprite;
            loadGameBtn.thumbnailImage = emptySavePreviewSprite;

            saveGameBtn.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
            saveGameBtn.GetComponentInChildren<Button>().onClick.AddListener(() => {
                GameSingleton.instance.gameStateManager.Save(saveGameBtn.saveLoadSlotNumber, (savedData) => {
                    RefreshSaveLoadButton(saveGameBtn.saveLoadSlotNumber);
                    savePreview_Screen1.sprite = saveGameBtn.thumbnailImage;
                });
            });

            SetupHoverPreview(saveGameBtn.GetComponentInChildren<EventTrigger>(), saveGameBtn.thumbnailImage);
            SetupHoverPreview(loadGameBtn.GetComponentInChildren<EventTrigger>(), loadGameBtn.thumbnailImage);

            return;
        }
        else
        {
            saveGameBtn.hasSave = true;
            loadGameBtn.hasSave = true;

            saveGameBtn.saveChapterName_Text.gameObject.SetActive(true);
            saveGameBtn.saveDescription_Text.gameObject.SetActive(true);
            saveGameBtn.saveTimeStamp_Text.gameObject.SetActive(true);
            saveGameBtn.empty_Text.gameObject.SetActive(false);

            loadGameBtn.saveChapterName_Text.gameObject.SetActive(true);
            loadGameBtn.saveDescription_Text.gameObject.SetActive(true);
            loadGameBtn.saveTimeStamp_Text.gameObject.SetActive(true);
            loadGameBtn.empty_Text.gameObject.SetActive(false);

            saveGameBtn.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
            saveGameBtn.GetComponentInChildren<Button>().onClick.AddListener(() => {
                GameSingleton.instance.gameStateManager.Save(saveGameBtn.saveLoadSlotNumber, (savedData) => {
                    RefreshSaveLoadButton(saveGameBtn.saveLoadSlotNumber);
                    savePreview_Screen1.sprite = saveGameBtn.thumbnailImage;
                });
            });

            loadGameBtn.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
            loadGameBtn.GetComponentInChildren<Button>().onClick.AddListener(() => {
                GameSingleton.instance.gameStateManager.LoadGame(loadGameBtn.saveLoadSlotNumber);
            });

            saveGameBtn.saveChapterName_Text.text = data.chapterName;
            saveGameBtn.saveDescription_Text.text = data.description;
            saveGameBtn.saveTimeStamp_Text.text = data.saveTimeStamp;

            loadGameBtn.saveChapterName_Text.text = data.chapterName;
            loadGameBtn.saveDescription_Text.text = data.description;
            loadGameBtn.saveTimeStamp_Text.text = data.saveTimeStamp;
            
            // Destroy old thumbnails to prevent overuse of memory storage
            DestroyDynamicThumbnail(saveGameBtn.thumbnailImage);

            Sprite screenshot = GameSingleton.instance.gameStateManager.GetSaveScreenshotSprite(saveGameBtn.saveLoadSlotNumber);
            saveGameBtn.thumbnailImage = screenshot;
            loadGameBtn.thumbnailImage = screenshot;

            SetupHoverPreview(saveGameBtn.GetComponentInChildren<EventTrigger>(), saveGameBtn.thumbnailImage);
            SetupHoverPreview(loadGameBtn.GetComponentInChildren<EventTrigger>(), loadGameBtn.thumbnailImage);
        }


    }

    private void SetupHoverPreview(EventTrigger trigger, Sprite thumbnail)
    {
        if (trigger == null) return;

        EventTrigger.Entry enterEntry = trigger.triggers.Find(e => e.eventID == EventTriggerType.PointerEnter);
        if (enterEntry == null)
        {
            enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            trigger.triggers.Add(enterEntry);
        }
        enterEntry.callback.RemoveAllListeners();
        enterEntry.callback.AddListener((data) => { savePreview_Screen1.sprite = thumbnail; });

        EventTrigger.Entry exitEntry = trigger.triggers.Find(e => e.eventID == EventTriggerType.PointerExit);
        if (exitEntry == null)
        {
            exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            trigger.triggers.Add(exitEntry);
        }
        exitEntry.callback.RemoveAllListeners();
        exitEntry.callback.AddListener((data) => { savePreview_Screen1.sprite = emptySavePreviewSprite; });
    }

    private void DestroyDynamicThumbnail(Sprite sprite)
    {
        if (sprite == null) return;
        if (sprite == emptySavePreviewSprite) return; // never destroy the default proj asset

        if (sprite.texture != null)
        {
            Destroy(sprite.texture);
        }

        Destroy(sprite);
    }
}
