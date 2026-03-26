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
using System.Collections;
using Unity.VisualScripting;
//using static System.Net.Mime.MediaTypeNames;

public class GameStateManager : MonoBehaviour
{
    public SaveData currentSave;
    //public ReadLineTracker readLineSave;
    public string screenshotBase64_saved;

    [Serializable]
    public class SaveData
    {
        public int saveID;
        public string chapterName;
        [HideInInspector]
        public string saveTimeStamp;
        [HideInInspector]
        public string screenshotBase64;

        public List<string> charOnScreen;
        public List<string> charMood;
        public List<SerializableVector3> charPosition;

        [HideInInspector]
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



    // =========================================================================================
    // GAME STATE MANAGER STUFF BELOW HERE
    // =========================================================================================

    // GAME STATE STARTS HERE ON EXE OPEN
    private void Awake()
    {
        GameSingleton.instance.cameraManager.DisableAllCameras();

    }
    // ==================================
    public void StartNewGame()
    {


        //GameSingleton.instance.gameStateManager.readLineSave.LoadReadLinesFile();
    }

    public void SaveGame(int saveID)
    {
        //currentSave.saveID = saveID;
        //currentSave.charOnScreen.Clear();
        //currentSave.charMood.Clear();
        //currentSave.charPosition.Clear();
        //currentSave.saveID = saveID;
        //currentSave.chapterName = 0.ToString(); // CHANGE THIS LATER
        //currentSave.saveTimeStamp = System.DateTime.Now.ToString();

        //foreach (Character c in GameSingleton.instance.characterManager.characters)
        //{

        //    if (c.ingameContainerObj.activeSelf == true)
        //    {
        //        currentSave.charOnScreen.Add(c.ingameContainerObj.name.Replace("_Container", ""));

        //        if (string.IsNullOrEmpty(c.currentMood.moodName) == false)
        //        {
        //            currentSave.charMood.Add(c.currentMood.moodName);
        //        }
        //        else
        //        {
        //            currentSave.charMood.Add(c.moods[0].moodName);
        //        }

        //        // If any animations are playing, skip them to the end before saving position
        //        if (GameSingleton.instance.spriteAnimationManager.IsAnyAnimationPlaying())
        //        {
        //            GameSingleton.instance.spriteAnimationManager.SkipAllToEnd();
        //        }

        //        currentSave.charPosition.Add(new SerializableVector3(c.ingameContainerObj.transform.localPosition));

        //    }
        //}

        //currentSave.InkStoryState = GameSingleton.instance.dialogueManager.GetStoryState();

        //string savePath = Application.persistentDataPath + GlobalVariables.saveFileBaseName + saveID.ToString() + GlobalVariables.saveFileExtension;

        //try
        //{
        //    FinalizeSave(saveID);
        //}
        //catch (System.Exception e)
        //{
        //    Debug.LogError($"Failed to save game: {e}");
        //}

        //readLineSave.LoadReadLinesFile();
        //readLineSave.SaveReadLinesFile();

        //// Save should include sprites, positions, animations, background sprite, variables, flags.

    }

    private void FinalizeSave(int saveID)
    {
        StartCoroutine(CaptureScreenshotAsBase64((base64) =>
        {
            currentSave.screenshotBase64 = base64;

            string savePath = Application.persistentDataPath + GlobalVariables.saveFileBaseName + saveID.ToString() + GlobalVariables.saveFileExtension;
            string json = JsonUtility.ToJson(currentSave, true);
            File.WriteAllText(savePath, json);
            Debug.Log($"Game saved as JSON to {savePath}");
        }));
    }

    public void LoadGame(int saveFileNumber)
    {
        string savePath = Application.persistentDataPath + GlobalVariables.saveFileBaseName + saveFileNumber.ToString() + GlobalVariables.saveFileExtension;
    }

    private List<Camera> saveCamList;
    public Camera targetScreenshotCamera;
    public IEnumerator CaptureScreenshotAsBase64(System.Action<string> onComplete)
    {
        yield return new WaitForEndOfFrame();

        int width = Screen.width;
        int height = Screen.height;

        // Create a new RenderTexture with the desired dimensions
        RenderTexture renderTexture = new RenderTexture(width, height, 16);
        // Set the target camera to render into this new texture
        targetScreenshotCamera.targetTexture = renderTexture;

        // This is only needed if the camera is otherwise disabled or not rendering automatically
        // If the camera is active and rendering, skip this manual Render() call.
        targetScreenshotCamera.Render();

        // Set the active RenderTexture to the one we just rendered into
        RenderTexture.active = renderTexture;

        // Create a new Texture2D to store the pixel data
        Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGBA32, false);

        // Read the pixels from the active RenderTexture into the Texture2D
        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        // Apply the changes to the Texture2D
        screenShot.Apply();

        // Clean up: restore the camera's original target texture and the active render texture
        targetScreenshotCamera.targetTexture = null;
        RenderTexture.active = null;

        // Release the temporary RenderTexture from memory
        renderTexture.Release();
        Destroy(renderTexture);


        byte[] bytes = screenShot.EncodeToJPG(50);
        UnityEngine.Object.Destroy(screenShot);

        string base64 = System.Convert.ToBase64String(bytes);

        onComplete?.Invoke(base64);

    }


    public Sprite GetLoadedScreenshotSprite()
    {
        if (string.IsNullOrEmpty(currentSave.screenshotBase64)) return null;


        byte[] imageData = System.Convert.FromBase64String(currentSave.screenshotBase64);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(imageData);
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
    }

    public Sprite LoadScreenshotSprite(int saveID)
    {
        string savePath = Application.persistentDataPath + GlobalVariables.saveFileBaseName + saveID.ToString() + GlobalVariables.saveFileExtension;

        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            string screenShotData = JsonUtility.FromJson<SaveData>(json).screenshotBase64;

            byte[] imageData = System.Convert.FromBase64String(screenShotData);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(imageData);

            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
        else
        {
            // TODO: IF NO SAVE FILE FOUND, RETURN A DEFAULT IMAGE
            return null;
        }
    }


    private void OnApplicationQuit()
    {
        //readLineSave.SaveReadLinesFile();
    }
}