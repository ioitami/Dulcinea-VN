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

    private SaveData CreateSaveGameObject(int id)
    {
        return new SaveData
        {
            SaveID = id,
            charOnScreen = new List<string>(),
            charMood = new List<string>(),
            charPosition = new List<SerializableVector3>(),
            InkStoryState = GameSingleton.instance.dialogueManager.GetStoryState(),
        };
    }

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
        SaveData save = CreateSaveGameObject(saveID);

        save.SaveID = saveID;
        
        foreach(Character c in GameSingleton.instance.characterManager.characters)
        {

            if (c.ingameContainerObj.activeSelf == true)
            {
                //save.charOnScreen.Add(c.ingameContainerObj.name.Replace("_Container", ""));
                save.charOnScreen.Add("Dulcinea");

                if (string.IsNullOrEmpty(c.currentMood.moodName) == false)
                {
                    save.charMood.Add(c.currentMood.moodName);
                }
                else
                {
                    save.charMood.Add(c.moods[0].moodName);
                }

                save.charPosition.Add(new SerializableVector3(c.ingameContainerObj.transform.localPosition));
            }
        }

        var bf = new BinaryFormatter();
        var savePath = Application.persistentDataPath + GlobalVariables.saveFileBaseName + saveID.ToString() + GlobalVariables.saveFileExtension;
        FileStream file = File.Create(savePath); // creates a file at the specified location
        bf.Serialize(file, save); // writes the content of SaveData object into the file
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

            SaveData save = (SaveData)bf.Deserialize(file);
            file.Close();

            GameSingleton.instance.sceneLoaderManager.LoadWindow1();
            GameSingleton.instance.dialogueManager.LoadState(save.InkStoryState);

            for(int i = 0; i < save.charOnScreen.Count; i++)
            {
                GameSingleton.instance.characterManager.ShowCharacter(save.charOnScreen[i], save.charMood[i], save.charPosition[i].ToVector3());
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