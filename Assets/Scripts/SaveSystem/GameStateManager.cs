using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
public class GameStateManager : MonoBehaviour
{
   

    [Serializable]
    public class SaveData
    {
        public string InkStoryState;
    }

    // GAME STATE STARTS HERE ON EXE OPEN
    private void Awake()
    {
        GameSingleton.instance.sceneLoaderManager.LoadMainMenu();
    }
    // ==================================
    public void StartNewGame()
    {
        GameSingleton.instance.dialogueManager.ResetStory();
        GameSingleton.instance.dialogueManager.StartStory(instant:false);
    }

    public void SaveGame()
    {
        SaveData save = CreateSaveGameObject();
        var bf = new BinaryFormatter();

        var savePath = Application.persistentDataPath + "/savedata.save";

        FileStream file = File.Create(savePath); // creates a file at the specified location

        bf.Serialize(file, save); // writes the content of SaveData object into the file

        file.Close();

        Debug.Log("Game saved");

        // Save should include sprites, positions, animations, background sprite, variables, flags.

    }

    private SaveData CreateSaveGameObject()
    {
        return new SaveData
        {
            InkStoryState = GameSingleton.instance.dialogueManager.GetStoryState(),
        };
    }

    public void LoadGame()
    {
        // Here we will load data from a file and make it available to other managers
        var SavePath = Application.persistentDataPath + "/savedata.save";

        if(File.Exists(SavePath))
        {
            BinaryFormatter bf = new BinaryFormatter();

            FileStream file = File.Open(SavePath, FileMode.Open);

            file.Position = 0;

            SaveData save = (SaveData)bf.Deserialize(file);
            file.Close();

            GameSingleton.instance.dialogueManager.LoadState(save.InkStoryState);
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