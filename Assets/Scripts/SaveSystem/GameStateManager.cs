using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Ink.Parsed;

public class GameStateManager : MonoBehaviour
{
   public SaveData currentSave;

    [Serializable]
    public class SaveData
    {
        public int SaveID;
        public List<string> charOnScreen;
        public List<string> charMood;
        public List<SerializableVector3> charPosition;
        public string InkStoryState;
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

    //private SaveData CreateSaveGameObject(int id)
    //{
    //    return new SaveData
    //    {
    //        SaveID = id,
    //        charOnScreen = new List<string>(),
    //        charMood = new List<string>(),
    //        charPosition = new List<SerializableVector3>(),
    //        InkStoryState = GameSingleton.instance.dialogueManager.GetStoryState(),
    //    };
    //}

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
    }

    public void SaveGame(int saveID)
    {
        currentSave.SaveID = saveID;
        
        foreach(Character c in GameSingleton.instance.characterManager.characters)
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

        var bf = new BinaryFormatter();
        var savePath = Application.persistentDataPath + GlobalVariables.saveFileBaseName + saveID.ToString() + GlobalVariables.saveFileExtension;
        FileStream file = File.Create(savePath); // creates a file at the specified location
        bf.Serialize(file, currentSave); // writes the content of SaveData object into the file
        file.Close();

        Debug.Log("Game saved");

        // Save should include sprites, positions, animations, background sprite, variables, flags.

    }



    public void LoadGame(int saveFileNumber)
    {
        string SavePath = Application.persistentDataPath + GlobalVariables.saveFileBaseName + saveFileNumber.ToString() + GlobalVariables.saveFileExtension;

        if(File.Exists(SavePath))
        {
            BinaryFormatter bf = new BinaryFormatter();

            FileStream file = File.Open(SavePath, FileMode.Open);

            file.Position = 0;

            currentSave = (SaveData)bf.Deserialize(file);
            file.Close();

            GameSingleton.instance.sceneLoaderManager.LoadWindow1();
            GameSingleton.instance.dialogueManager.LoadState(currentSave.InkStoryState);

            for(int i = 0; i < currentSave.charOnScreen.Count; i++)
            {
                GameSingleton.instance.characterManager.ShowCharacter(currentSave.charOnScreen[i], currentSave.charMood[i], currentSave.charPosition[i].ToVector3());
            }

            Debug.Log("Game loaded");

        }
        else
        {
            Debug.Log("No save file found");
        }
    }

    public void ExitGame()
    {
    }

    public void LoadMainMenu()
    {

    }

    public void LoadMainGameWindow()
    {

    }

    public void LoadSettingsMenu()
    {

    }
}