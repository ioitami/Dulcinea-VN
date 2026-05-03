using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using UnityEditor.Overlays;
using UnityEngine;


public class GameStateManager : MonoBehaviour
{
    [Header("Screenshot Settings")]
    public int screenshotWidth = 320;
    public int screenshotHeight = 180;

    private string SaveFolder = GlobalVariables.saveFileFolderName;
    private string SavePrefix = GlobalVariables.saveFileBaseName;
    private string SaveExtension = GlobalVariables.saveFileExtension;

    [Header("Visited Blocks")]
    private const string VisitedBlocksFile = "visited_blocks.json";
    private string VisitedBlocksPath => Path.Combine(Application.streamingAssetsPath, VisitedBlocksFile);
    
    public HashSet<string> visitedBlockIDs = new HashSet<string>();

    private string SaveDirectory => Path.Combine(Application.persistentDataPath, SaveFolder);

    private void Awake()
    {

        if (!Directory.Exists(SaveDirectory))
            Directory.CreateDirectory(SaveDirectory);

        // Ensure StreamingAssets directory exists
        if (!Directory.Exists(Application.streamingAssetsPath))
            Directory.CreateDirectory(Application.streamingAssetsPath);

        LoadVisitedBlocks();
    }

    // ===========================
    // Public API
    // ===========================

    public void Save(int saveID, Action<SaveData> onComplete = null)
    {
        StartCoroutine(SaveRoutine(saveID, onComplete));
    }

    public SaveData Load(int saveID)
    {
        string path = GetSavePath(saveID);

        if (!File.Exists(path))
        {
            Debug.Log($"[GameStateManager] No save found with ID {saveID}.");
            return null;
        }

        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        return data;
    }

    public void DeleteSave(int saveID)
    {
        string path = GetSavePath(saveID);

        if (!File.Exists(path))
        {
            Debug.LogWarning($"[GameStateManager] No save found with ID {saveID} to delete.");
            return;
        }

        File.Delete(path);
        Debug.Log($"[GameStateManager] Deleted save {saveID}.");
    }

    public bool SaveExists(int saveID)
    {
        return File.Exists(GetSavePath(saveID));
    }

    public List<SaveData> LoadAllSaves()
    {
        List<SaveData> saves = new List<SaveData>();

        string[] files = Directory.GetFiles(SaveDirectory, SavePrefix + "*" + SaveExtension);

        foreach (string file in files)
        {
            string json = File.ReadAllText(file);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            if (data != null)
                saves.Add(data);
        }

        saves.Sort((a, b) => a.saveID.CompareTo(b.saveID));
        return saves;
    }

    public void RegisterVisitedBlock(string blockID)
    {
        if (string.IsNullOrEmpty(blockID)) return;
        if (visitedBlockIDs.Contains(blockID)) return;

        visitedBlockIDs.Add(blockID);
        SaveVisitedBlocks();
    }

    public bool HasVisitedBlock(string blockID)
    {
        if (string.IsNullOrEmpty(blockID)) return false;
        return visitedBlockIDs.Contains(blockID);
    }

    public void ClearVisitedBlocks()
    {
        visitedBlockIDs.Clear();
        SaveVisitedBlocks();
    }

    private void SaveVisitedBlocks()
    {
        VisitedBlocksData data = new VisitedBlocksData();
        data.blockIDs = new List<string>(visitedBlockIDs);
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(VisitedBlocksPath, json);
    }

    private void LoadVisitedBlocks()
    {
        if (!File.Exists(VisitedBlocksPath))
        {
            visitedBlockIDs = new HashSet<string>();
            return;
        }

        string json = File.ReadAllText(VisitedBlocksPath);
        VisitedBlocksData data = JsonUtility.FromJson<VisitedBlocksData>(json);

        if (data != null && data.blockIDs != null)
            visitedBlockIDs = new HashSet<string>(data.blockIDs);
        else
            visitedBlockIDs = new HashSet<string>();
    }

    // ===========================
    // Save Routine
    // ===========================

    private string pendingScreenshotBase64 = "";

    public void CaptureScreenshotForSave()
    {
        StartCoroutine(CaptureScreenshotRoutine());
    }

    public IEnumerator CaptureScreenshotRoutine()
    {
        yield return new WaitForEndOfFrame();

        try
        {
            Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            screenshot.Apply();

            Texture2D thumbnail = ScaleTexture(screenshot, screenshotWidth, screenshotHeight);
            Destroy(screenshot);

            byte[] bytes = thumbnail.EncodeToJPG(60);
            pendingScreenshotBase64 = Convert.ToBase64String(bytes);

            Destroy(thumbnail);
        }
        catch (Exception e)
        {
            Debug.LogError($"[GameStateManager] Screenshot capture failed: {e.Message}");
            pendingScreenshotBase64 = "";
        }
    }

    private IEnumerator SaveRoutine(int saveID, Action<SaveData> onComplete)
    {
        yield return new WaitForEndOfFrame();

        SaveData data = new SaveData();
        data.saveID = saveID;
        data.saveTimeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        data.screenshotBase64 = pendingScreenshotBase64;

        CollectDialogueData(data);
        CollectCharacterData(data);
        WriteToFile(data);

        Debug.Log($"[GameStateManager] Saved slot {saveID} — chapter: {data.chapterName}.");
        onComplete?.Invoke(data);
    }

    private GameObject GetOverlayCamera(int overlayCameraID)
    {
        return GameSingleton.instance.cameraManager.overlayCameraList[overlayCameraID].gameObject;
    }

    // ===========================
    // Data Collection
    // ===========================

    private void CollectDialogueData(SaveData data)
    {
        DialogueManager dialogueManager = GameSingleton.instance.dialogueManager;

        if (dialogueManager == null)
        {
            data.chapterName = "Unknown";
            data.description = "";
            data.dialogueGroupID = "";
            data.dialogueBlockID = "";
            return;
        }

        if (dialogueManager.currentBlock != null)
            data.chapterName = dialogueManager.currentBlock.ID;
        else
            data.chapterName = "Unknown";


        if (dialogueManager.currentBlock != null)
            data.description = dialogueManager.currentBlock.saveDescription;
        else
            data.description = "";


        if (dialogueManager.currentGroup != null)
            data.dialogueGroupID = dialogueManager.currentGroup.ID;
        else
            data.dialogueGroupID = "";


        if (dialogueManager.currentBlock != null)
            data.dialogueBlockID = dialogueManager.currentBlock.ID;
        else
            data.dialogueBlockID = "";
    }


    private void CollectCharacterData(SaveData data)
    {
        data.charactersOnScreen = new List<string>();
        data.charactersMood = new List<string>();
        data.charactersPosition = new List<SerializableVector3>();

        CharacterManager characterManager = GameSingleton.instance.characterManager;

        if (characterManager == null) return;

        foreach (Character character in characterManager.characters)
        {
            if (character.ingameContainerObj == null) continue;
            if (!character.ingameContainerObj.activeSelf) continue;

            data.charactersOnScreen.Add(character.characterName);
            data.charactersMood.Add(character.currentMood != null ? character.currentMood.moodName : "");
            data.charactersPosition.Add(new SerializableVector3(
                character.ingameContainerObj.transform.localPosition
            ));
        }
    }


    private string CaptureScreenshot()
    {
        try
        {
            Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            screenshot.Apply();

            Texture2D thumbnail = ScaleTexture(screenshot, screenshotWidth, screenshotHeight);
            Destroy(screenshot);

            byte[] bytes = thumbnail.EncodeToJPG(60);
            string base64 = Convert.ToBase64String(bytes);

            Destroy(thumbnail);

            return base64;
        }
        catch (Exception e)
        {
            Debug.LogError($"[GameStateManager] Screenshot failed: {e.Message}\n{e.StackTrace}");
            return "";
        }
    }

    public Sprite GetSaveScreenshotSprite(int saveID)
    {
        SaveData data = Load(saveID);

        if (data == null) return null;
        if (string.IsNullOrEmpty(data.screenshotBase64)) return null;

        byte[] bytes = Convert.FromBase64String(data.screenshotBase64);
        Texture2D texture = new Texture2D(screenshotWidth, screenshotHeight, TextureFormat.RGB24, false);
        texture.LoadImage(bytes);

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f)
        );

        return sprite;
    }

    private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        RenderTexture rt = RenderTexture.GetTemporary(targetWidth, targetHeight, 0);
        RenderTexture.active = rt;

        Graphics.Blit(source, rt);

        Texture2D result = new Texture2D(targetWidth, targetHeight, TextureFormat.RGB24, false);
        result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
        result.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        return result;
    }

    // ===========================
    // File IO
    // ===========================

    private void WriteToFile(SaveData data)
    {
        try
        {
            string path = GetSavePath(data.saveID);
            string json = JsonUtility.ToJson(data, true);

            Debug.Log($"[GameStateManager] Attempting to write to: {path}");
            Debug.Log($"[GameStateManager] Directory exists: {Directory.Exists(SaveDirectory)}");
            Debug.Log($"[GameStateManager] JSON length: {json.Length}");

            File.WriteAllText(path, json);

            Debug.Log($"[GameStateManager] File exists after write: {File.Exists(path)}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[GameStateManager] Failed to write save file: {e.Message}\n{e.StackTrace}");
        }
    }

    private string GetSavePath(int saveID)
    {
        return Path.Combine(SaveDirectory, SavePrefix + saveID + SaveExtension);
    }
}