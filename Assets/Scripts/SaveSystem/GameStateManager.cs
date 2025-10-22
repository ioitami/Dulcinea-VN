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
        public int SaveID;
        public string InkStoryState;
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
        var bf = new BinaryFormatter();

        var savePath = Application.persistentDataPath + GlobalVariables.saveFileBaseName + saveID.ToString() + GlobalVariables.saveFileExtension;

        FileStream file = File.Create(savePath); // creates a file at the specified location

        bf.Serialize(file, save); // writes the content of SaveData object into the file

        file.Close();

        Debug.Log("Game saved");

        // Save should include sprites, positions, animations, background sprite, variables, flags.

    }

    private SaveData CreateSaveGameObject(int id)
    {
        return new SaveData
        {
            SaveID = id,
            InkStoryState = GameSingleton.instance.dialogueManager.GetStoryState(),
        };
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