using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Ink.Parsed;
using Unity.XR.OpenVR;

public class GameStateManager : MonoBehaviour
{
    public SaveData currentSave;
    public ReadLineTracker readLineSave;

    [Serializable]
    public class SaveData
    {
        public int SaveID;
        public List<string> charOnScreen;
        public List<string> charMood;
        public List<SerializableVector3> charPosition;
        public string InkStoryState;

        // Other flags and whatever else to save game state
    }

    [System.Serializable]
    public class SerializableVector3
    {
        public float x, y, z;

        public SerializableVector3(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }

    [Serializable]
    public class ReadLineTracker
    {
        public HashSet<string> readLineIDs = new HashSet<string>();

        private int linesSinceLastSave = 0;
        private const int saveThreshold = 10; // save every 10 lines

        // Mark a line as read
        public void MarkAsRead(string id)
        {
            if (string.IsNullOrEmpty(id) == false)
            {
                readLineIDs.Add(id);


                linesSinceLastSave++;
                if (linesSinceLastSave >= saveThreshold)
                {
                    SaveReadLinesFile();
                    linesSinceLastSave = 0;
                }
            }

        }

        // Check if a line has been read
        public bool HasBeenRead(string id)
        {
            return !string.IsNullOrEmpty(id) && readLineIDs.Contains(id);
        }

        // Save to independent JSON file
        public void SaveReadLinesFile()
        {
            string savePath = Application.persistentDataPath + GlobalVariables.readLines_global_saveFileName + GlobalVariables.readLines_global_saveFileExtension;

            try
            {
                var wrapper = new Wrapper { readLineIDs = new List<string>(readLineIDs) };
                string json = JsonUtility.ToJson(wrapper, true);
                File.WriteAllText(savePath, json);
#if UNITY_EDITOR
                Debug.Log($"[ReadLineTracker] Saved {readLineIDs.Count} read lines to {savePath}");
#endif
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ReadLineTracker] Failed to save LoggedPages: {e}");
            }
        }

        // Load from JSON file
        public void LoadReadLinesFile()
        {
            string savePath = Application.persistentDataPath + GlobalVariables.readLines_global_saveFileName + GlobalVariables.readLines_global_saveFileExtension;

            try
            {
                if (File.Exists(savePath))
                {
                    string json = File.ReadAllText(savePath);
                    var wrapper = JsonUtility.FromJson<Wrapper>(json);
                    readLineIDs = new HashSet<string>(wrapper.readLineIDs);
#if UNITY_EDITOR
                    Debug.Log($"[ReadLineTracker] Loaded {readLineIDs.Count} read lines from {savePath}");
#endif
                }
                else
                {
                    readLineIDs = new HashSet<string>();
#if UNITY_EDITOR
                    Debug.Log("[ReadLineTracker] No LoggedPages file found — starting fresh.");
#endif
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ReadLineTracker] Failed to load LoggedPages: {e}");
                readLineIDs = new HashSet<string>();
            }
        }

        // Delete saved data (useful for testing or resets)
        public void ClearReadLinesFile()
        {
            string savePath = Application.persistentDataPath + GlobalVariables.readLines_global_saveFileName + GlobalVariables.readLines_global_saveFileExtension;

            readLineIDs.Clear();
            if (File.Exists(savePath))
                File.Delete(savePath);
#if UNITY_EDITOR
            Debug.Log("[ReadLineTracker] LoggedPages file cleared.");
#endif
        }

        [System.Serializable]
        private class Wrapper
        {
            public List<string> readLineIDs;
        }
    }








    // =========================================================================================
    // GAME STATE MANAGER STUFF BELOW HERE
    // =========================================================================================

    // GAME STATE STARTS HERE ON EXE OPEN
    private void Awake()
    {
        GameSingleton.instance.cameraManager.DisableAllCameras();
        GameSingleton.instance.sceneLoaderManager.LoadMainMenu();
    }
    // ==================================
    public void StartNewGame()
    {
        GameSingleton.instance.dialogueManager.ResetStory();
        GameSingleton.instance.dialogueManager.StartStory(instant:false);

        GameSingleton.instance.gameStateManager.readLineSave.LoadReadLinesFile();
    }

    public void SaveGame(int saveID)
    {
        currentSave.SaveID = saveID;
        currentSave.charOnScreen.Clear();
        currentSave.charMood.Clear();
        currentSave.charPosition.Clear();

        foreach (Character c in GameSingleton.instance.characterManager.characters)
        {

            if (c.ingameContainerObj.activeSelf == true)
            {
                currentSave.charOnScreen.Add(c.ingameContainerObj.name.Replace("_Container", ""));

                if (string.IsNullOrEmpty(c.currentMood.moodName) == false)
                {
                    currentSave.charMood.Add(c.currentMood.moodName);
                }
                else
                {
                    currentSave.charMood.Add(c.moods[0].moodName);
                }

                // If any animations are playing, skip them to the end before saving position
                if (GameSingleton.instance.spriteAnimationManager.IsAnyAnimationPlaying())
                {
                    GameSingleton.instance.spriteAnimationManager.SkipAllToEnd();
                }

                currentSave.charPosition.Add(new SerializableVector3(c.ingameContainerObj.transform.localPosition));

            }
        }

        currentSave.InkStoryState = GameSingleton.instance.dialogueManager.GetStoryState();

        string savePath = Application.persistentDataPath + GlobalVariables.saveFileBaseName + saveID.ToString() + GlobalVariables.saveFileExtension;

        try
        {
            string json = JsonUtility.ToJson(currentSave, true);
            File.WriteAllText(savePath, json);
            Debug.Log($"Game saved as JSON to {savePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save game: {e}");
        }

        readLineSave.SaveReadLinesFile();

        // Save should include sprites, positions, animations, background sprite, variables, flags.

    }



    public void LoadGame(int saveFileNumber)
    {
        string savePath = Application.persistentDataPath + GlobalVariables.saveFileBaseName + saveFileNumber.ToString() + GlobalVariables.saveFileExtension;

        if(File.Exists(savePath))
        {
            try
            {
                string json = File.ReadAllText(savePath);
                currentSave = JsonUtility.FromJson<SaveData>(json);

                // Load game data
                GameSingleton.instance.sceneLoaderManager.LoadWindow1();
                GameSingleton.instance.dialogueManager.LoadState(currentSave.InkStoryState);

                for (int i = 0; i < currentSave.charOnScreen.Count; i++)
                {
                    GameSingleton.instance.characterManager.ShowCharacter(currentSave.charOnScreen[i], currentSave.charMood[i], currentSave.charPosition[i].ToVector3());
                }

                Debug.Log($"Game loaded from JSON: {savePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load game: {e}");
            }

            readLineSave.LoadReadLinesFile();
        }
        else
        {
            Debug.Log("No save file found");
        }
    }


    private void OnApplicationQuit()
    {
        readLineSave.SaveReadLinesFile();
    }
}