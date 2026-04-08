using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Overlays;
using UnityEngine;


public class GameStateManager : MonoBehaviour
{
    [Header("Screenshot Settings")]
    public int screenshotWidth = 320;
    public int screenshotHeight = 180;

    private const string SaveFolder = "Saves";
    private const string SavePrefix = "save_";
    private const string SaveExtension = ".json";

    private string SaveDirectory => Path.Combine(Application.persistentDataPath, SaveFolder);

    private void Awake()
    {
        if (!Directory.Exists(SaveDirectory))
            Directory.CreateDirectory(SaveDirectory);
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
            Debug.LogWarning($"[GameStateManager] No save found with ID {saveID}.");
            return null;
        }

        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        Debug.Log($"[GameStateManager] Loaded save {saveID}.");
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

    // ===========================
    // Save Routine
    // ===========================

    private IEnumerator SaveRoutine(int saveID, Action<SaveData> onComplete)
    {
        yield return new WaitForEndOfFrame();

        SaveData data = new SaveData();

        data.saveID = saveID;
        data.saveTimeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        data.screenshotBase64 = CaptureScreenshot();

        CollectDialogueData(data);
        CollectCharacterData(data);
        WriteToFile(data);

        Debug.Log($"[GameStateManager] Saved slot {saveID} — chapter: {data.chapterName}.");
        onComplete?.Invoke(data);
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
        Camera mainCam = Camera.main;

        if (mainCam == null)
        {
            Debug.LogWarning("[GameStateManager] No main camera found for screenshot.");
            return "";
        }

        RenderTexture rt = new RenderTexture(screenshotWidth, screenshotHeight, 24);
        RenderTexture previousTarget = mainCam.targetTexture;

        mainCam.targetTexture = rt;
        mainCam.Render();
        mainCam.targetTexture = previousTarget;

        RenderTexture.active = rt;
        Texture2D screenshot = new Texture2D(screenshotWidth, screenshotHeight, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, screenshotWidth, screenshotHeight), 0, 0);
        screenshot.Apply();
        RenderTexture.active = null;

        byte[] bytes = screenshot.EncodeToJPG(60);
        string base64 = Convert.ToBase64String(bytes);

        Destroy(rt);
        Destroy(screenshot);

        return base64;
    }

    // ===========================
    // File IO
    // ===========================

    private void WriteToFile(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSavePath(data.saveID), json);
    }

    private string GetSavePath(int saveID)
    {
        return Path.Combine(SaveDirectory, SavePrefix + saveID + SaveExtension);
    }
}